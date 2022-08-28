using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ManuscriptsProcessor.Values;

namespace ManuscriptsProcessor.Units
{
    [Serializable]
    public class Token : ICorpusUnit, IUnitGroup<Grapheme>, IEquatable<Token>, IComparable<Token>
    {

        #region objectValues
        [JsonProperty]
        public List<Dictionary<string, List<Value>>> tagging { get; set; }
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string lexemeView { get; set; }
        [JsonProperty]
        public string text { get; set; }
        [JsonProperty]
        public List<Grapheme> subunits { get; set; }
        #endregion

        #region Constructors

        [JsonConstructor]
        public Token(List<Dictionary<string, List<Value>>> _fields, string _realizationID, string _lexemeOne, string _lexemeTwo, List<Grapheme> _letters)
        {
            this.tagging = _fields;
            this.Id = _realizationID;
            this.lexemeView = _lexemeOne;
            this.text = _lexemeTwo;
            this.subunits = _letters;
        }

        public Token(Clause clause, string _realizationID, string _lexemeOne, string _lexemeTwo)
        {
            this.Id = clause.Id + "|" +_realizationID;
            this.lexemeView =_lexemeOne;
            this.text = _lexemeTwo;
        }


        public Token(string _realizationID, string _lexemeOne, string _lexemeTwo)
        {
            this.Id = _realizationID;
            this.lexemeView = _lexemeOne;
            this.text = _lexemeTwo;
        }

        public Token()
        {

        }


        #endregion

        #region publicMethods

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            return realizationToJson;
        }

        public string Output()
        {
            if (tagging is null || tagging.Count < 1)
            {
                return "<span title= \"\" data-content=\"\" class=\"" + this.GetType().Name + "\" id=\"" + Id + "\"> " + String.Join("", subunits.Select(x => x.Output())) + "</span>";
            }
            return "<span title=\"" + MyExtensions.GetFieldsInText(tagging) + "\" data-content=\"" + MyExtensions.GetFieldsInText(tagging).Replace("\n", "<br />") + "\" class=\"" + this.GetType().Name + "\" id=\"" + Id + "\"> " + String.Join("", subunits.Select(x => x.Output()))+ "</span>";
        }

        public string KeyOutput()
        {
            if (tagging is null || tagging.Count < 1)
            {
                return "<span title= \"\" data-content=\"\" class=\"" + this.GetType().Name + "\" id=\"" + Id + "\"> " + text + "</span>";
            }
            return "<span title=\"" + MyExtensions.GetFieldsInText(tagging) + "\" data-content=\"" + MyExtensions.GetFieldsInText(tagging).Replace("\n", "<br />") + "\" class=\"" + this.GetType().Name + "\" id=\"" + Id + "\"> " + text + "</span>";
        }

        public bool Equals(Token other)
        {
            return Id == other.Id;
        }

        public int CompareTo(Token other)
        {
            return MyExtensions.CompareIds(Id, other.Id);
        }

        #endregion

        #region privateMethods
        #endregion

    }
}
