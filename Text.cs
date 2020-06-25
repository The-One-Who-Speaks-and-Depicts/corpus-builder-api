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
    [JsonIgnore]
    protected Dictionary<string, DataTable> spreadsheets;
    [JsonProperty]
    public string filePath;
    [JsonProperty]
    public string textID;
    [JsonIgnore]
	protected List<string> clauseIDs;
    [JsonIgnore]
	protected Dictionary<string, string> textFields;
    [JsonIgnore]
    public readonly List<Clause> clauses;
	#endregion	
	
	#region Constructors
    
    [JsonConstructor]
    public Text (string textNumber, string _textID, string _filepath)
    {
            this.documentID = textNumber;
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