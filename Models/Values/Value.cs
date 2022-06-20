using Newtonsoft.Json;

namespace ManuscriptsProcessor.Values
{
	public class Value
    {
        [JsonProperty]
        public string name { get; set; }


        [JsonConstructor]
        public Value(string _name)
        {
            this.name = _name;
        }
        public string Jsonize()
        {
            string realizationToJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            return realizationToJson;
        }
    }
}
