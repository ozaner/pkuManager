using Newtonsoft.Json;

namespace pkuManager.WinForms.Formats.pku;

public class GlobalFlags
{
    [JsonProperty("Battle Stat Override")]
    public bool Battle_Stat_Override;

    [JsonProperty("Default Form Override")]
    public bool Default_Form_Override;
}