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
            var parallelManuscrupt = new ParallelManuscript();
            parallelManuscrupt.Id = directoryTextsInfo.GetFiles().Length.ToString();
            parallelManuscrupt.text = scriptToParallelize.text;
            parallelManuscrupt.tagging = scriptToParallelize.tagging;
            int maxClausesNumber = scriptToParallelize.subunits.SelectMany(t => t.subunits).Select(t => t.subunits.Count).Max();
            ParallelClause[,] parallelMatrix = new ParallelClause[scriptToParallelize.subunits.Count, maxClausesNumber];
            for (int i = 0; i < maxClausesNumber; i++)
            {
                for (int j = 0; j < docToParallelize.texts.Count; j++)
                {
                    if (docToParallelize.texts[j].clauses.Count > i)
                    {
                        parallelMatrix[i, j] = new ParallelClause
                        {
                            textName = docToParallelize.texts[j].textName,
                            textMetaData = docToParallelize.texts[j].textMetaData,
                            clause = docToParallelize.texts[j].clauses[i]
                        };
                        continue;
                    }
                    parallelMatrix[i, j] = new ParallelClause
                    {
                        textName = docToParallelize.texts[j].textName,
                        textMetaData = docToParallelize.texts[j].textMetaData,
                        clause = null
                    };
                }
            }
            parallelDocument.parallelClauses = parallelMatrix;
            string documentInJSON = JsonConvert.SerializeObject(parallelDocument, Formatting.Indented);
            var documentDBFile = Path.Combine(dirTexts, directoryTextsInfo.GetFiles().Length.ToString() + "_" + docToParallelize.documentName + ".json");
            FileStream fs = new FileStream(documentDBFile, FileMode.Create);
            using (StreamWriter w = new StreamWriter(fs))
            {
                w.Write(documentInJSON);
            }
        }
    }
}
