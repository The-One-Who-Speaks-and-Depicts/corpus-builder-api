using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuscriptsProcessor.Units;
using ManuscriptsProcessor.Values;
using Newtonsoft.Json;

namespace CroatianProject.Pages.Admin
{
    public class CorrectionModel : PageModel
    {
        public string correction {get; set;}
        private IWebHostEnvironment _environment;
        public CorrectionModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public void OnPost(string correction)
        {
            var directory = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents"));
            var docs = directory.GetFiles();
            var changedDocs = new Dictionary<string, Manuscript>();
            foreach (var doc in docs)
            {
                using (StreamReader r = new StreamReader(doc.FullName))
                {
                    var changed = false;
                    var deserialized = JsonConvert.DeserializeObject<Manuscript>(r.ReadToEnd());
                    foreach (var section in deserialized.subunits)
                    {
                        foreach (var segment in section.subunits)
                        {
                            foreach (var clause in segment.subunits)
                            {
                                foreach (var realization in clause.subunits)
                                {
                                    if (realization.tagging != null)
                                    {
                                        foreach (var tagging in realization.tagging)
                                        {
                                            foreach (KeyValuePair<string, List<Value>> kv in tagging)
                                            {
                                                if (kv.Key == correction)
                                                {
                                                    changed = true;
                                                    string edited = kv.Value[0].name;
                                                    realization.lexemeView = edited;
                                                    realization.text = edited;
                                                    realization.subunits = new List<Grapheme>();
                                                    for (int i = 0; i < edited.Length; i++)
                                                    {
                                                        realization.subunits.Add(new Grapheme(realization, i.ToString(), edited[i].ToString()));
                                                    }
                                                }
                                            }
                                        }
                                        realization.tagging = realization.tagging.Where(t => !t.ContainsKey(correction)).ToList();
                                    }
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
