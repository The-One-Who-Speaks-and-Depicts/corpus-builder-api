using ManuscriptsProcessor;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ManuscriptsProcessor.Units
{
    public class ParallelDictionaryUnit : DictionaryUnit
    {

        [JsonProperty]
        public List<ParallelToken> realizations;

        [JsonConstructor]
        public ParallelDictionaryUnit(string _lemma, List<ParallelToken> _realizations)
        {
            lemma = _lemma;
            realizations = _realizations;
        }

        public ParallelDictionaryUnit()
        {

        }
    }
}
