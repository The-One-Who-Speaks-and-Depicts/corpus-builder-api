using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuscriptsProcessor.Units;
using Newtonsoft.Json;
using ManuscriptsProcessor;
namespace CroatianProject.Pages
{
    public class ParallelizeModel : PageModel
    {
        private IWebHostEnvironment _environment;
        public string manuscriptPicked { get; set; }
        public List<string> manuscriptsNames
        {
            get
            {
                return MyExtensions.GetManuscripts(Path.Combine(_environment.ContentRootPath, "database", "parallelizedManuscripts"));
            }
        }

        public ParallelizeModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public void OnPostParallelize(string documentPicked)
        {
            var dirTexts = Path.Combine(_environment.ContentRootPath, "database", "parallelizedManuscripts");
            Directory.CreateDirectory(dirTexts);
            var scriptToParallelize = new Manuscript();
            using (StreamReader r = new StreamReader(new FileStream(Path.Combine(_environment.ContentRootPath, "database", "manuscripts", documentPicked + ".json"), FileMode.Open)))
            {
                scriptToParallelize = JsonConvert.DeserializeObject<Manuscript>(r.ReadToEnd());
            }
            DirectoryInfo directoryTextsInfo = new DirectoryInfo(dirTexts);
            var parallelManuscript = new ParallelManuscript();
            parallelManuscript.Id = directoryTextsInfo.GetFiles().Length.ToString();
            parallelManuscript.text = scriptToParallelize.text;
            parallelManuscript.tagging = scriptToParallelize.tagging;
            int maxClausesNumber = scriptToParallelize.subunits.SelectMany(t => t.subunits).Select(t => t.subunits.Count).Max();
            ParallelClause[,] parallelMatrix = new ParallelClause[scriptToParallelize.subunits.Count, maxClausesNumber];
            for (int i = 0; i < scriptToParallelize.subunits.Count; i++)
            {
                var sectionClauses = scriptToParallelize.subunits[i].subunits.SelectMany(t => t.subunits).ToList();
                for (int j = 0; j < maxClausesNumber; j++)
                {
                    if (sectionClauses[j] is null) break;
                    parallelMatrix[i, j] = new ParallelClause
                    {
                        text = scriptToParallelize.subunits[i].text,
                        tagging = scriptToParallelize.subunits[i].tagging,
                        clause = sectionClauses[j]
                    };
                }
            }
            parallelManuscript.parallelClauses = parallelMatrix;
            string scriptInJSON = JsonConvert.SerializeObject(parallelManuscript, Formatting.Indented);
            var scriptDBFile = Path.Combine(dirTexts, directoryTextsInfo.GetFiles().Length.ToString() + "_" + scriptToParallelize.text + ".json");
            FileStream fs = new FileStream(scriptDBFile, FileMode.Create);
            using (StreamWriter w = new StreamWriter(fs))
            {
                w.Write(scriptInJSON);
            }
        }
    }
}
