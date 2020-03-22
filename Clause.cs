using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Clause
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
        protected static FieldDummy clauseTextField
        {
            get
            {
                return _clauseTextField;
            }
            set
            {
                _clauseTextField = value;
            }
        }
        protected static FieldDummy _clauseTextField;
        protected static List<FieldDummy> fieldDummies = new List<FieldDummy>();
        protected static FieldDummy realizationIDsField
        {
            get
            {
                return _realizationIDsField;
            }
            set
            {
                _realizationIDsField = value;
            }
        }
        protected static FieldDummy _realizationIDsField;
        
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
        protected Dictionary<string, string> clauseFields;
        [JsonProperty]
        public string clauseID;
        [JsonProperty]
        public string clauseText;
        [JsonIgnore]
        protected List<string> realizationIDs;
        [JsonIgnore]
        public readonly List<Realization> realizations;
        [JsonIgnore]
        private double clauseBrokenness = 0;
        [JsonIgnore]
        private int wordCount = 0;
        #endregion


        #region Constructors
        public Clause(Text text, string _clauseID, string _clauseText)
        {
            this.documentID = text.documentID;
            this.filePath = text.filePath;
            this.textID = text.textID;
            this.clauseID = _clauseID;
            this.clauseText = _clauseText;
        }
        public Clause(string documentID, Dictionary<string, DataTable> spreadsheets, string filePath, string textID, string clauseID)
        {
            Console.WriteLine("Starting clause generation. Clause number is {0}, ID will be {2}{1}{0}", clauseID, textID, documentID);
            this.documentID = documentID;
            this.spreadsheets = spreadsheets;
            this.filePath = filePath;
            this.textID = textID;
            this.clauseID = clauseID;
            Console.WriteLine("Getting clause graphic forms...");
            if (clauseTextField == null)
            {
                clauseTextField = new FieldDummy("clause", MyExtensions.InsertString("name of table where clauses are stored"), 
                    MyExtensions.InsertString("name of column where clauses are stored(name is usually contained in the first row of your file)"),
                    int.Parse(MyExtensions.InsertString("number of field where clause name is stored")));
            }
            this.clauseText = MyExtensions.SingleFieldIntoList(spreadsheets, "clause text data", "clause", clauseID, clauseTextField.tableName, clauseTextField.colName, clauseTextField.colNumber)[0];
           
            this.clauseFields = new Dictionary<string, string>();
            if ((fieldDummies.Count < 1) && (!_areFieldsAdded))
            {
                _areFieldsAdded = true;
                bool createFields = true;
                while(createFields) { 
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
                            MyExtensions.InsertString("name of column where clauses are stored (name is usually contained in the first row of your file)"),
                            MyExtensions.KeyCreation("desired field data field number")));

                }
                }
            }
            if (fieldDummies.Count > 0)
            {            
                foreach (FieldDummy field in fieldDummies)
                { 
                    this.clauseFields = MyExtensions.CreateAdditionalFields(clauseID, spreadsheets, "clause data", field.fieldName, field.tableName, field.colName, field.colNumber);
                }
            }
            Console.WriteLine("Getting realizations...");
            if (realizationIDsField == null)
            {
                realizationIDsField = new FieldDummy("realizationIDs", MyExtensions.InsertString("name of table where realization data are stored"), 
                    MyExtensions.InsertString("name of column where clause indexes are stored"), MyExtensions.KeyCreation("number of column where realizations are stored"));
            }
            this.realizationIDs = MyExtensions.SingleFieldIntoList(spreadsheets, "realization indexes", "clause indexes", clauseID, realizationIDsField.tableName, 
               realizationIDsField.colName, realizationIDsField.colNumber);            
            this.realizations = GenerateRealizations(realizationIDs);
        }
        #endregion

        #region publicMethods
        public string Jsonize()
        {
            string jsonedClause = JsonConvert.SerializeObject(this);
            return jsonedClause;
        }
        #endregion

        #region PrivateMethods
        private List<Realization> GenerateRealizations(List<string> realizationIDs)
        {
            List<Realization> realizations = new List<Realization>();
            int counter = 0;
            if (realizationIDs.Count < 1)
            {
                Console.WriteLine("Clause is not parsed");
                this.wordCount = 0;
                this.clauseBrokenness = 100;
            }
            else
            {          
            while (counter < realizationIDs.Count)
            {
                Realization realizationToValidate = new Realization(this.spreadsheets, realizationIDs[counter], clauseID);
                try
                    {
                        int.Parse(realizationToValidate.partOfSpeech);
                        this.wordCount++;
                        this.clauseBrokenness++;
                    }
                catch
                    {
                        if (realizationToValidate.partOfSpeech == "Num")
                        {
                            this.wordCount++;
                        }
                        else
                        {
                            realizations.Add(new Realization(documentID, spreadsheets, filePath, textID, clauseID, realizationIDs[counter], realizationToValidate.partOfSpeech));
                            this.wordCount++;
                        }
                    }
                finally
                    { 
                counter++;
                    }
            }
            }
            this.clauseBrokenness = (1 - this.clauseBrokenness / this.wordCount) * 100;
            return realizations;
        }        
        #endregion
    }
}
