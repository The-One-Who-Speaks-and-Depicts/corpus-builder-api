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
        public string filePath { get; set; }
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
        public Token(string _filePath, List<Dictionary<string, List<Value>>> _fields, string _realizationID, string _lexemeOne, string _lexemeTwo, List<Grapheme> _letters)
        {
            this.filePath = _filePath;
            this.tagging = _fields;
            this.Id = _realizationID;
            this.lexemeView = _lexemeOne;
            this.text = _lexemeTwo;
            this.subunits = _letters;
        }

        public Token(Clause clause, string _realizationID, string _lexemeOne, string _lexemeTwo)
        {
            this.filePath = clause.filePath;
            this.Id = clause.Id + "|" +_realizationID;
            this.lexemeView =_lexemeOne;
            this.text = _lexemeTwo;
        }


        public Token(string _filePath, string _realizationID, string _lexemeOne, string _lexemeTwo)
        {
            this.filePath = _filePath;
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
            return MyExtensions.UnitOutput(this);
        }

        public string KeyOutput()
        {
            return MyExtensions.UnitOutput(this, true);
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
