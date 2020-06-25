using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Realization
    {
        
        #region objectValues
        [JsonProperty]
        public string documentID;
        [JsonProperty]
        public string filePath;
        [JsonProperty]
        public string textID;
        [JsonProperty]
        public string clauseID;
        [JsonProperty]
        public Dictionary<string, List<IValue>> realizationFields;
        [JsonProperty]
        public string realizationID;
        [JsonProperty]
        public string lexeme;
        #endregion

        #region Constructors

        [JsonConstructor]
        public Realization(string _documentID, string _filePath, string _textID, string _clauseID, Dictionary<string, List<IValue>> _fields, string _realizationID, string _lexeme)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.realizationFields = _fields;
            this.realizationID = _realizationID;
            this.lexeme = _lexeme;
        }

        public Realization(Clause clause, string _realizationID, string _lexeme)
        {
            this.documentID = clause.documentID;
            this.filePath = clause.filePath;
            this.textID = clause.textID;
            this.clauseID = clause.clauseID;
            this.realizationID = _realizationID;
            this.lexeme = _lexeme;
        }


        public Realization(string _documentID, string _filePath, string _textID, string _clauseID, string _realizationID, string _lexeme)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.realizationID = _realizationID;
            this.lexeme = _lexeme;
        }

        public Realization()
        {

        }
        
        
        #endregion

        #region publicMethods

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this);
            return realizationToJson;
        }

        #endregion

        #region privateMethods
        #endregion

    }
}
