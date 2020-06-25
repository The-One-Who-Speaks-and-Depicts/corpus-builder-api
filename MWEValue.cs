using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class MWEValue : IValue
    {
        [JsonProperty]
        public string name { get; set; }
        [JsonIgnore]
        public List<int> letters { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [JsonProperty]
        public List<Realization> connectedRealizations { get; set; }

        [JsonConstructor]
        public MWEValue(string _name, List<Realization> _connectedRealizations)
        {
            this.name = _name;
            this.connectedRealizations = _connectedRealizations;
        }

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this);
            return realizationToJson;
        }
    }
}
