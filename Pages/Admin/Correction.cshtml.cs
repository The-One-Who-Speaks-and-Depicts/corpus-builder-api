using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using CorpusDraftCSharp;
using System.IO;
using Newtonsoft.Json;

namespace CroatianProject.Pages.Admin
{
    public class CorrectionModel : PageModel
    {
        public string correction {get; set;}
        private IHostingEnvironment _environment;
        public CorrectionModel(IHostingEnvironment environment)
        {
            _environment = environment;
        }
        public void OnPost(string correction)
        {
            var directory = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents"));
            var docs = directory.GetFiles();
            var changedDocs = new Dictionary<string, Document>();
            foreach (var doc in docs)
            {                
                using (StreamReader r = new StreamReader(doc.FullName))
                {
                    var changed = false;
                    var deserialized = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                    foreach (var text in deserialized.texts)
                    {
                        foreach (var clause in text.clauses)
                        {
                            foreach (var realization in clause.realizations)
                            {                                
                                if (realization.realizationFields != null)
                                {
                                    foreach (var tagging in realization.realizationFields)
                                    {
                                        foreach (KeyValuePair<string, List<Value>> kv in tagging)
                                        {
                                            if (kv.Key == correction)
                                            {
                                                changed = true;
                                                string edited = kv.Value[0].name;
                                                realization.lexemeOne = edited;
                                                realization.lexemeTwo = edited;
                                                realization.letters = new List<Grapheme>();
                                                for (int i = 0; i < edited.Length; i++)
                                                {
                                                    realization.letters.Add(new Grapheme(realization.documentID, deserialized.filePath, realization.textID, realization.clauseID, realization.realizationID, i.ToString(), edited[i].ToString()));
                                                }                                                
                                            }
                                        }
                                    }
                                    realization.realizationFields = realization.realizationFields.Where(t => !t.ContainsKey(correction)).ToList();                                    
                                }
                            }
                        }
                    }
                    if (changed)
                    {
                        changedDocs[doc.FullName] = deserialized;
                    }
                }                
            }
            foreach (var deserialized in changedDocs)
            {
                string documentInJSON = deserialized.Value.Jsonize();
                var documentDBFile = Path.Combine(deserialized.Key);
                Console.WriteLine(documentDBFile);
                FileStream fs = new FileStream(documentDBFile, FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(documentInJSON);
                }  
            }             
        }
    }
}
