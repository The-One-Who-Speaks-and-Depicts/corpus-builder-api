using System;
using System.Collections.Generic;

namespace CorpusDraftCSharp 
{    
	public interface IValue
    {
        string name { get; set; }
        List<int> letters { get; set; }
        List<Realization> connectedRealizations { get; set; }
        string Jsonize();
    }
}