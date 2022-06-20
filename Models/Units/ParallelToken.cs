using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ManuscriptsProcessor.Units
{
    public class ParallelToken : List<RealizationGroup>
    {

        public List<RealizationGroup> GetParallels(RealizationGroup source)
        {
            var parallels = this.Where(r => r != source).ToList();
            return parallels;
        }
        public ParallelToken()
        {

        }
    }
}
