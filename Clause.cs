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
        public List<Realization> realizations = new List<Realization>();
        #endregion


        #region Constructors
        [JsonConstructor]
        public Clause(string _documentID, string _textID, string _filePath, string _clauseID, string _clauseText, Dictionary<string, List<IValue>> _clauseFields, List<Realization> _realizations)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.clauseText = _clauseText;
            this.clauseFields = _clauseFields;
            this.realizations = _realizations;
        }
        public Clause(string _documentID, string _textID, string _filePath, string _clauseID, string _clauseText)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.clauseText = _clauseText;
        }

        public Clause(Text text, string _clauseID, string _clauseText)
        {
            this.documentID = text.documentID;
            this.filePath = text.filePath;
            this.textID = text.textID;
            this.clauseID = _clauseID;
            this.clauseText = _clauseText;
        }
        public Clause()
        {

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
