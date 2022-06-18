using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using CorpusDraftCSharp;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System;

namespace CroatianProject.Pages
{
    public class ParallelizeTokensModel : PageModel
    {
        [BindProperty]
        public List<List<string>> parallelizedClauses {get; set;} = new List<List<string>>();
        [BindProperty]
        public List<string> parallelizedTokens {get; set;} = new List<string>();
        [BindProperty]
        public List<ParallelDocument> documents
        {
            get
            {
                List<ParallelDocument> deserializedDocuments = new List<ParallelDocument>();
                try
                {
                    DirectoryInfo docDirectory = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "parallelizedDocuments"));
                    var jsonedDocuments = docDirectory.GetFiles();
                    foreach (var doc in jsonedDocuments)
                    {
                        using (StreamReader r = new StreamReader(doc.FullName))
                        {
                            deserializedDocuments.Add(JsonConvert.DeserializeObject<ParallelDocument>(r.ReadToEnd()));
                        }
                    }
                    return deserializedDocuments;
                }
                catch
                {
                    return deserializedDocuments;
                }

            }
        }

        [BindProperty]
        public string documentPicked { get; set; }

        [BindProperty]
        public string sequenceOfParallelTokens {get; set;}

        private IWebHostEnvironment _environment;
        public ParallelizeTokensModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        public void OnPost(string documentPicked)
        {
            var id = documentPicked.Split('[')[1].Split(']')[0];
            var doc = documents.Where(d => d.id == id).FirstOrDefault();
            var clauses = doc.parallelClauses;
            for (int i = 0; i < clauses.GetLength(0); i++)
            {
                parallelizedClauses.Add(new List<string>());
                for (int j = 0; j < clauses.GetLength(1); j++)
                {
                    parallelizedClauses[i].Add(clauses[i, j].clause is null ? "-" : String.Join(' ', clauses[i, j].clause.realizations.Select(r => "<span class=\"token\" id=\"" + id  + "|" + i + "|" + j + "\">" + r.Output() + "</span>")));
                }
            }
            var tokens = doc.parallelTokens;
            if (!(tokens is null))
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    parallelizedTokens.Add("<div>");
                    for (int j = 0; j < tokens[i].Count; j++)
                    {
                        for (int k = 0; k < tokens[i][j].Count; k++)
                        {
                            parallelizedTokens[i] += "{" + tokens[i][j][k].documentID + "|" + tokens[i][j][k].clauseID + "|" + tokens[i][j][k].textID + "|" + tokens[i][j][k].realizationID + " - " + tokens[i][j][k].lexemeTwo + "};";
                        }
                    }
                    parallelizedTokens[i] += "<button id=\"deleteTemporary\">[[Delete]]</button></div>";
                }
            }

        }

        [HttpPost]
        public IActionResult OnPostTag(string sequenceOfParallelTokens)
        {
            var parallels = sequenceOfParallelTokens.Trim().Split("[[Delete]]").Where(parallel => parallel != "").ToList();
            ParallelDocument parallelSubcorpus = null;
            string parallelSubcorpusFilePath = null;
            foreach (var parallel in parallels)
            {
                var sequencesSplit = parallel.Split("};").Where(token => token != "").ToList();
                var sequencesIDs = sequencesSplit.Select(x => x.Trim().Trim('{').Split(" - ")[0]).ToList();
                if (parallelSubcorpus is null)
                {
                    string documentID = sequencesIDs[0].Split('|')[0];
                    var dirParallelCorpus =Path.Combine(_environment.ContentRootPath, "database", "parallelizedDocuments");
                    DirectoryInfo directoryParallelCorpusInfo = new DirectoryInfo(dirParallelCorpus);
                    parallelSubcorpusFilePath = directoryParallelCorpusInfo.GetFiles().Where(f => f.Name.Split('_')[0] == documentID).FirstOrDefault().FullName;
                    using (StreamReader r = new StreamReader(new FileStream(parallelSubcorpusFilePath, FileMode.Open)))
                    {
                        parallelSubcorpus = JsonConvert.DeserializeObject<ParallelDocument>(r.ReadToEnd());
                    }
                    parallelSubcorpus.parallelTokens = new List<ParallelToken>();
                }
                var token2Add = new ParallelToken();
                foreach (var token in sequencesIDs)
                {
                    Console.WriteLine(token);
                    var splitID = token.Split('|').Where(x => x != "").ToList();
                    RealizationGroup currentGroup;
                    if (token2Add.Where(g => g.documentID == splitID[0] && g.textID == splitID[2] && g.clauseID == splitID[1]).ToList().Count == 0)
                    {
                        currentGroup = new RealizationGroup();
                        currentGroup.documentID = splitID[0];
                        currentGroup.textID = splitID[2];
                        currentGroup.clauseID = splitID[1];
                        token2Add.Add(currentGroup);
                    }
                    currentGroup = token2Add.Where(g => g.documentID == splitID[0] && g.textID == splitID[2] && g.clauseID == splitID[1]).FirstOrDefault();

                    var singleToken = parallelSubcorpus.parallelClauses[Convert.ToInt32(splitID[2]), Convert.ToInt32(splitID[1])].clause.realizations.Where(r => r.realizationID == splitID[3]).FirstOrDefault();
                    singleToken.documentID = splitID[0];
                    singleToken.textID = splitID[2];
                    singleToken.clauseID = splitID[1];
                    if (!currentGroup.Contains(singleToken)) currentGroup.Add(singleToken);
                }
                if (!parallelSubcorpus.parallelTokens.Contains(token2Add)) parallelSubcorpus.parallelTokens.Add(token2Add);
            }
            if (!(parallelSubcorpus is null) && !(parallelSubcorpusFilePath is null))
            {
                var parallelSubcorpusInJSON = parallelSubcorpus.Jsonize();
                using (StreamWriter w = new StreamWriter(new FileStream(parallelSubcorpusFilePath, FileMode.Create)))
                {
                    w.Write(parallelSubcorpusInJSON);
                }
            }
            return RedirectToPage();
        }
    }
}
