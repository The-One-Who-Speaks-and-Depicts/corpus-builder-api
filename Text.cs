using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;

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
	    public Dictionary<string, List<IValue>> textMetaData = new Dictionary<string, List<IValue>>();
        [JsonProperty]
        public List<Clause> clauses;
	    #endregion	
	
	    #region Constructors
    
        [JsonConstructor]
        public Text (string _documentID, string _textID, string _filePath, Dictionary<string, List<IValue>> _textMetaData, List<Clause> _clauses)
        {
            this.documentID = _documentID;
            this.textID = _textID;
            this.filePath = _filePath;
            this.textMetaData = _textMetaData;
            this.clauses = _clauses;
        }

        public Text(string _documentID, string _textID, string _filePath)
        {
            this.documentID = _documentID;
            this.textID = _textID;
            this.filePath = _filePath;
        }

        public Text(Document document, string _textID)
        {
            this.documentID = document.documentID;
            this.textID = _textID;
            this.filePath = document.filePath;
        }

        public Text()
        {

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