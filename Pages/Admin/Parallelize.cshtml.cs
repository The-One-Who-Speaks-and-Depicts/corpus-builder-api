using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CorpusDraftCSharp;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;

namespace CroatianProject.Pages
{
    public class ParallelizeModel : PageModel
    {
        private IHostingEnvironment _environment;
        public string documentPicked { get; set; }
        public List<string> documentNames
        {
            get
            {
                List<Document> deserializedDocuments = new List<Document>();
                try
                {
                    DirectoryInfo docDirectory = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents"));
                    var jsonedDocuments = docDirectory.GetFiles();
                    foreach (var doc in jsonedDocuments)
                    {
                        using (StreamReader r = new StreamReader(doc.FullName))
                        {
                            deserializedDocuments.Add(JsonConvert.DeserializeObject<Document>(r.ReadToEnd()));
                        }
                    }
                    return deserializedDocuments.Select(d => d.documentID + "_" + d.documentName).ToList();
                }
                catch
                {
                    return new List<string>();
                }

            }
        }

        public ParallelizeModel(IHostingEnvironment environment)
        {
            _environment = environment;
        }
        public void OnPostParallelize(string documentPicked)
        {
            var dirTexts = Path.Combine(_environment.ContentRootPath, "database", "parallelizedDocuments");
            Directory.CreateDirectory(dirTexts);
            Document docToParallelize = new Document();
            using (StreamReader r = new StreamReader(new FileStream(Path.Combine(_environment.ContentRootPath, "database", "documents", documentPicked + ".json"), FileMode.Open)))
            {
                docToParallelize = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
            }
            DirectoryInfo directoryTextsInfo = new DirectoryInfo(dirTexts);
            ParallelDocument parallelDocument = new ParallelDocument();
            parallelDocument.id = directoryTextsInfo.GetFiles().Length.ToString();
            parallelDocument.name = docToParallelize.documentName;
            parallelDocument.documentMetaData = docToParallelize.documentMetaData;
            int maxClausesNumber = docToParallelize.texts.Select(t => t.clauses.Count).Max();
            ParallelClause[,] parallelMatrix = new ParallelClause[maxClausesNumber, docToParallelize.texts.Count];
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
