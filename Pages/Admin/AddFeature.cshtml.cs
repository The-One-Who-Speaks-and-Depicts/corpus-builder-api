using System.Text.RegularExpressions;
using ManuscriptsProcessor.Units;
using ManuscriptsProcessor;
using ManuscriptsProcessor.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace CroatianProject.Pages.Admin
{
    public class AddFeatureModel : PageModel
    {
        [BindProperty]
        public List<string> manuscriptList
        {
            get
            {
                return MyExtensions.GetManuscriptsWithTexts(Path.Combine(_environment.ContentRootPath, "database", "manuscripts"));
            }
        }
        [BindProperty]
        public string names { get; set; }
        public string sectionByWords { get; set; }
        [BindProperty]
        public List<string> fieldsList
        {
            get
            {
                return MyExtensions.GetFields(Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields"));
            }
        }
        [BindProperty]
        public string currentWordId { get; set; }
        [BindProperty]
        public string currentField { get; set; }
        [BindProperty]
        public string currentFieldValue { get; set; }
        [BindProperty]
        public string isFieldMultiple { get; set; }
        [BindProperty]
        public string currentSection { get; set; }

        private IWebHostEnvironment _environment;
        public AddFeatureModel(IWebHostEnvironment environment)
        {
            _environment = environment;

        }
        public void OnGet(string names)
        {
            if (names != null)
            {
                var manuscriptId = names.Split("||")[0].Split('[')[1].Split(']')[0];
                var sectionId = names.Split("||")[1].Split('[')[1].Split(']')[0];
                var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "manuscripts")).GetFiles();
                if (files.Length < 1) return;
                foreach (var file in files)
                {
                    using (StreamReader r = new StreamReader(file.FullName))
                    {
                        var analyzedManuscript = JsonConvert.DeserializeObject<Manuscript>(r.ReadToEnd());
                        if (analyzedManuscript.Id == manuscriptId)
                        {
                            foreach (var s in analyzedManuscript.subunits)
                            {
                                if (s.Id.Split('|')[1] == sectionId)
                                {
                                    sectionByWords = s.Output();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            MatchCollection units = Regex.Matches(currentSection, @"\{(\d*\|){2}.*?\}");
            string script_edited = units[0].Value.Split('|')[0].Replace("{", "");
            string text_edited = units[0].Value.Split('|')[1];
            var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "manuscripts")).GetFiles();
            var editedManuscript = new Manuscript();
            if (files.Length < 1) return RedirectToPage();
            foreach (var file in files)
            {
                using (StreamReader r = new StreamReader(file.FullName))
                {
                    var analyzedManuscript = JsonConvert.DeserializeObject<Manuscript>(r.ReadToEnd());
                    if (analyzedManuscript.Id == script_edited)
                    {
                        editedManuscript = analyzedManuscript;
                    }
                }
            }
            foreach (var clause in editedManuscript.subunits.Where(t => t.Id.Split('|')[1] == text_edited).Single().subunits.SelectMany(sgm => sgm.subunits).ToList())
            {
                List<string> forEditing = units.Select(c => c.Value).Where(v => (Regex.Replace(v.Split('|')[2], @"(\s.*|\})", "") + "|" + Regex.Replace(v.Split('|')[2], @"(\s.*|\})", ""))  == (clause.Id.Split('|')[2] + "|" + clause.Id.Split('|')[3])).ToList();
                for (int i = 0; i < forEditing.Count; i++)
                {
                    if (Regex.IsMatch(forEditing[i], @"\{(\d*\|){3}(\d*)(\s.*?\}|\})"))
                    {
                        List<string> addedFields = forEditing[i].Split(" => ")[1].Replace("{", "").Split(";<br />").ToList();
                        List<Dictionary<string, List<Value>>> newFields = new List<Dictionary<string, List<Value>>>();
                        Dictionary<string, List<Value>> addedField = new Dictionary<string, List<Value>>();
                        for (int j = 0; j < addedFields.Count; j++)
                        {
                            if (addedFields[j] != "")
                            {
                                List<string> currentFields = addedFields[j].Replace(" ;}", "").Split(";").ToList();
                                for (int f = 0; f < currentFields.Count; f++)
                                {
                                    if (currentFields[f] != "}")
                                    {
                                        string field = currentFields[f].Split(':')[0];
                                        string values = currentFields[f].Split(':')[1];
                                        List<Value> addedValues = new List<Value>();
                                        if (values.Contains(','))
                                        {
                                            List<string> splitValues = values.Split(",").ToList();
                                            for (int k = 0; k < splitValues.Count; k++)
                                            {
                                                addedValues.Add(new Value(splitValues[k].Trim()));
                                            }
                                        }
                                        else
                                        {
                                            addedValues.Add(new Value(values.Trim()));
                                        }
                                        addedField[field] = addedValues;
                                    }
                                }
                            }
                        }
                        newFields.Add(addedField);
                        clause.tagging = newFields;
                    }
                    else if (Regex.IsMatch(forEditing[i], @"\{(\d*\|){5}(\d*)(\s.*?\}|\})"))
                    {
                        string realizationID = Regex.Replace(forEditing[i].Split('|')[4], @"(\s.*|\})", "");
                        string graphemeID = Regex.Replace(forEditing[i].Split('|')[5], @"(\s.*|\})", "");
                        var grapheme = clause.subunits.Where(r => r.Id.Split('|')[4] == realizationID).Select(r => r.subunits).First().Where(g => g.Id.Split('|')[5] == graphemeID).First();
                        List<string> addedFields = forEditing[i].Split(" => ")[1].Replace("{", "").Split(";<br />").ToList();
                        List<Dictionary<string, List<Value>>> newFields = new List<Dictionary<string, List<Value>>>();
                        Dictionary<string, List<Value>> addedField = new Dictionary<string, List<Value>>();
                        for (int j = 0; j < addedFields.Count; j++)
                        {
                            List<string> currentFields = addedFields[j].Replace(" ;}", "").Split(";").ToList();
                            for (int f = 0; f < currentFields.Count; f++)
                            {
                                if (currentFields[f] != "}")
                                {
                                    string field = currentFields[f].Split(':')[0];
                                    string values = currentFields[f].Split(':')[1];
                                    List<Value> addedValues = new List<Value>();
                                    if (values.Contains(','))
                                    {
                                        List<string> splitValues = values.Split(",").ToList();
                                        for (int k = 0; k < splitValues.Count; k++)
                                        {
                                            addedValues.Add(new Value(splitValues[k].Trim()));
                                        }
                                    }
                                    else
                                    {
                                        addedValues.Add(new Value(values.Trim()));
                                    }
                                    addedField[field] = addedValues;
                                }
                            }
                        }
                        newFields.Add(addedField);
                        grapheme.tagging = newFields;
                    }
                    else if (Regex.IsMatch(forEditing[i], @"\{(\d*\|){4}(\d*)(\s.*?\}|\})"))
                    {
                        var token = clause.subunits.Where(r => r.Id.Split('|')[4] == Regex.Replace(forEditing[i].Split('|')[4], @"(\s.*|\})", "")).First();
                        List<string> addedFields = forEditing[i].Split(" => ")[1].Split("***").ToList();
                        List<Dictionary<string, List<Value>>> newFields = new List<Dictionary<string, List<Value>>>();
                        for (int j = 0; j < addedFields.Count; j++)
                        {
                            Dictionary<string, List<Value>> addedTagging = new Dictionary<string, List<Value>>();
                            if (addedFields[j] != "")
                            {
                                List<string> tagging = addedFields[j].Split(";<br />").ToList();
                                for (int f = 0; f < tagging.Count; f++)
                                {
                                    if (tagging[f] != "}")
                                    {
                                        List<string> fieldsToAdd = tagging[f].Split(" ;").ToList();
                                        for (int n = 0; n < fieldsToAdd.Count; n++)
                                        {
                                            if (fieldsToAdd[n] != "" && fieldsToAdd[n] != ";")
                                            {
                                                List<string> splitFields = fieldsToAdd[n].Split(';').ToList();
                                                for (int s = 0; s < splitFields.Count; s++)
                                                {
                                                    if (splitFields[s] != ";" && splitFields[s] != "")
                                                    {
                                                        string field = splitFields[s].Split(':')[0];
                                                        string values = splitFields[s].Split(':')[1].Replace(";", "");
                                                        List<Value> addedValues = new List<Value>();
                                                        if (values.Contains(','))
                                                        {
                                                            List<string> splitValues = values.Split(",").ToList();
                                                            for (int k = 0; k < splitValues.Count; k++)
                                                            {
                                                                addedValues.Add(new Value(splitValues[k].Trim()));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            addedValues.Add(new Value(values.Trim()));
                                                        }
                                                        addedTagging[field] = addedValues;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            newFields.Add(addedTagging);
                        }
                        token.tagging = newFields;
                        token.tagging = token.tagging.Where(d => d.Count != 0).ToList();
                    }
                }
            }
            string documentInJSON = editedManuscript.Jsonize();
            var documentDBFile = Path.Combine(_environment.ContentRootPath, "database", "manuscripts", editedManuscript.Id + "_" + editedManuscript.text + ".json");
            FileStream fs = new FileStream(documentDBFile, FileMode.Create);
            using (StreamWriter w = new StreamWriter(fs))
            {
                w.Write(documentInJSON);
            }
            return RedirectToPage();
        }


    }
}
