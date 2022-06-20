using System.Collections.Generic;

namespace CorpusDraftCSharp
{
    public record DecomposedSegment
    {
        public string clause {get; init;}
        public List<DecomposedToken> tokens {get; init;}
    }
}
