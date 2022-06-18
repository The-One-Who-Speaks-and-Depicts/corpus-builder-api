using System.Collections.Generic;

namespace CorpusDraftCSharp
{
    public record DecomposedClause
    {
        public string clause {get; init;}
        public List<DecomposedToken> tokens {get; init;}
    }
}
