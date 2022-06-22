using ManuscriptsProcessor.Values;
using ManuscriptsProcessor;
using Newtonsoft.Json;

namespace ManuscriptsProcessor.Units
{
    public class TokenGroup : ICorpusUnit, IUnitGroup<Token>, IComparable<TokenGroup>
    {
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string text { get; set; }
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> tagging { get; set; }
        [JsonProperty]
        public List<Token> subunits { get; set; }
        [JsonConstructor]
        public TokenGroup (string _Id, string _text, List<Dictionary<string, List<Value>>> _tagging, List<Token> _subunits)
        {
            Id = _Id;
            text = _text;
            tagging = _tagging;
            subunits = _subunits;
        }

        public TokenGroup()
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

        public int CompareTo(TokenGroup other)
        {
            return MyExtensions.CompareIds(Id, other.Id);
        }
    }
}
