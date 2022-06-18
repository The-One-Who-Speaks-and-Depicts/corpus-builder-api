using CorpusDraftCSharp;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorpusDraftCSharp
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
