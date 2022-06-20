using System.Collections.Generic;
using System;

namespace CorpusDraftCSharp
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
