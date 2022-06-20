using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CorpusDraftCSharp
{
    [Serializable]
    public class Field
    {
        [JsonProperty]
        public string name { get; private set; }
        [JsonProperty]
        public List<object> values { get; private set; } = new List<object>();
        [JsonProperty]
        public string description { get; private set;  }
        [JsonProperty]
        public bool isUserFilled { get; private set; } = true;
        [JsonProperty]
        public bool isMultiple { get; private set; } = false;
        [JsonProperty]
        public string type { get; private set; }
        [JsonProperty]
        public Dictionary<string, List<string>> connectedFields { get; set; } = new Dictionary<string, List<string>>();

        [JsonConstructor]
        Field(string _name, List<object> _values, string _description,  bool _isUserFilled, bool _isMultiple, string _type, Dictionary<string, List<string>> _connectedFields)
        {
            this.name = _name;
            this.values = _values;
            this.description = _description;
            this.isUserFilled = _isUserFilled;
            this.isMultiple = _isMultiple;
            this.type = _type;
            this.connectedFields = _connectedFields;
        }

        protected Field()
        {

        }

        public Field(string _name, string _description)
        {
            this.name = _name;
            this.description = _description;
        }

        public void ChangeName(string _name)
        {
            this.name = _name;
        }

        public void ChangeDesc(string _description)
        {
            this.description = _description;
        }


        public void AddValue(object _value)
        {
            this.values.Add(_value);
        }


        public void RemoveValue(object _value)
        {
            if (this.values.Count > 0)
            {
                this.values.Remove(_value);
            }
            else
            {
                throw new Exception("В поле нет значений!");
            }
        }

        public void MakeUserFilled()
        {
            this.isUserFilled = true;
        }

        public void MakeRestricted()
        {
            this.isUserFilled = false;
        }

        public void Multiply()
        {
            this.isMultiple = true;
        }

        public void MakeSingle()
        {
            this.isMultiple = false;

        }

        public void changeType(string _type)
        {
            this.type = _type;
        }

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            return realizationToJson;
        }

    }

}
