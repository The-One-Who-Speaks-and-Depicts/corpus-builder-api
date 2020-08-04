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
        public Dictionary<string, List<IValue>> graphemeFields;
        [JsonProperty]
        public string realizationID;
        [JsonProperty]
        public string graphemeID;
        [JsonProperty]
        public string grapheme;


        [JsonConstructor]
        public Grapheme(string _documentID, string _filePath, string _textID, string _clauseID, Dictionary<string, List<IValue>> _fields, string _realizationID, string _graphemeID, string _grapheme)
        {
            this.documentID = _documentID;
            this.filePath = _filePath;
            this.textID = _textID;
            this.clauseID = _clauseID;
            this.graphemeFields = _fields;
            this.realizationID = _realizationID;
            this.graphemeFields = _fields;
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
            string realizationToJson = JsonConvert.SerializeObject(this);
            return realizationToJson;
        }

        public string Output()
        {
            return grapheme;
        }
    }
}
