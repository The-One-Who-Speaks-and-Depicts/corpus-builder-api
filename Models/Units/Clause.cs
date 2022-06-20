using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Clause : ICorpusUnit, IUnitGroup<Token>, IComparable<Clause>
    {


        #region objectValues
        [JsonProperty]
        public string filePath { get; set; }
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> tagging { get; set; }
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string text { get; set; }
        [JsonProperty]
        public List<Token> subunits { get; set; }
        #endregion


        #region Constructors
        [JsonConstructor]
        public Clause(string _filePath, string _clauseID, string _clauseText, List<Dictionary<string, List<Value>>> _clauseFields, List<Token> _realizations)
        {
            this.filePath = _filePath;
            this.Id = _clauseID;
            this.text = _clauseText;
            this.tagging = _clauseFields;
            this.subunits = _realizations;
        }
        public Clause(string _filePath, string _clauseID, string _clauseText)
        {
            this.filePath = _filePath;
            this.Id = _clauseID;
            this.text = _clauseText;
        }

        public Clause(Section text, string _clauseID, string _clauseText)
        {
            this.Id = text.Id + "|" + _clauseID;
            this.filePath = text.filePath;
            this.text = _clauseText;
        }
        public Clause()
        {

        }

        #endregion

        #region publicMethods
        public string Output()
        {
            return MyExtensions.UnitOutput(this);
        }
        public string Jsonize()
        {
            string jsonedClause = JsonConvert.SerializeObject(this, Formatting.Indented);
            return jsonedClause;
        }

        public int CompareTo(Clause other)
        {
            return MyExtensions.CompareIds(Id, other.Id);
        }
        #endregion
    }
}
