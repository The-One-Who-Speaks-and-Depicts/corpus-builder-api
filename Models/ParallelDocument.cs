using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using CorpusDraftCSharp;

namespace CorpusDraftCSharp {
    [Serializable]
    public class ParallelDocument {
        [JsonProperty]
        public string id;
        [JsonProperty]
        public string name;
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> documentMetaData;
        [JsonProperty]
        public ParallelClause[,] parallelClauses;
        public List<ParallelToken> parallelTokens;
        public string Jsonize()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }
    }
}
