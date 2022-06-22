using Newtonsoft.Json;
using ManuscriptsProcessor.Values;

namespace ManuscriptsProcessor.Units
{
    public class ParallelToken : ICorpusUnit, IUnitGroup<TokenGroup>, IComparable<ParallelToken>
    {
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string text { get; set; }
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> tagging { get; set; }
        [JsonProperty]
        public List<TokenGroup> subunits { get; set; }

        public List<TokenGroup> GetParallels(TokenGroup source)
        {
            var parallels = this.subunits.Where(r => r != source).ToList();
            return parallels;
        }
        [JsonConstructor]
        public ParallelToken (string _Id, string _text, List<Dictionary<string, List<Value>>> _tagging, List<TokenGroup> _subunits)
        {
            Id = _Id;
            text = _text;
            tagging = _tagging;
            subunits = _subunits;
        }
        public ParallelToken()
        {

        }
        public string Output()
        {
            throw new NotImplementedException();
        }

        public string Jsonize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public int CompareTo(ParallelToken other)
        {
            return MyExtensions.CompareIds(Id, other.Id);
        }

    }
}
