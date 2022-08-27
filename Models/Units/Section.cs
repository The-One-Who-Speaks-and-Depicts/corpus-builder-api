using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ManuscriptsProcessor.Values;

namespace ManuscriptsProcessor.Units
{
    [Serializable]
    public class Section : ICorpusUnit, IUnitGroup<Segment>, IComparable<Section>
    {


	    #region objectValues
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string text { get; set; }
        [JsonProperty]
	    public List<Dictionary<string, List<Value>>> tagging { get; set; }
        [JsonProperty]
        public List<Segment> subunits { get; set; }
        #endregion

        #region Constructors

        [JsonConstructor]
        public Section(string _textID, string _textName, List<Dictionary<string, List<Value>>> _textMetaData, List<Segment> _clauses)
        {
            this.Id = _textID;
            this.text = _textName;
            this.tagging = _textMetaData;
            this.subunits = _clauses;
        }

        public Section(string _textID, string _textName)
        {
            this.Id = _textID;
            this.text = _textName;
        }

        public Section(Manuscript document, string _textID, string _textName)
        {
            this.Id = document.Id + "|" + _textID;
            this.text = _textName;
        }

        public Section()
        {

        }


	#endregion



    #region PublicMethods
        public string Jsonize()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }

        public string Output()
        {
            if (tagging is null || tagging.Count < 1)
            {
                return "<span title= \"\" data-content=\"\" class=\"" + this.GetType().Name + "\" id=\"" + Id + "\"> " + String.Join('\n', subunits.Select(x => x.Output())) + "</span><br />";
            }
            return "<span title=\"" + MyExtensions.GetFieldsInText(tagging) + "\" data-content=\"" + MyExtensions.GetFieldsInText(tagging).Replace("\n", "<br />") + "\" class=\"" + this.GetType().Name + "\" id=\"" + Id + "\"> " + String.Join('\n', subunits.Select(x => x.Output()))+ "</span><br />";
        }

        public int CompareTo(Section other)
        {
            return MyExtensions.CompareIds(Id, other.Id);
        }

        #endregion
    }
}
