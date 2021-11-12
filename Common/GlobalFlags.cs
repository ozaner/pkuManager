using Newtonsoft.Json;

namespace pkuManager.Common;

public class GlobalFlags
{
    [JsonProperty("Battle Stat Override")]
    public bool Battle_Stat_Override;

    //[JsonProperty("Castable Form Override")]
    //public bool Castable_Form_Override;
}