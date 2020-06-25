using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class SimpleValue : IValue
    {
        [JsonProperty]
        public string name { get ; set ; }
        [JsonIgnore]
        public List<int> letters { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [JsonIgnore]
        public List<Realization> connectedRealizations { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [JsonConstructor]
        public SimpleValue(string _name)
        {
            this.name = _name;
        }

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this);
            return realizationToJson;
        }
    }
}
