using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using ManuscriptsProcessor.Values;

namespace ManuscriptsProcessor.Units
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
        [JsonConstructor]
        public Segment(string _Id, string _text, List<Dictionary<string, List<Value>>> _tagging, List<Clause> _subunits)
        {
            Id = _Id;
            text = _text;
            tagging = _tagging;
            subunits = _subunits;
        }

        public Segment(Section _section, string _segmentId, string name)
        {
            Id = _section.Id + "|" + _segmentId;
            text = name;
        }

        public Segment()
        {

        }
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
