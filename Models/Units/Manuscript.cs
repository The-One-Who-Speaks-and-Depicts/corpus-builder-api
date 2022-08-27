using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ManuscriptsProcessor.Values;

namespace ManuscriptsProcessor.Units
{
    public class Manuscript : ICorpusUnit, IUnitGroup<Section>, IComparable<Manuscript>
    {


        #region objectValues
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string text { get; set; }
        [JsonProperty]
        public string filePath;
        [JsonProperty]
        public string googleDocPath;
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> tagging { get; set; }
        [JsonProperty]
        public List<Section> subunits { get; set; }
        #endregion

        #region Constructors

        [JsonConstructor]
        public Manuscript(string _documentID, string _documentName, string _filePath, string _googleDocPath, List<Dictionary<string, List<Value>>> _documentMetaData, List<Section> _texts)
        {
            Id = _documentID;
            text = _documentName;
            filePath = _filePath;
            googleDocPath = _googleDocPath;
            tagging = _documentMetaData;
            subunits = _texts;
        }

        public Manuscript(string _documentID, string _documentName, string _filePath, string _googleDocPath)
        {
            Id = _documentID;
            text = _documentName;
            filePath = _filePath;
            googleDocPath = _googleDocPath;
        }

        public Manuscript()
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
        #endregion

        public int CompareTo(Manuscript other)
        {
            return MyExtensions.CompareIds(Id, other.Id);
        }



    }
}
