using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace CorpusDraftCSharp
{
    public class Segment : ICorpusUnit, IUnitGroup<Clause>, IComparable<Segment>
    {
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string text { get; set; }
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> tagging { get; set; }
        [JsonProperty]
        public List<Clause> subunits { get; set; }
        public string Jsonize()
        {
            string jsonedClause = JsonConvert.SerializeObject(this, Formatting.Indented);
            return jsonedClause;
        }

        public string Output()
        {
            return MyExtensions.UnitOutput(this);
        }
        public int CompareTo(Segment other)
        {
            return MyExtensions.CompareIds(Id, other.Id);
        }
    }
}
