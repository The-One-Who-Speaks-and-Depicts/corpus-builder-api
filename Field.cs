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
        public bool isActive { get; private set; } = true;
        [JsonProperty]
        public bool isMultiple { get; private set; } = false;

        [JsonConstructor]
        Field(string _name, List<object> _values, string _description,  bool _isActive, bool _isMultiple)
        {
            this.name = _name;
            this.values = _values;
            this.description = _description;
            this.isActive = _isActive;
            this.isMultiple = _isMultiple;
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
            if (this.isActive)
            {
                if (this.values.Count > 0)
                {
                    if (this.isMultiple)
                    {
                        this.values.Add(_value);
                    }
                    else
                    {
                        throw new Exception("В поле допустимо только одно значение!");
                    }
                }
                else
                {
                    this.values.Add(_value);
                }
            }
            else
            {
                throw new Exception("Попытка добавить значение в неактивное поле!"); 
            }
            
        }

        public void RemoveValue(object _value)
        {
            if (this.isActive)
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
            else
            {
                throw new Exception("Попытка удалить значение из неактивного поля!");
            }
        }

        public void Activate()
        {
            this.isActive = true;
        }
        
        public void Deactivate()
        {
            this.isActive = false;
        }

        public void Multiply()
        {
            this.isMultiple = true;
        }

        public void MakeSingle()
        {
            try
            {
                if (this.values.Count > 1)
                {
                    throw new Exception("Сначала необходимо оставить только одно значение!");
                }
                else
                {
                    this.isMultiple = false;
                }
            }
            catch
            {
                this.isMultiple = false;
            }
                        
        }

        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this);
            return realizationToJson;
        }

    }

}
