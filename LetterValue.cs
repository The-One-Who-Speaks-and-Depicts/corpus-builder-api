using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class LetterValue : IValue
    {
        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public List<int> letters { get; set; }
        [JsonIgnore]
        public List<Realization> connectedRealizations { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [JsonConstructor]
        public LetterValue(string _name, List<int> _letters)
        {
            this.name = _name;
            this.letters = _letters;
        }

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this);
            return realizationToJson;
        }
    }
}
