using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{ 
[Serializable]
public class Text 
{    
    #region classValues
    protected static bool areFieldsAdded
        {
            get
            {
                return _areFieldsAdded;
            }
            set
            {
                _areFieldsAdded = value;
            }
        }
    protected static bool _areFieldsAdded = false;
    internal static List<FieldDummy> fieldDummies = new List<FieldDummy>();
    internal  static FieldDummy clauseIDsField
        {
            get
            {
                return _clauseIDsField;
            }
            set
            {
                _clauseIDsField = value;
            }
        }
    internal static FieldDummy _clauseIDsField;
    #endregion

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
	public Text(string documentID, Dictionary<string, DataTable> spreadsheets, string filePath,	string textID)
	{
		Console.WriteLine("Starting text generation. Text number is {0}, ID will be {1}{0}", textID, documentID);
		this.documentID = documentID;
		this.spreadsheets = spreadsheets;		
		this.filePath = filePath;		
		this.textID = textID;		
		this.clauseIDs = MyExtensions.SingleFieldIntoList(spreadsheets, "clause indexes", "text indexes", textID, MyExtensions.InsertString("name of sheet with clauses"), MyExtensions.InsertString("columns with text nums"),
            int.Parse(MyExtensions.InsertString("columns with clause nums")));	
		this.textFields = new Dictionary<string, string>();
        if ((!areFieldsAdded) && (fieldDummies.Count < 1))
            {
                _areFieldsAdded = true;
                bool createFields = true;
                while (createFields)
                {
                    Console.WriteLine("Do you want to add fields? [Y/N]");
                    string decision_to_quit = Console.ReadLine();
                    string negative = "N";
                    if (decision_to_quit == negative)
                    {
                        createFields = false;
                    }
                    else
                    {
                        fieldDummies.Add(new FieldDummy(MyExtensions.InsertString("names of field you want to create"), MyExtensions.InsertString("name of table where desired field data are stored"),
                                MyExtensions.InsertString("name of column where text indexes are stored (name is usually contained in the first row of your file)"),
                                MyExtensions.KeyCreation("desired field data field number")));

                    }
                }
            }
		if (fieldDummies.Count > 0)
            {            
                foreach (FieldDummy field in fieldDummies)
                { 
                    this.textFields = MyExtensions.CreateAdditionalFields(textID, spreadsheets, "clause data", field.fieldName, field.tableName, field.colName, field.colNumber);
                }
            } 
        this.clauses = GenerateClauses(this.clauseIDs);        
	}
	#endregion
	
	#region PrivateMethods
	private List<Clause> GenerateClauses(List<string> clauseIDs)
    {
        List<Clause> clauses = new List<Clause>();
        int counter = 0;
        while (counter < clauseIDs.Count)
        {
            clauses.Add(new Clause(documentID, spreadsheets, filePath, textID, clauseIDs[counter]));
            counter++;
        }
        return clauses;
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