using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using ManuscriptsProcessor.Values;

namespace ManuscriptsProcessor.Units
{
    [Serializable]
    public class ParallelClause : ICorpusUnit
    {
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string text { get; set; }
        [JsonProperty]

        public List<Dictionary<string, List<Value>>> tagging { get; set; }
        [JsonProperty]
        public Clause clause;


        public string Jsonize()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }

        public string Output()
        {
            return "";
        }
    }
}
