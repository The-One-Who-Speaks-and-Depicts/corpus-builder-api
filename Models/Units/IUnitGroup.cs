using System.Collections.Generic;

namespace CorpusDraftCSharp
{
    public interface IUnitGroup<T> where T: ICorpusUnit
    {
        public List<T> subunits { get; set; }
    }
}
