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
                return MyExtensions.GetManuscripts(Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields"));
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
            foreach (Clause clause in editedDocument.texts.Where(t => t.textID == text_edited).ToList()[0].clauses)
            {
                List<string> forEditing = units.Select(c => c.Value).Where(v => Regex.Replace(v.Split('|')[2], @"(\s.*|\})", "") == clause.clauseID).ToList();
                for (int i = 0; i < forEditing.Count; i++)
                {
                    if (Regex.IsMatch(forEditing[i], @"\{(\d*\|){2}(\d*)(\s.*?\}|\})"))
                    {
                        try
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
                            clause.clauseFields = newFields;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            try
                            {
                                if (clause.clauseFields.Count < 1)
                                {
                                    continue;
                                }
                                else
                                {
                                    clause.clauseFields.Clear();
                                }
                            }
                            catch (NullReferenceException)
                            {
                                continue;
                            }
                        }
                    }
                    else if (Regex.IsMatch(forEditing[i], @"\{(\d*\|){4}(\d*)(\s.*?\}|\})"))
                    {
                        string realizationID = Regex.Replace(forEditing[i].Split('|')[3], @"(\s.*|\})", "");
                        string graphemeID = Regex.Replace(forEditing[i].Split('|')[4], @"(\s.*|\})", "");
                        Grapheme grapheme = clause.realizations.Where(r => r.realizationID == realizationID).Select(r => r.letters).ToList()[0].Where(g => g.graphemeID == graphemeID).ToList()[0];
                        try
                        {
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
                            grapheme.graphemeFields = newFields;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            try
                            {
                                if (grapheme.graphemeFields.Count < 1)
                                {
                                    continue;
                                }
                                else
                                {
                                    grapheme.graphemeFields.Clear();
                                }
                            }
                            catch (NullReferenceException)
                            {
                                continue;
                            }
                        }
                    }
                    else if (Regex.IsMatch(forEditing[i], @"\{(\d*\|){3}(\d*)(\s.*?\}|\})"))
                    {
                        Realization realization = clause.realizations.Where(r => r.realizationID == Regex.Replace(forEditing[i].Split('|')[3], @"(\s.*|\})", "")).ToList()[0];
                        try
                        {
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
                            realization.realizationFields = newFields;
                            realization.realizationFields = realization.realizationFields.Where(d => d.Count != 0).ToList();
                        }
                        catch (IndexOutOfRangeException)
                        {
                            try
                            {
                                if (realization.realizationFields.Count < 1)
                                {
                                    continue;
                                }
                                else
                                {
                                    realization.realizationFields.Clear();
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            string documentInJSON = editedDocument.Jsonize();
            var documentDBFile = Path.Combine(_environment.ContentRootPath, "database", "documents", editedDocument.documentID + "_" + editedDocument.documentName + ".json");
            FileStream fs = new FileStream(documentDBFile, FileMode.Create);
            using (StreamWriter w = new StreamWriter(fs))
            {
                w.Write(documentInJSON);
            }
            return RedirectToPage();
        }


    }
}
