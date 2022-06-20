using System.Collections.Generic;

namespace CorpusDraftCSharp
{
    public record DecomposedToken
    {
        public string lexemeOne {get; init;}
        public string lexemeTwo {get; init;}
        public List<string> graphemes {get; init;}
    }
}
