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
        public List<Dictionary<string, List<IValue>>> realizationFields;
        [JsonProperty]
        public string realizationID;
        [JsonProperty]
        public string lexemeOne;
        [JsonProperty]
        public string lexemeTwo;
        [JsonProperty]
        public List<Grapheme> letters;
        #endregion

        #region Constructors

        [JsonConstructor]
        public Realization(string _documentID, string _filePath, string _textID, string _clauseID, List<Dictionary<string, List<IValue>>> _fields, string _realizationID, string _lexemeOne, string _lexemeTwo, List<Grapheme> _letters)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.realizationFields = _fields;
            this.realizationID = _realizationID;
            this.lexemeOne = _lexemeOne;
            this.lexemeTwo = _lexemeTwo;
            this.letters = _letters;
        }

        public Realization(Clause clause, string _realizationID, string _lexemeOne, string _lexemeTwo, List<Grapheme> _letters)
        {
            this.documentID = clause.documentID;
            this.filePath = clause.filePath;
            this.textID = clause.textID;
            this.clauseID = clause.clauseID;
            this.realizationID = _realizationID;
            this.lexemeOne = _lexemeOne;
            this.lexemeTwo = _lexemeTwo;
            this.letters = _letters;
        }


        public Realization(string _documentID, string _filePath, string _textID, string _clauseID, string _realizationID, string _lexemeOne, string _lexemeTwo, List<Grapheme> _letters)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.realizationID = _realizationID;
            this.lexemeOne = _lexemeOne;
            this.lexemeTwo = _lexemeTwo;
            this.letters = _letters;
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
