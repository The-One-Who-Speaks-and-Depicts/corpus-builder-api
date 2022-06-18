using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using Newtonsoft.Json;
using CorpusDraftCSharp;
using System.Text.RegularExpressions;

namespace CroatianProject.Pages
{
    public class CollectDictionaryModel : PageModel
    {
        [BindProperty]
        public List<string> documentNames
        {
            get
            {
                try
                {
                    return new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents")).GetFiles().Select(f =>
                    {
                        using (StreamReader r = new StreamReader(new FileStream(f.FullName, FileMode.Open, FileAccess.Read), System.Text.Encoding.UTF8))
                        {
                            return JsonConvert.DeserializeObject<Document>(r.ReadToEnd()).documentName;
                        }
                    })
                    .OrderBy(x => x)
                    .ToList();
                }
                catch
                {
                    return new List<string>();
                }
            }
        }
        [BindProperty]
        public List<KeyValuePair<string, List<string>>> parallelDocumentNames
        {
            get
            {
                try
                {
                    return new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "parallelizedDocuments")).GetFiles().Select(f =>
                    {
                        using (StreamReader r = new StreamReader(new FileStream(f.FullName, FileMode.Open, FileAccess.Read), System.Text.Encoding.UTF8))
                        {
                            var parallelizedDoc = JsonConvert.DeserializeObject<ParallelDocument>(r.ReadToEnd());
                            var parallelizedClausesNames = new List<string>();
                            for (int i = 0; i < parallelizedDoc.parallelClauses.GetLength(1); i++)
                            {
                                parallelizedClausesNames.Add(parallelizedDoc.parallelClauses[0, i].textName + " (" + i.ToString()+ ")");
                            }
                            return new KeyValuePair<string, List<string>>(parallelizedDoc.name, parallelizedClausesNames);
                        }
                    })
                    .OrderBy(x => x.Key)
                    .ToList();
                }
                catch
                {
                    return new List<KeyValuePair<string, List<string>>>();
                }
            }
        }
        [BindProperty]
        public List<string> chosenTexts { get; set; }
        [BindProperty]
        public bool dictsToFiles { get; set; }
        [BindProperty]
        public bool parallelDictsToFiles { get; set; }
        [BindProperty]
        public List<string> convertedTexts { get; set; }

        private IWebHostEnvironment _environment;

        public CollectDictionaryModel (IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public void OnGet()
        {

        }

        public void OnPost()
        {
            var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents")).GetFiles();
            var documents = new List<Document>();
            if (files.Length > 0)
            {
                for (int f = 0; f < files.Length; f++)
                {
                    using (StreamReader r = new StreamReader(new FileStream(files[f].FullName, FileMode.Open, FileAccess.Read)))
                    {
                        documents.Add(JsonConvert.DeserializeObject<Document>(r.ReadToEnd()));
                    }

                }
                documents = documents.Where(x => chosenTexts.Contains(x.documentName)).ToList();
                var taggedDocs = documents.Where(d => d.documentMetaData.Any(f => f.ContainsKey("Tagged") && f["Tagged"].Any(t => t.name != "Not_tagged"))).ToList();
                var untaggedDocs = documents.Where(d => d.documentMetaData.Any(f => !f.ContainsKey("Tagged") || f["Tagged"].Any(t => t.name == "Not_tagged"))).ToList();
                List<string> untaggedLemmata = untaggedDocs
                    .SelectMany(d => d.texts)
                    .SelectMany(t => t.clauses)
                    .SelectMany(c => c.realizations)
                    .Select(r => r.lexemeTwo)
                    .Distinct()
                    .ToList();
                List<string> taggedLemmata = taggedDocs
                    .SelectMany(d => d.texts)
                    .SelectMany(t => t.clauses)
                    .SelectMany(c => c.realizations)
                    .SelectMany(r => r.realizationFields.Where(t => t.ContainsKey("Lemma"))
                    .SelectMany(t => t["Lemma"])
                    .Select(v => v.name))
                    .Distinct()
                    .ToList();
                taggedLemmata.AddRange(untaggedLemmata);
                List<string> lemmata = taggedLemmata.Distinct().ToList();
                var finalDictionary = new List<DictionaryUnit>();
                lemmata.ForEach(lemma => finalDictionary.Add(new DictionaryUnit(lemma, untaggedDocs.SelectMany(d => d.texts).SelectMany(t => t.clauses).SelectMany(c => c.realizations).Where(r => r.lexemeTwo == lemma).ToList().Concat(taggedDocs.SelectMany(d => d.texts).SelectMany(t => t.clauses).SelectMany(c => c.realizations).Where(r => r.realizationFields.Where(f => f.ContainsKey("Lemma")).SelectMany(kvp => kvp["Lemma"]).Any(v => v.name == lemma)).ToList()).ToList())));
                finalDictionary = finalDictionary.OrderBy(unit => unit.lemma).ToList();
                if (dictsToFiles == true)
                {
                    string path = Path.Combine(_environment.ContentRootPath, "database");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    path = Path.Combine(path, "dictionary");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    using (StreamWriter w = new StreamWriter(new FileStream(Path.Combine(path, string.Join("_", chosenTexts) + "_dict.json"),FileMode.CreateNew, FileAccess.Write)))
                    {
                        for (int u = 0; u < finalDictionary.Count; u++)
                        {
                            w.Write(JsonConvert.SerializeObject(finalDictionary[u], Formatting.Indented));
                        }
                    }
                }
                finalDictionary.ForEach(x => convertedTexts.Add("<b>" + x.lemma + "(" + x.realizations.Count + ")</b><br /><br/><br/>" + ((x.realizations.Count > 0) ? string.Join("<br /><br/>", x.realizations.Select(r => "<i>" + r.lexemeTwo + "</i>(<i>" + documents.Where(d => d.documentID == r.documentID).SelectMany(d => d.texts).Where(t => t.textID == r.textID).SelectMany(t => t.clauses).Where(c => c.clauseID == r.clauseID).First().clauseText + "</i>)" + ((r.realizationFields != null) ? ":<br/>" + string.Join("<br />", r.realizationFields.SelectMany(t => t).Where(a => a.Key != "Lemma").Select(a => a.Key + ": " + string.Join("", a.Value.SelectMany(v => v.name).ToList())).Distinct().ToList()) : "")).ToList()) : "") + "<br/><br/><br/><br/>"));
            }
        }

        public void OnPostParallel()
        {
            var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "parallelizedDocuments")).GetFiles();
            var documents = new List<ParallelDocument>();
            if (files.Length > 0)
            {
                for (int f = 0; f < files.Length; f++)
                {
                    using (StreamReader r = new StreamReader(new FileStream(files[f].FullName, FileMode.Open, FileAccess.Read)))
                    {
                        documents.Add(JsonConvert.DeserializeObject<ParallelDocument>(r.ReadToEnd()));
                    }

                }
                List<string> lemmata = new List<string>();
                chosenTexts.ForEach(x =>
                {
                    var docName = x.Split(": ")[0];
                    var textID = x.Split(": ")[1].Split(" (")[1].Split(')')[0];
                    var currentLemmata = documents
                    .Where(d => d.name == docName)
                    .SelectMany(d => d.parallelTokens)
                    .SelectMany(t => t.ToList())
                    .SelectMany(rg => rg.ToList())
                    .Where(r => r.clauseID == textID)
                    .Select(r => r.realizationFields is null ? r.lexemeTwo : r.realizationFields.Any(f => f.ContainsKey("Lemma")) ? r.realizationFields.Where(t => t.ContainsKey("Lemma")).SelectMany(t => t["Lemma"]).Select(v => v.name).FirstOrDefault() : r.lexemeTwo)
                    .Distinct()
                    .ToList();
                    currentLemmata.ForEach(l => lemmata.Add(l));
                });
                lemmata = lemmata.Distinct().ToList();
                var finalDictionary = new List<ParallelDictionaryUnit>();
                lemmata.ForEach(lemma => finalDictionary.Add(new ParallelDictionaryUnit(lemma, documents
                .SelectMany(d => d.parallelTokens)
                .Where(t => t.Any(rg => rg.Any(r => r.lexemeTwo == lemma || (r.realizationFields != null ? (r.realizationFields.Any(f => f.ContainsKey("Lemma")) ? r.realizationFields.Where(f => f.ContainsKey("Lemma")).SelectMany(kvp => kvp["Lemma"]).Any(v => v.name == lemma) : false) : false))))
                .ToList())));
                finalDictionary = finalDictionary.OrderBy(unit => unit.lemma).ToList();
                if (parallelDictsToFiles == true)
                {
                    string path = Path.Combine(_environment.ContentRootPath, "database");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    path = Path.Combine(path, "parallelDictionary");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    using (StreamWriter w = new StreamWriter(new FileStream(Path.Combine(path, Regex.Replace(string.Join("_", chosenTexts), @"[*""><:/\\|?\.,]", "") + "_parallelDict.json"),FileMode.CreateNew, FileAccess.Write)))
                    {
                        for (int u = 0; u < finalDictionary.Count; u++)
                        {
                            w.Write(JsonConvert.SerializeObject(finalDictionary[u], Formatting.Indented));
                        }
                    }
                }
                foreach (var unit in finalDictionary)
                {
                    string output = "<b>" + unit.lemma + "</b>";
                    output += " (" + unit.realizations.Count.ToString() + ")<br />";
                    output += "<ul>";
                    foreach (var token in unit.realizations)
                    {
                        RealizationGroup coreGroup = null;
                        Realization coreRealization = null;
                        foreach (var rg in token)
                        {
                            foreach (var realization in rg)
                            {
                                if (realization.realizationFields != null)
                                {
                                    if (realization.realizationFields.Any(f => f.ContainsKey("Lemma")))
                                    {
                                        if (realization.realizationFields.Where(f => f.ContainsKey("Lemma")).SelectMany(kvp => kvp["Lemma"]).Any(v => v.name == unit.lemma))
                                        {
                                            coreGroup = rg;
                                            coreRealization = realization;
                                            break;
                                        }
                                    }
                                }
                                if (realization.lexemeTwo == unit.lemma)
                                {
                                    coreGroup = rg;
                                    coreRealization = realization;
                                    break;
                                }

                            }
                        }
                        output += "<li>";
                        var currentDocument = documents.Where(d => d.id == coreRealization.documentID).Single();
                        output += coreRealization.lexemeTwo + " (" + String.Join(' ', coreGroup.Select(r => r.lexemeTwo)) + ", " + currentDocument.name + " - " + currentDocument.parallelClauses[Convert.ToInt32(coreRealization.textID), Convert.ToInt32(coreRealization.clauseID)].textName +  "); <span class=\"clause\" id=\"clauseExtractionButton\" clause=\"clause: " + currentDocument.parallelClauses[Convert.ToInt32(coreRealization.textID), Convert.ToInt32(coreRealization.clauseID)].clause.clauseText + "\">see text segment</span><br />";
                        output += ((coreRealization.realizationFields != null && coreRealization.realizationFields.Count > 0) ? String.Join(" - ", coreRealization.realizationFields.SelectMany(t => t).Where(a => a.Key != "Lemma").Select(a => String.Join("", a.Value.SelectMany(v => v.name).ToList())).Distinct().ToList()) + "<br />" : "");
                        output += coreRealization.letters.Any(l => !(l.graphemeFields is null) && l.graphemeFields.Count > 0) ? ("<ul class=\"graphemeFeatures\">" + String.Join("",coreRealization.letters.Select(l => ((l.graphemeFields != null && l.graphemeFields.Count > 0) ? "<li>" + l.grapheme + ": " + String.Join(" - ", l.graphemeFields.SelectMany(t => t).Select(a => a.Key + "(" + String.Join("", a.Value.SelectMany(v => v.name).ToList()) + ")").Distinct().ToList()) + "<br />" : "")))  + "</ul>") : "";
                        output += "<ul class=\"parallels\"><li>" + String.Join(";<br/><li>", token.GetParallels(coreGroup).Select(rg => String.Join(' ', rg.Select(r => r.lexemeTwo).ToList()) + "(" + currentDocument.name + " - " + currentDocument.parallelClauses[Convert.ToInt32(rg[0].textID), Convert.ToInt32(rg[0].clauseID)].textName + ")"+ "; <span class=\"clause\" id=\"clauseExtractionButton\" clause=\"text segment:\r" + String.Join('\r', rg.Select(r => r.clauseID).Distinct().ToList().OrderBy(id => Convert.ToInt32(id)).Select(id => currentDocument.parallelClauses[Convert.ToInt32(rg.textID), Convert.ToInt32(id)].clause.clauseText)) + "\">see text segment</span>")) + "</ul></li>";
                    }
                    output += "</ul><br /><br />";
                    convertedTexts.Add(output);

                }
            }
        }
    }
}
