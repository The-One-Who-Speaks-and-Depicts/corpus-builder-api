using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{
    public class Document
    {


        #region objectValues
        [JsonProperty]
        public string documentID;
        [JsonProperty]
        public readonly string documentName;
        [JsonProperty]
        public string filePath;
        [JsonProperty]
        public string googleDocPath;
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> documentMetaData = new List<Dictionary<string, List<Value>>>();
        [JsonProperty]
        public List<Text> texts = new List<Text>();
        #endregion

        #region Constructors

        [JsonConstructor]
        public Document(string _documentID, string _documentName, string _filePath, string _googleDocPath, List<Dictionary<string, List<Value>>> _documentMetaData, List<Text> _texts)
        {
            documentID = _documentID;
            documentName = _documentName;
            filePath = _filePath;
            googleDocPath = _googleDocPath;
            documentMetaData = _documentMetaData;
            texts = _texts;
        }

        public Document(string _documentID, string _documentName, string _filePath, string _googleDocPath)
        {
            documentID = _documentID;
            documentName = _documentName;
            filePath = _filePath;
            googleDocPath = _googleDocPath;
        }

        public Document()
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
            Func<string> parts = () =>
            {
                string collected = "";
                foreach (var t in texts.OrderBy(text => Convert.ToInt32(text.documentID)).ThenBy(text => Convert.ToInt32(text.textID)))
                {
                    collected += t.Output();
                }
                return collected;
            };
            try
            {
                Func<List<Dictionary<string, List<Value>>>, string> documentInRawText = (List<Dictionary<string, List<Value>>> fields) =>
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
                Func<List<Dictionary<string, List<Value>>>, string> documentInHTML = (List<Dictionary<string, List<Value>>> fields) =>
                {
                    return documentInRawText.Invoke(fields).Replace("\n", "<br />");
                };
                return "<span title=\"" + documentInRawText.Invoke(documentMetaData) + "\" data-content=\"" + documentInHTML.Invoke(documentMetaData) + "\" class=\"text\" id=\"" + this.documentID + "\"> " + parts.Invoke() + "</span><br />";
            }
            catch
            {
                return "<span title= \"\" data-content=\"\" class=\"text\" id=\"" + this.documentID + "\"> " + parts.Invoke() + "</span><br />";
            }
        }
        #endregion





    }
}
