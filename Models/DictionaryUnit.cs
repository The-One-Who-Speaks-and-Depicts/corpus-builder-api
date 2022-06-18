using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{
    public class DictionaryUnit
    {
        #region classValues

        #endregion

        #region objectValues
        [JsonProperty]
        public string lemma;
        [JsonProperty]
        public List<Realization> realizations;
        #endregion

        [JsonConstructor]
        public DictionaryUnit(string lemma, List<Realization> realizations)
        {
            this.lemma = lemma;
            this.realizations = realizations;
        }

        public DictionaryUnit()
        {

        }
        public string Jsonize()
        {
            string DictionaryUnitJsoned = JsonConvert.SerializeObject(this, Formatting.Indented);
            return DictionaryUnitJsoned;
        }
    }
}
