using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace pkuManager.WinForms.Utilities;

public struct pkuTime
{
    public DateOnly? Date { get; private set; } //D
    public TimeOnly? Time { get; private set; } //T - Leapseconds not supported
    public TimeSpan? Offset { get; private set; } //O
    public string Duration { get; private set; } //P - Can't use TimeSpan as it removes Year/Month info

    public int LastDateComp { get; private set; }
    public int LastTimeComp { get; private set; }

    public static pkuTime? Parse(string pkuTimeStr)
    {
        pkuTime pt = new();
        int groupNum = 1;

        //parse input
        Match m = PKUTIME_REGEX.Match(pkuTimeStr);
        if (!m.Success) return null; //invalid pkuTime

        //D - Date
        if (m.Groups[groupNum].Success)
        {
            string wMonth = m.Groups[groupNum].Value;
            if (wMonth.Length is 4) //YYYY
                wMonth += "-01"; //DateOnly needs at least month to parse.
            if (!DateOnly.TryParse(wMonth, out var d)) 
                return null;  //Failed to parse, should be b.c. invalid component range
            pt.Date = d;

            //calculate last comp
            pt.LastDateComp = -1;
            for (int i = 0; i < DATE_GROUPS; i++)
                if (m.Groups[1 + groupNum + i].Success)
                    pt.LastDateComp++;
        }
        groupNum += 1 + DATE_GROUPS;

        //T - Time
        if (m.Groups[groupNum].Success)
        {
            string wMinutes = m.Groups[groupNum].Value[1..]; //TimeOnly can't read 'T'
            if (wMinutes.Length is 2) //hh
                wMinutes += ":00"; //TimeOnly needs at least minutes to parse.
            if (!TimeOnly.TryParse(wMinutes, out var t))
                return null; //Failed to parse, should be b.c. invalid component range
            pt.Time = t;

            //calculate last comp
            pt.LastTimeComp = -1;
            for (int i = 0; i < TIME_GROUPS; i++)
                if (m.Groups[1 + groupNum + i].Success)
                    pt.LastTimeComp++;
        }
        groupNum += 1 + TIME_GROUPS;

        //O - Offset
        if (m.Groups[groupNum].Success)
        {
            if (!TimeSpan.TryParse(m.Groups[groupNum].Value.Replace("+", ""), out var o))
                return null; //Failed to parse, idk how.
            pt.Offset = o;
        }
        groupNum += 1 + OFFSET_GROUPS;

        //P - Duration
        if (m.Groups[groupNum].Success)
            pt.Duration = m.Groups[groupNum].Value;

        return pt;
    }

    /// <summary>
    /// Returns this <see cref="pkuTime"/> as Unix Time.<br/>
    /// If there is no <see cref="Date"/>, assumes 1970-01-01.<br/>
    /// If there is no <see cref="Time"/>, assumes T00:00:00.000.<br/>
    /// If there is no <see cref="Offset"/>, assumes +00:00 (UTC).<br/>
    /// </summary>
    /// <returns>This <see cref="pkuTime"/> as a <see cref="long"/> valued Unix time.</returns>
    public long ToUnixTime()
    {
        DateOnly date = Date ?? new DateOnly(1970, 1, 1); //unix epoch, i.e. t = 0 unix time
        TimeOnly time = Time ?? new TimeOnly(0, 0); //12am, i.e. t = 0 in day
        TimeSpan offset = Offset ?? new TimeSpan(0, 0, 0);//UTC, i.e. no offset

        DateTimeOffset dto = new(date.ToDateTime(time), offset);
        return dto.ToUnixTimeSeconds();
    }


    /* ------------------------------------
     * Determine Local TimeZone
     * ------------------------------------
    */
    /// <summary>
    /// This computer's current time zone expressed as an
    /// <see href="https://en.wikipedia.org/wiki/List_of_tz_database_time_zones">IANA time zone</see>.<br/>
    /// <i>Note that this is calculated only once, upon the launch of the application.</i>
    /// </summary>
    public static readonly string LOCAL_TIMEZONE; //can be null if no local IANA TZ is found
    static pkuTime()
    {
        if (TimeZoneInfo.Local.HasIanaId)
            LOCAL_TIMEZONE = TimeZoneInfo.Local.Id;
        else
            TimeZoneInfo.TryConvertWindowsIdToIanaId(TimeZoneInfo.Local.Id,
                RegionInfo.CurrentRegion.TwoLetterISORegionName, out LOCAL_TIMEZONE);
    }


    /* ------------------------------------
     * ToString methods
     * ------------------------------------
    */
    public override string ToString()
    {
        string str = "";
        str += DateToString(); //D - Date
        str += TimeToString(); //T - Time
        str += OffsetToString(); //O - Offset
        str += Duration; //P - Duration

        return str;
    }

    public string DateToString()
    {
        if (Date.HasValue)
        {
            string str = $"{Date.Value.Year:D4}";
            if (LastDateComp > 0) str += $"-{Date.Value.Month:D2}";
            if (LastDateComp > 1) str += $"-{Date.Value.Day:D2}";
            return str;
        }
        else return null;
    }

    public string TimeToString()
    {
        if (Time.HasValue)
        {
            string str = $"T{Time.Value.Hour:D2}";
            if (LastTimeComp > 0) str += $":{Time.Value.Minute:D2}";
            if (LastTimeComp > 1) str += $"-{Time.Value.Second:D2}";
            if (LastTimeComp > 2) str += $".{Time.Value.Millisecond:D3}";
            return str;
        }
        else return null;
    }

    public string OffsetToString()
    {
        if (Offset.HasValue)
        {
            var temp = Offset.Value.Duration();
            return $"{(Offset.Value < TimeSpan.Zero ? "-" : "+")}{temp.Hours:D2}:{temp.Minutes:D2}";
        }
        else return null;
    }


    /* ------------------------------------
     * Regex
     * ------------------------------------
    */
    //pkuTime: (DATE)?(TIME)?(OFFSET)?(DURATION)?
    //The extra regex asserts that:
    //  a) Must have at least one of {DATE, SET}
    //  b) If DATE and TIME were both matched, DATE must have a DAYS match.
    //Note that: pkuTime allows DATE w/o TIME to have an OFFSET, unlike pure ISO 8601.
    //           It also allows a time with duration, yet no date.
    private static readonly Regex PKUTIME_REGEX = new($@"^({DATE_REGEX})?(^|(?(3)|(?:$^)){TIME_REGEX})?(?!^)({OFFSET_REGEX})?({DURATION_REGEX})?$");

    //D - Date: YYYY(-MM(-DD)?)?
    private const string DATE_REGEX = @"(\d{4})(?:-(\d{2})(?:-(\d{2}))?)?";
    private const int DATE_GROUPS = 3;

    //T - Time: Thh(:mm(:ss(.fff)?)?)?
    private const string TIME_REGEX = @"T(\d{2})(?::(\d{2})(?::(\d{2})(?:\.(\d{3}))?)?)?";
    private const int TIME_GROUPS = 4;

    //O - Offset: ±hh:mm
    private const string OFFSET_REGEX = @"([+-])(\d{2}):(\d{2})";
    private const int OFFSET_GROUPS = 3;

    //P - Duration: P(nY)?(nM)?(nD)?(T(nH)?(nM)?(n.fffS)?)?
    private const string DURATION_REGEX = @"\/P(?!$)(?:(?:(\d+)Y)?(?:(\d+)M)?(?:(\d+)D)?)?(?:T(?!$)(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)(\.\d{3})?S)?)?";
    private const int DURATION_GROUPS = 7;
}