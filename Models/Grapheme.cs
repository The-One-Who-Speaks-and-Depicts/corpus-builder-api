using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Grapheme
    {
        [JsonProperty]
        public string documentID;
        [JsonProperty]
        public string filePath;
        [JsonProperty]
        public string textID;
        [JsonProperty]
        public string clauseID;
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> graphemeFields;
        [JsonProperty]
        public string realizationID;
        [JsonProperty]
        public string graphemeID;
        [JsonProperty]
        public string grapheme;


        [JsonConstructor]
        public Grapheme(string _documentID, string _filePath, string _textID, string _clauseID, List<Dictionary<string, List<Value>>> _fields, string _realizationID, string _graphemeID, string _grapheme)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.graphemeFields = _fields;
            this.realizationID = _realizationID;
            this.graphemeID = _graphemeID;
            this.grapheme = _grapheme;
        }

        public Grapheme(Realization realization, string _graphemeID, string _grapheme)
        {
            this.documentID = realization.documentID;
            this.filePath = realization.filePath;
            this.textID = realization.textID;
            this.clauseID = realization.clauseID;
            this.realizationID = realization.realizationID;
            this.graphemeID = _graphemeID;
            this.grapheme = _grapheme;
        }

        public Grapheme(string _documentID, string _filePath, string _textID, string _clauseID, string _realizationID, string _graphemeID, string _grapheme)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.realizationID = _realizationID;
            this.graphemeID = _graphemeID;
            this.grapheme = _grapheme;
        }

        public Grapheme()
        {

        }

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            return realizationToJson;
        }

        public string Output()
        {
            try
            {
                Func<List<Dictionary<string, List<Value>>>, string> fieldsInRawText = (List<Dictionary<string, List<Value>>> fields) =>
                {
                    string result = "";
                    foreach (var optional_tagging in fields)
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
                    }
                    return result;
                };
                Func<List<Dictionary<string, List<Value>>>, string> fieldsInHTML = (List<Dictionary<string, List<Value>>> fields) =>
                {
                    return fieldsInRawText.Invoke(fields).Replace("\n", "<br />");
                };
                return "<span title=\"" + fieldsInRawText.Invoke(graphemeFields) + "\" data-content=\"" + fieldsInHTML.Invoke(graphemeFields) + "\" class=\"grapheme\" id=\"" + this.documentID + "|" + this.textID + "|" + this.clauseID + "|" + this.realizationID + "|" + this.graphemeID + "\">" + grapheme + "</span>";
            }
            catch
            {
                return "<span title= \"\" data-content=\"\" class=\"grapheme\" id=\"" + this.documentID + "|" + this.textID + "|" + this.clauseID + "|" + this.realizationID + "|" + this.graphemeID + "\">" + grapheme + "</span>";
            }
        }
    }
}
