using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using static CorpusDraftCSharp.MyExtensions;

namespace CorpusDraftCSharp
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
            return MyExtensions.UnitOutput(this);
        }
        #endregion

        public int CompareTo(Manuscript other)
        {
            return MyExtensions.CompareIds(Id, other.Id);
        }



    }
}
