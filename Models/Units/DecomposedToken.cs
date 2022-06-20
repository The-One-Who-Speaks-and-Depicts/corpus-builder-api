using System.Collections.Generic;

namespace ManuscriptsProcessor.Units
{
    public record DecomposedToken
    {
        public string lexemeOne {get; init;}
        public string lexemeTwo {get; init;}
        public List<string> graphemes {get; init;}
    }
}
