using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class ParallelClause
    {
        [JsonProperty]
        public string textName;
        [JsonProperty]

        public List<Dictionary<string, List<Value>>> textMetaData;
        [JsonProperty]
        public Clause clause;


        public string Jsonize()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }
    }
}
