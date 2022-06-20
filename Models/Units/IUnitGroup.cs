using System.Collections.Generic;

namespace ManuscriptsProcessor.Units
{
    public interface IUnitGroup<T> where T: ICorpusUnit
    {
        public List<T> subunits { get; set; }
    }
}
