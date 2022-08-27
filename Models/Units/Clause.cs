using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ManuscriptsProcessor.Values;
namespace ManuscriptsProcessor.Units
{
    [Serializable]
    public class Clause : ICorpusUnit, IUnitGroup<Token>, IComparable<Clause>
    {


        #region objectValues
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
        public Clause(string _clauseID, string _clauseText, List<Dictionary<string, List<Value>>> _clauseFields, List<Token> _realizations)
        {
            this.Id = _clauseID;
            this.text = _clauseText;
            this.tagging = _clauseFields;
            this.subunits = _realizations;
        }
        public Clause(string _clauseID, string _clauseText)
        {
            this.Id = _clauseID;
            this.text = _clauseText;
        }

        public Clause(Segment segment, string _clauseID, string _clauseText)
        {
            this.Id = segment.Id + "|" + _clauseID;
            this.text = _clauseText;
        }
        public Clause()
        {

        }

        #endregion

        #region publicMethods
        public string Output()
        {
            if (tagging is null || tagging.Count < 1)
            {
                return "<span title= \"\" data-content=\"\" class=\"" + this.GetType().Name + "\" id=\"" + Id + "\"> " + String.Join(' ', subunits.Select(x => x.Output())) + "</span>";
            }
            return "<span title=\"" + MyExtensions.GetFieldsInText(tagging) + "\" data-content=\"" + MyExtensions.GetFieldsInText(tagging).Replace("\n", "<br />") + "\" class=\"" + this.GetType().Name + "\" id=\"" + Id + "\"> " + String.Join(' ', subunits.Select(x => x.Output()))+ "</span>";
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
