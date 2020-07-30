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
        public Dictionary<string, List<IValue>> documentMetaData = new Dictionary<string, List<IValue>>();
        [JsonProperty]
        public List<Text> texts = new List<Text>();
        #endregion

        #region Constructors

        [JsonConstructor]
        public Document(string _documentID, string _documentName, string _filePath, string _googleDocPath)
        {
            documentID = _documentID;
            documentName = _documentName;
            filePath = _filePath;
            googleDocPath = _googleDocPath;
        }

        #endregion
        #region PublicMethods
        public string Jsonize()
        {
            string json = JsonConvert.SerializeObject(this);
            return json;
        }
        #endregion





    }
}