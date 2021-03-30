using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YouLessAPI
{
    public class History
    {
        /// <summary>
        /// Type of Unit used for the samples
        /// </summary>
        [JsonProperty("un")]
        public string Unit { get; set; }

        /// <summary>
        /// Time of the first sample
        /// </summary>
        [JsonProperty("tm")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Delta time in seconds (time between samples)
        /// </summary>
        [JsonProperty("dt")]
        public int Delta { get; set; }

        /// <summary>
        /// An array with values for each (recorded) sample (array is null-terminated)
        /// </summary>
        [JsonProperty("val")]
        public List<string> Samples { get; set; }
    }
}