using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Clause
    {
        

        #region objectValues
        [JsonProperty]
        public string documentID;
        [JsonProperty]
        public string filePath;
        [JsonProperty]
        public string textID;
        [JsonProperty]
        protected Dictionary<string, List<IValue>> clauseFields = new Dictionary<string, List<IValue>>();
        [JsonProperty]
        public string clauseID;
        [JsonProperty]
        public string clauseText;
        [JsonProperty]
        public List<Realization> realizations;
        #endregion


        #region Constructors
        public Clause(Text text, string _clauseID, string _clauseText)
        {
            this.documentID = text.documentID;
            this.filePath = text.filePath;
            this.textID = text.textID;
            this.clauseID = _clauseID;
            this.clauseText = _clauseText;
        }
        
        #endregion

        #region publicMethods
        public string Jsonize()
        {
            string jsonedClause = JsonConvert.SerializeObject(this);
            return jsonedClause;
        }
        #endregion
    }
}
