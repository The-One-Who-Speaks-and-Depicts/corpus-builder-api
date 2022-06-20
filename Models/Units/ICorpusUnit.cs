using System.Collections.Generic;
using ManuscriptsProcessor.Values;
using System;

namespace ManuscriptsProcessor.Units
{
    public interface ICorpusUnit
    {
        public string Id {get; set;}
        public string text { get; set; }
        public List<Dictionary<string, List<Value>>> tagging { get; set; }

        public string Output();
        public string Jsonize();
    }
}
