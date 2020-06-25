using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class MWELetterValue : IValue
    {
        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public List<int> letters { get; set; }
        [JsonProperty]
        public List<Realization> connectedRealizations { get; set; }

        [JsonConstructor]
        public MWELetterValue(string _name, List<int> _letters, List<Realization> _connectedRealizations)
        {
            this.name = _name;
            this.letters = _letters;
            this.connectedRealizations = _connectedRealizations;
        }

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this);
            return realizationToJson;
        }
    }
}
