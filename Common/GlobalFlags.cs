using Newtonsoft.Json;

namespace pkuManager.Common
{
    public partial class GlobalFlags
    {
        [JsonProperty("Battle Stat Override")]
        public bool Battle_Stat_Override;
    }
}
