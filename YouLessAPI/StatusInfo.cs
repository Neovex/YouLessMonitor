using Newtonsoft.Json;

namespace YouLessAPI
{
    public class StatusInfo
    {
        /// <summary>
        /// Counter in kWh
        /// </summary>
        [JsonProperty("cnt")]
        public string TotalKwH { get; set; }

        /// <summary>
        /// Power consumption in Watt
        /// </summary>
        [JsonProperty("pwr")]
        public int Consumption { get; set; }

        /// <summary>
        /// Moving average level (intensity of reflected light on analog meters)
        /// </summary>
        [JsonProperty("lvl")]
        public int ReflectionLevelAverage { get; set; }

        /// <summary>
        /// Deviation of reflection
        /// </summary>
        [JsonProperty("dev")]
        public string ReflectionDeviation { get; set; }

        /// <summary>
        /// Unused - always empty
        /// </summary>
        [JsonIgnore()]
        [JsonProperty("det")]
        public string Det { get; set; }

        /// <summary>
        /// Connection status to remote online service monitoring
        /// </summary>
        [JsonProperty("con")]
        public string RemoteConnectionStatus { get; set; }

        /// <summary>
        /// Time until next status update will be send to remote online service monitoring
        /// </summary>
        [JsonProperty("sts")]
        public string RemoteConnectionNextUpdate { get; set; }

        /// <summary>
        /// Counter in kWh for S0
        /// </summary>
        [JsonProperty("cs0")]
        public string TotalKwH_S0 { get; set; }

        /// <summary>
        /// Power consumption in Watt for S0
        /// </summary>
        [JsonProperty("ps0")]
        public int Consumption_S0 { get; set; }

        /// <summary>
        /// Raw 10-bit light reflection level (without averaging)
        /// </summary>
        [JsonProperty("raw")]
        public int ReflectionLevelRaw { get; set; }
    }
}