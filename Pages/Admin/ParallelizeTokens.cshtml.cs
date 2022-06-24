using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuscriptsProcessor.Units;
using ManuscriptsProcessor;
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
        public List<string> manuscripts
        {
            get
            {
                return MyExtensions.GetParallelManuscripts(Path.Combine(_environment.ContentRootPath, "database", "parallelizedManuscripts"));
            }
        }

        [BindProperty]
        public string manuscriptPicked { get; set; }

        [BindProperty]
        public string sequenceOfParallelTokens {get; set;}

        private IWebHostEnvironment _environment;
        public ParallelizeTokensModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        public void OnPost(string manuscriptPicked)
        {
            var id = manuscriptPicked.Split('[')[1].Split(']')[0];
            var deserializedManuscripts = new List<ParallelManuscript>();
            var scriptDirectory = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "parallelizedManuscripts"));
            var jsonedScripts = scriptDirectory.GetFiles();
            if (jsonedScripts.Length < 1) return;
            foreach (var script in jsonedScripts)
            {
                using (StreamReader r = new StreamReader(script.FullName))
                {
                    deserializedManuscripts.Add(JsonConvert.DeserializeObject<ParallelManuscript>(r.ReadToEnd()));
                }
            }
            var manuscript = deserializedManuscripts.Where(d => d.Id == id).FirstOrDefault();
            var clauses = manuscript.parallelClauses;
            for (int i = 0; i < clauses.GetLength(1); i++)
            {
                parallelizedClauses.Add(new List<string>());
                for (int j = 0; j < clauses.GetLength(0); j++)
                {
                    if (clauses[j, i] is null) continue;
                    parallelizedClauses[i].Add(clauses[j, i].clause is null ? "-" : String.Join(' ', clauses[j, i].clause.subunits.Select(t => "<span class=\"token\" id=\"" + id  + "|" + i + "|" + j + "|" + t.Id.Split('|')[4] + "\">" + t.Output() + "</span>")));
                }
            }
            var tokens = manuscript.parallelTokens;
            if (!(tokens is null))
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    parallelizedTokens.Add("<div>");
                    for (int j = 0; j < tokens[i].subunits.Count; j++)
                    {
                        for (int k = 0; k < tokens[i].subunits[j].subunits.Count; k++)
                        {
                            parallelizedTokens[i] += "{" + tokens[i].subunits[j].subunits[k].Id + " - " + tokens[i].subunits[j].subunits[k].text + "};";
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
            ParallelManuscript? parallelSubcorpus = null;
            string? parallelSubcorpusFilePath = null;
            foreach (var parallel in parallels)
            {
                var sequencesSplit = parallel.Split("};").Where(token => token != "").ToList();
                var sequencesIDs = sequencesSplit.Select(x => x.Trim().Trim('{').Split(" - ")[0]).ToList();
                if (parallelSubcorpus is null)
                {
                    string scriptID = sequencesIDs[0].Split('|')[0];
                    var dirParallelCorpus =Path.Combine(_environment.ContentRootPath, "database", "parallelizedDocuments");
                    DirectoryInfo directoryParallelCorpusInfo = new DirectoryInfo(dirParallelCorpus);
                    parallelSubcorpusFilePath = directoryParallelCorpusInfo.GetFiles().Where(f => f.Name.Split('_')[0] == scriptID).FirstOrDefault().FullName;
                    using (StreamReader r = new StreamReader(new FileStream(parallelSubcorpusFilePath, FileMode.Open)))
                    {
                        parallelSubcorpus = JsonConvert.DeserializeObject<ParallelManuscript>(r.ReadToEnd());
                    }
                    parallelSubcorpus.parallelTokens = new List<ParallelToken>();
                }
                var token2Add = new ParallelToken();
                foreach (var token in sequencesIDs)
                {
                    Console.WriteLine(token);
                    var splitID = token.Split('|').Where(x => x != "").ToList();
                    TokenGroup currentGroup;
                    if (token2Add.subunits.Where(g => g.Id.Split('|')[0] == splitID[0] && g.Id.Split('|')[1] == splitID[1] && g.Id.Split('|')[2] == splitID[2]).ToList().Count == 0)
                    {
                        currentGroup = new TokenGroup();
                        currentGroup.Id = String.Join('|', new string[] { splitID[0], splitID[1], splitID[2] });
                    }
                    currentGroup = token2Add.subunits.Where(g => g.Id.Split('|')[0] == splitID[0] && g.Id.Split('|')[1] == splitID[1] && g.Id.Split('|')[2] == splitID[2]).FirstOrDefault();

                    var singleToken = parallelSubcorpus.parallelClauses[Convert.ToInt32(splitID[1]), Convert.ToInt32(splitID[2])].clause.subunits.Where(r => r.Id == splitID[4]).FirstOrDefault();
                    singleToken.Id = String.Join('|', new string[] { splitID[0], splitID[1], singleToken.Id.Split('|')[2], splitID[2],  singleToken.Id.Split('|')[4] });
                    if (!currentGroup.subunits.Contains(singleToken)) currentGroup.subunits.Add(singleToken);
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
