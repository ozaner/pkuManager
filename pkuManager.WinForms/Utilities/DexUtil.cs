using Newtonsoft.Json.Linq;
using pkuManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace pkuManager.WinForms.Utilities;

public static class DexUtil
{
    /* ------------------------------------
     * Generic DataDex Methods
     * ------------------------------------
    */
    /// <summary>
    /// Searches the contents of a <paramref name="dex"/> with the given <paramref name="keys"/>.<br/>
    /// If no object exists at the given location, <see langword="null"/> will be returned.<br/>
    /// Throws an exception if an object does exist but can't be casted to <typeparamref name="T"/>.<br/>
    /// Note that direct array indexing is not yet implemented.
    /// </summary>
    /// <typeparam name="T">The nullable type of the value to be read.</typeparam>
    /// <param name="dex">The datadex being read.</param>
    /// <param name="keys">The location of the desired value in <paramref name="dex"/>.</param>
    /// <returns>The value pointed at by <paramref name="keys"/>, or <see langword="null"/> if it doesn't exist.</returns>
    public static T ReadDataDex<T>(this JObject dex, params string[] keys)
    {
        JObject temp = dex;
        if (keys?.Length is 0)
            return dex.ToObject<T>();
        for (int i = 0; i < keys.Length; i++)
        {
            //null isn't a valid key
            if (keys[i] is null || temp is null)
                return default;

            // try searching local for key
            if (temp.ContainsKey(keys[i]))
            {
                var value = temp[keys[i]];
                if (i >= keys.Length - 1) //last key
                {
                    try { return value.ToObject<T>(); } //return whatever the value is
                    catch { return default; }//can't return the value, doesn't match the type
                }
                else //not last key, should be a dict
                {
                    if (value is JObject valueJObj) //is a dict
                        temp = valueJObj; //continue loop
                    else //isn't a dict
                        return default; //failure, return null

                    //TODO: add direct array indexing, i.e. else if(value is JArray valueJArr)
                }
            }
            // no local key found, look for possible override
            else if (temp.ContainsKey("$override"))
            {
                List<string> remote_keys = new(temp["$override"].ToObject<string[]>());
                remote_keys.AddRange(keys.Skip(i));
                return dex.ReadDataDex<T>(remote_keys.ToArray());
            }
            else //key not found, no potential override
                return default; //failure, return null
        }
        return default; //shouldn't get here
    }


    /* ------------------------------------
     * SpeciesDex Methods
     * ------------------------------------
    */
    /// <summary>
    /// Enumerates all the different subsets of appearances
    /// of the given <paramref name="sfam"/>'s appearance.<br/>
    /// This method enumerates the appearance combos in the canonical order.
    /// </summary>
    /// <param name="sfam">An SFA.</param>
    /// <returns>An enumerator of <paramref name="sfam"/>'s different appearance combinations.</returns>
    private static IEnumerable<string> GetSearchableAppearances(this SFAM sfam)
    {
        string[] appList = sfam.Appearance.SplitLexical();
        if (appList is not null)
        {
            int effectiveSize = appList.Length > 10 ? 10 : appList.Length;
            int powesize = (1 << effectiveSize) - 1;
            for (int i = 0; i <= powesize; i++)
            {
                List<string> apps = new();
                for (int j = 0; j < effectiveSize; j++)
                {
                    if ((i & (1 << j)) is 0) //reversed 0 and 1 so loop could go form 0 to powsize
                        apps.Add(appList[j]);
                }
                yield return apps.ToArray().JoinLexical();
            }
        }
        yield return null; //no appearance
    }

    // Iterates through each possible appearance, returning every combo of key prefixes for species/form/appearances.
    // Used to search through the SpeciesDex.
    private static IEnumerable<List<string>> GetSearchablePKUCombos(this SFAM sfam)
    {
        var apps = sfam.GetSearchableAppearances();
        List<string> keys = new() { sfam.Species, "Forms", sfam.Form, "Appearance", null };
        foreach (string app in apps)
        {
            if (app is null)
                yield return keys.Take(3).ToList();
            else
                keys[4] = app;
            yield return keys;
        }
        yield return new() { sfam.Species }; //all forms/appearnces failed, try base species.
    }

    /// <summary>
    /// Like <see cref="ReadDataDex{T}(JObject, string[])"/> but specifically for SpeciesDexes.<br/>
    /// Searches through the appearances, form(s), then species of a species entry with the given keys.
    /// </summary>
    /// <param name="sfam">The pku whose species/form/appearance is to be used.</param>
    /// <inheritdoc cref="ReadDataDex{T}(JObject, string[])"/>
    public static T ReadSpeciesDex<T>(this JObject dex, SFAM sfam, params string[] keys)
    {
        var combos = sfam.GetSearchablePKUCombos();
        foreach (var combo in combos)
        {
            combo.AddRange(keys);
            T obj = dex.ReadDataDex<T>(combo.ToArray());
            if (obj is not null)
                return obj;
            else //try gender split
            {
                T[] objArr = dex.ReadDataDex<T[]>(combo.ToArray());
                if (objArr?.Length == 2) //two values, one for each gender
                    return objArr[Convert.ToInt32(sfam.IsFemale)];
            }
        }
        return default;
    }
}