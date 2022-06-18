using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using System.Linq;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Realization : IEquatable<Realization>
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
        public List<Dictionary<string, List<Value>>> realizationFields;
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
        public Realization(string _documentID, string _filePath, string _textID, string _clauseID, List<Dictionary<string, List<Value>>> _fields, string _realizationID, string _lexemeOne, string _lexemeTwo, List<Grapheme> _letters)
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
            string realizationToJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            return realizationToJson;
        }

        public string Output()
        {
            Func<string> graphemes = () =>
            {
                string collected = "";
                foreach (var l in letters.OrderBy(graheme => Convert.ToInt32(graheme.documentID)).ThenBy(graheme => Convert.ToInt32(graheme.textID)).ThenBy(grapheme => Convert.ToInt32(grapheme.clauseID)).ThenBy(graheme => Convert.ToInt32(graheme.realizationID)).ThenBy(graheme => Convert.ToInt32(graheme.graphemeID)))
                {
                    collected += l.Output();
                }
                return collected;
            };
            try
            {
                Func<List<Dictionary<string, List<Value>>>, string> fieldsInRawText = (List<Dictionary<string, List<Value>>> fields) =>
                {
                    string result = "";
                    foreach (var optional_tagging in fields)
                    {
                        if (optional_tagging.Count > 0)
                        {
                            foreach (var field in optional_tagging)
                            {
                                result += field.Key;
                                result += ":";
                                for (int i = 0; i < field.Value.Count; i++)
                                {
                                    result += field.Value[i].name;
                                    if (i < field.Value.Count - 1)
                                    {
                                        result += ",";
                                    }
                                }
                                result += ";\n";
                            }
                            result += "***";
                        }
                    }
                    return result;
                };
                Func<List<Dictionary<string, List<Value>>>, string> fieldsInHTML = (List<Dictionary<string, List<Value>>> fields) =>
                {
                    return fieldsInRawText.Invoke(fields).Replace("\n", "<br />");
                };
                return "<span title=\"" + fieldsInRawText.Invoke(realizationFields) + "\" data-content=\"" + fieldsInHTML.Invoke(realizationFields) + "\" class=\"word\" id=\"" + this.documentID + "|" + this.textID + "|" + this.clauseID + "|" + this.realizationID + "\"> " + graphemes.Invoke() + "</span>";
            }
            catch
            {
                return "<span title= \"\" data-content=\"\" class=\"word\" id=\"" + this.documentID + "|" + this.textID +  "|" + this.clauseID + "|" + this.realizationID + "\"> " + graphemes.Invoke() + "</span>";
            }
        }

        public string KeyOutput()
        {
            try
            {
                Func<List<Dictionary<string, List<Value>>>, string> fieldsInRawText = (List<Dictionary<string, List<Value>>> fields) =>
                {
                    string result = "";
                    foreach (var optional_tagging in fields)
                    {
                        if (optional_tagging.Count > 0)
                        {
                            foreach (var field in optional_tagging)
                            {
                                result += field.Key;
                                result += ":";
                                for (int i = 0; i < field.Value.Count; i++)
                                {
                                    result += field.Value[i].name;
                                    if (i < field.Value.Count - 1)
                                    {
                                        result += ",";
                                    }
                                }
                                result += ";\n";
                            }
                            result += "***";
                        }
                    }
                    return result;
                };
                Func<List<Dictionary<string, List<Value>>>, string> fieldsInHTML = (List<Dictionary<string, List<Value>>> fields) =>
                {
                    return fieldsInRawText.Invoke(fields).Replace("\n", "<br />");
                };
                return "<span title=\"" + fieldsInRawText.Invoke(realizationFields) + "\" data-content=\"" + fieldsInHTML.Invoke(realizationFields) + "\" class=\"word\" id=\"" + this.documentID + "|" + this.textID + "|" + this.clauseID + "|" + this.realizationID + "\"> " + this.lexemeTwo + "</span>";
            }
            catch
            {
                return "<span title= \"\" data-content=\"\" class=\"word\" id=\"" + this.documentID + "|" + this.textID + "|" + this.clauseID + "|" + this.realizationID + "\"> " + this.lexemeTwo + "</span>";
            }
        }

        public bool Equals(Realization other)
        {
            if (documentID == other.documentID && textID == other.textID && clauseID == other.clauseID && realizationID == other.realizationID) return true;
            return false;
        }

        #endregion

        #region privateMethods
        #endregion

    }
}
