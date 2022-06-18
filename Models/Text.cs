using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Text
    {


	    #region objectValues
        [JsonProperty]
        public string documentID;
        [JsonProperty]
        public string filePath;
        [JsonProperty]
        public string textID;
        [JsonProperty]
        public string textName;
        [JsonProperty]
	    public List<Dictionary<string, List<Value>>> textMetaData = new List<Dictionary<string, List<Value>>>();
        [JsonProperty]
        public List<Clause> clauses = new List<Clause>();
        #endregion

        #region Constructors

        [JsonConstructor]
        public Text(string _documentID, string _textID, string _textName, string _filePath, List<Dictionary<string, List<Value>>> _textMetaData, List<Clause> _clauses)
        {
            this.documentID = _documentID;
            this.textID = _textID;
            this.filePath = _filePath;
            this.textName = _textName;
            this.textMetaData = _textMetaData;
            this.clauses = _clauses;
        }

        public Text(string _documentID, string _textID, string _textName, string _filePath)
        {
            this.documentID = _documentID;
            this.textID = _textID;
            this.textName = _textName;
            this.filePath = _filePath;
        }

        public Text(Document document, string _textID, string _textName)
        {
            this.documentID = document.documentID;
            this.textID = _textID;
            this.textName = _textName;
            this.filePath = document.filePath;
        }

        public Text()
        {

        }


	#endregion



    #region PublicMethods
        public string Jsonize()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }

        public string Output()
        {
            Func<string> sentences = () =>
            {
                string collected = "";
                foreach (var c in clauses.OrderBy(clause => Convert.ToInt32(clause.documentID)).ThenBy(clause => Convert.ToInt32(clause.textID)).ThenBy(clause => Convert.ToInt32(clause.clauseID)))
                {
                    collected += c.Output();
                }
                return collected;
            };
            try
            {
                Func<List<Dictionary<string, List<Value>>>, string> textInRawText = (List<Dictionary<string, List<Value>>> fields) =>
                {
                    string result = "";
                    foreach (var optional_tagging in fields)
                    {
                        foreach (var field in optional_tagging)
                        {
                            result += field.Key;
                            result += ":";
                            foreach (var fieldValue in field.Value)
                            {
                                result += fieldValue.name;
                                result += ";";
                            }
                            result += "||";
                        }
                        result += "\n";
                    }
                    return result;
                };
                Func<List<Dictionary<string, List<Value>>>, string> textInHTML = (List<Dictionary<string, List<Value>>> fields) =>
                {
                    return textInRawText.Invoke(fields).Replace("\n", "<br />");
                };
                return "<span title=\"" + textInRawText.Invoke(textMetaData) + "\" data-content=\"" + textInHTML.Invoke(textMetaData) + "\" class=\"text\" id=\"" + this.documentID + "|" + this.textID + "\"> " + sentences.Invoke() + "</span><br />";
            }
            catch
            {
                return "<span title= \"\" data-content=\"\" class=\"text\" id=\"" + this.documentID + "|" + this.textID + "\"> " + sentences.Invoke() + "</span><br />";
            }
        }



        #endregion
    }
}
