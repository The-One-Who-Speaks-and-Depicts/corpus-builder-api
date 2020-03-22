using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Realization
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
        protected static FieldDummy lemmaOneField
        {
            get
            {
                return _lemmaOneField;
            }
            set
            {
                _lemmaOneField = value;
            }
        }
        protected static FieldDummy _lemmaOneField;
        protected static FieldDummy lemmaTwoField
        {
            get
            {
                return _lemmaTwoField;
            }
            set
            {
                _lemmaTwoField = value;
            }
        }
        protected static FieldDummy _lemmaTwoField;
        protected static FieldDummy lexemeField
        {
            get
            {
                return _lexemeField;
            }
            set
            {
                _lexemeField = value;
            }
        }
        protected static FieldDummy _lexemeField;
        protected static FieldDummy partOfSpeechField
        {
            get
            {
                return _partOfSpeechField;
            }
            set
            {
                _partOfSpeechField = value;
            }
        }
        protected static FieldDummy _partOfSpeechField;
        protected static FieldDummy glossField
        {
            get
            {
                return _glossField;
            }
            set
            {
                _glossField = value;
            }
        }
        protected static FieldDummy _glossField;
        protected static List<FieldDummy> fieldDummies = new List<FieldDummy>();

        #endregion

        #region objectValues
        [JsonProperty]
        protected string documentID;
        [JsonIgnore]
        protected Dictionary<string, DataTable> spreadsheets;
        [JsonProperty]
        protected string filePath;
        [JsonProperty]
        protected string textID;
        [JsonProperty]
        protected string clauseID;
        [JsonIgnore]
        protected Dictionary<string, string> realizationFields;
        [JsonProperty]
        protected string realizationID;
        [JsonIgnore]
        protected string lemmaOne;
        [JsonIgnore]
        protected string lemmaTwo;
        [JsonProperty]
        public string lexeme;
        [JsonProperty]
        public string partOfSpeech;
        [JsonIgnore]
        protected string gloss;
        [JsonIgnore]
        protected string fullID;
        #endregion

        #region Constructors

        public Realization(Clause clause, string _realizationID, string _lexeme, string _PoS)
        {
            this.documentID = clause.documentID;
            this.filePath = clause.filePath;
            this.textID = clause.textID;
            this.clauseID = clause.clauseID;
            this.realizationID = _realizationID;
            this.lexeme = _lexeme;
            this.partOfSpeech = _PoS;
        }

        public Realization(string documentID, Dictionary<string, DataTable> spreadsheets, string filePath, string textID, string clauseID, string realizationID, string partOfSpeech)
        {
            Console.WriteLine("Generating realizations. Starting realization generation. Realization number is {0}, ID will be {1}{2}{3}{0}", realizationID, documentID, textID, clauseID);
            this.documentID = documentID;
            this.spreadsheets = spreadsheets;
            this.filePath = filePath;
            this.textID = textID;
            this.clauseID = clauseID;
            this.realizationID = realizationID;
            this.fullID = documentID + textID + clauseID + realizationID;
            this.partOfSpeech = partOfSpeech;
            Console.WriteLine("Getting realization graphic forms. Lemma 1...");                               
            if (lemmaOneField == null)
            {
                lemmaOneField = new FieldDummy("Lemma 1", MyExtensions.InsertString("name of table where realizations are stored"),
                    MyExtensions.InsertString("name of column where realization IDs are stored(name is usually contained in the first row of your file)"),
                    MyExtensions.InsertString("name of column where clause IDs are stored(name is usually contained in the first row of your file)"),
                    int.Parse(MyExtensions.InsertString("number of field where lemma one is stored")));
            }
            this.lemmaOne = MyExtensions.SingleFieldIntoListTwoCols(spreadsheets, realizationID, clauseID, lemmaOneField.tableName, lemmaOneField.colName, lemmaOneField.colTwoName, lemmaOneField.colNumber)[0];
            
            Console.WriteLine("Getting realization graphic forms. Lemma 2...");
            if (lemmaTwoField == null)
            {
                lemmaTwoField = new FieldDummy("Lemma 2", MyExtensions.InsertString("name of table where realizations are stored"),
                    MyExtensions.InsertString("name of column where realization IDs are stored(name is usually contained in the first row of your file)"),
                    MyExtensions.InsertString("name of column where clause IDs are stored(name is usually contained in the first row of your file)"),
                    int.Parse(MyExtensions.InsertString("number of field where lemma two is stored")));
            }
            this.lemmaTwo = MyExtensions.SingleFieldIntoListTwoCols(spreadsheets, realizationID, clauseID, lemmaTwoField.tableName, lemmaTwoField.colName, lemmaTwoField.colTwoName, lemmaTwoField.colNumber)[0];
            
            Console.WriteLine("Getting lexeme...");
            if (lexemeField == null)
            {
                lexemeField = new FieldDummy("Lexeme", MyExtensions.InsertString("name of table where realizations are stored"),
                    MyExtensions.InsertString("name of column where realization IDs are stored(name is usually contained in the first row of your file)"),
                    MyExtensions.InsertString("name of column where clause IDs are stored(name is usually contained in the first row of your file)"),
                    int.Parse(MyExtensions.InsertString("number of field where lexeme is stored")));
            }
            this.lexeme = MyExtensions.SingleFieldIntoListTwoCols(spreadsheets, realizationID, clauseID, lexemeField.tableName, lexemeField.colName, lexemeField.colTwoName, lexemeField.colNumber)[0];

            Console.WriteLine("Getting gloss...");
            if (glossField == null)
            {
                glossField = new FieldDummy("Gloss", MyExtensions.InsertString("name of table where realizations are stored"),
                    MyExtensions.InsertString("name of column where realization IDs are stored(name is usually contained in the first row of your file)"),
                    MyExtensions.InsertString("name of column where clause IDs are stored(name is usually contained in the first row of your file)"),
                    int.Parse(MyExtensions.InsertString("number of field where gloss is stored")));
            }
            this.gloss = MyExtensions.SingleFieldIntoListTwoCols(spreadsheets, realizationID, clauseID, glossField.tableName, glossField.colName, glossField.colTwoName, glossField.colNumber)[0];

            this.realizationFields = new Dictionary<string, string>();
            if ((fieldDummies.Count < 1) && (!_areFieldsAdded))
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
                                MyExtensions.InsertString("name of column where realizations are stored (name is usually contained in the first row of your file)"),
                                MyExtensions.InsertString("name of column where clause IDs are stored(name is usually contained in the first row of your file)"),
                                MyExtensions.KeyCreation("desired field data field number")));

                    }
                }
            }
            if (fieldDummies.Count > 0)
            {
                foreach (FieldDummy field in fieldDummies)
                {
                    this.realizationFields = MyExtensions.CreateAdditionalFieldsTwoCols(realizationID, clauseID, spreadsheets, field.fieldName, field.tableName, field.colName, field.colTwoName, field.colNumber);
                }
            }
        }

        public Realization (Dictionary<string, DataTable> spreadsheets, string realizationID, string clauseID)
        {
            if (partOfSpeechField == null)
            {
                partOfSpeechField = new FieldDummy("part of speech", MyExtensions.InsertString("realization table"), MyExtensions.InsertString("realization column"), 
                    MyExtensions.InsertString("clause column"), int.Parse(MyExtensions.InsertString("number of column with part of speech")));
            }
            this.partOfSpeech = MyExtensions.SingleFieldIntoListTwoCols(spreadsheets, realizationID, clauseID, 
                partOfSpeechField.tableName, partOfSpeechField.colName, partOfSpeechField.colTwoName, partOfSpeechField.colNumber)[0];
            if (String.IsNullOrEmpty(partOfSpeech) || String.IsNullOrWhiteSpace(partOfSpeech))
            {                
                Console.WriteLine("Part of speech is not given. The information about the gloss is presented below");
                DataTable textSheet = spreadsheets[partOfSpeechField.tableName];
                string expression = partOfSpeechField.colName + " = " + "'" + realizationID + "'" + " and " + partOfSpeechField.colTwoName + " = " + "'" + clauseID + "'";
                DataRow[] foundRows;
                foundRows = textSheet.Select(expression);
                foreach (DataRow row in foundRows) 
                {
                   
                        for (int j = 0; j < textSheet.Columns.Count; j++)
                        {
                            Console.Write(row[j] + " ");
                        }
                        Console.WriteLine();
                    
                }
                Console.WriteLine("Please add part of speech now. 0 if the gloss is fragment, Num if it is a number, N for Noun, V for Verb etc.");
                this.partOfSpeech = Console.ReadLine();
            }
            
        }
        #endregion

        #region publicMethods

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this);
            return realizationToJson;
        }

        #endregion

        #region privateMethods
        #endregion

    }
}
