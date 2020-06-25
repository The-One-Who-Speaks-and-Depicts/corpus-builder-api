using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Clause
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
        
        #endregion

        #region publicMethods
        public string Jsonize()
        {
            string jsonedClause = JsonConvert.SerializeObject(this);
            return jsonedClause;
        }
        #endregion
        /*
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
        
        #endregion*/
    }
}
