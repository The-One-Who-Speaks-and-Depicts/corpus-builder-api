using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using System.Linq;

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
        public List<Grapheme> letters = new List<Grapheme>();
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

        public Realization(Clause clause, string _realizationID, string _lexemeOne, string _lexemeTwo)
        {
            this.documentID = clause.documentID;
            this.filePath = clause.filePath;
            this.textID = clause.textID;
            this.clauseID = clause.clauseID;
            this.realizationID = _realizationID;
            this.lexemeOne = _lexemeOne;
            this.lexemeTwo = _lexemeTwo;
        }


        public Realization(string _documentID, string _filePath, string _textID, string _clauseID, string _realizationID, string _lexemeOne, string _lexemeTwo)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.realizationID = _realizationID;
            this.lexemeOne = _lexemeOne;
            this.lexemeTwo = _lexemeTwo;
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

        public string Output()
        {
            Func<string> graphemes = () =>
            {
                string collected = "";
                foreach (var l in letters)
                {
                    collected += l.Output();
                }
                return collected;
            };
            try
            {
                Func<List<Dictionary<string, List<IValue>>>, string> fieldsInRawText = (List<Dictionary<string, List<IValue>>> fields) =>
                {
                    return "";
                };
                Func<List<Dictionary<string, List<IValue>>>, string> fieldsInHTML = (List<Dictionary<string, List<IValue>>> fields) =>
                {
                    return "";
                };
                return "<span title=\"" + fieldsInRawText.Invoke(realizationFields) + "\" data-content=\"" + fieldsInHTML.Invoke(realizationFields) + "\" class=\"word\" id=\"" + this.documentID + "|" + this.clauseID + "|" + this.realizationID + "\"> " + graphemes.Invoke() + "</span>";
            }
            catch
            {
                return "<span title= \"\" data-content=\"\" class=\"word\" id=\"" + this.documentID + "|" + this.clauseID + "|" + this.realizationID + "\"> " + graphemes.Invoke() + "</span>";
            }
        }

        #endregion

        #region privateMethods
        #endregion

    }
}
