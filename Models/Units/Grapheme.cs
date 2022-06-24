﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ManuscriptsProcessor.Values;

namespace ManuscriptsProcessor.Units
{
    [Serializable]
    public class Grapheme : ICorpusUnit, IComparable<Grapheme>
    {
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> tagging { get; set; }
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string text { get; set; }


        [JsonConstructor]
        public Grapheme(List<Dictionary<string, List<Value>>> _fields, string _graphemeID, string _grapheme)
        {
            this.tagging = _fields;
            this.Id = _graphemeID;
            this.text = _grapheme;
        }

        public Grapheme(Token realization, string _graphemeID, string _grapheme)
        {
            this.Id = realization.Id + "|" + _graphemeID;
            this.text = _grapheme;
        }

        public Grapheme(string _graphemeID, string _grapheme)
        {
            this.Id = _graphemeID;
            this.text = _grapheme;
        }

        public Grapheme()
        {

        }

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            return realizationToJson;
        }

        public string Output()
        {
            return MyExtensions.UnitOutput(this);
        }
        public int CompareTo(Grapheme other)
        {
            return MyExtensions.CompareIds(Id, other.Id);
        }
    }
}
