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
    public Text (string _documentID, string _textID, string _filepath)
    {
            this.documentID = _documentID;
            this.textID = _textID;
            this.filePath = _filepath;
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