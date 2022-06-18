using CorpusDraftCSharp;
using System.Collections.Generic;

namespace CorpusDraftCSharp
{
    public class RealizationGroup : List<Realization>
    {
        public string documentID { get; set; }
        public string textID { get; set; }
        public string clauseID { get; set; }
        public RealizationGroup()
        {

        }
    }
}
