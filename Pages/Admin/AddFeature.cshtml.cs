using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CorpusDraftCSharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Xml;

namespace CroatianProject.Pages.Admin
{
    public class AddFeatureModel : PageModel
    {
        [BindProperty]
        public List<string> docList { get; set; }
        [BindProperty]
        public string docName { get; set; }
        [BindProperty]
        public string textName { get; set; }
        public string textByWords { get; set; }
        [BindProperty]
        public List<string> fieldsList { get; set; } = new List<string>();
        [BindProperty]
        public string currentWordId { get; set; }
        [BindProperty]
        public string currentField { get; set; }
        [BindProperty]
        public string currentFieldValue { get; set; }
        [BindProperty]
        public string isFieldMultiple { get; set; }
        [BindProperty]
        public string currentText { get; set; }


    public List<string> getFields()
        {
            List<string> existingFields = new List<string>();
            try
            {
                var directory = Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields");
                DirectoryInfo fieldsDirectory = new DirectoryInfo(directory);
                var fields = fieldsDirectory.GetFiles();
                for (int i = 0; i < fields.Length; i++)
                {
                    if (i < fields.Length - 1)
                    {
                        existingFields.Add(fields[i].Name + "|");
                    }
                    else
                    {
                        existingFields.Add(fields[i].Name);
                    }
                }
            }
            catch
            {

            }
            return existingFields;
        }
        public List<string> getDocs()
        {
            List<string> existingTexts = new List<string>();
            try
            {
                var directory = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents"));
                var docs = directory.GetFiles();
                foreach (var doc in docs)
                {
                    string document = "";
                    using (StreamReader r = new StreamReader(doc.FullName))
                    {
                        var deserialized = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                        document += deserialized.documentID + "_" + deserialized.documentName + ":";
                        for (int i = 0; i < deserialized.texts.Count; i++)
                        {
                            if (i < (deserialized.texts.Count - 1))
                            {
                               document += deserialized.texts[i].textID + "_" + deserialized.texts[i].textName + "||";
                            }
                            else
                            {
                                document += deserialized.texts[i].textID + "_" + deserialized.texts[i].textName;
                            }
                        }
                    }
                    existingTexts.Add(document + "\n");
                }
            }
            catch
            {

            }
            return existingTexts;
        }

        public DirectoryInfo SearchForText(string textName, string directory)
        {
            DirectoryInfo dirTexts = new DirectoryInfo(directory);
            var searchedDirectory = dirTexts.GetDirectories().Where((dir) => dir.Name == textName).First();
            return searchedDirectory;
        }

        private IWebHostEnvironment _environment;
        public AddFeatureModel(IWebHostEnvironment environment)
        {
            _environment = environment;
            try
            {
                docList = getDocs();
                fieldsList = getFields();
            }
            catch
            {
                Redirect("./Error");
            }
        }
        public void OnGet(string docName, string textName)
        {
            try
            {
                docName = docName.Split('_', 2)[0];
                textName = textName.Split('_', 2)[0];
                var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents")).GetFiles();
                Text text = new Text();
                foreach (var file in files)
                {
                    using (StreamReader r = new StreamReader(file.FullName))
                    {
                        Document analyzedDocument = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                        if (analyzedDocument.documentID == docName)
                        {
                            foreach (var t in analyzedDocument.texts)
                            {
                                if (t.textID == textName)
                                {
                                    text = t;
                                    break;
                                }
                            }
                        }
                    }
                }
                textByWords = text.Output();
            }
            catch
            {

            }
            docList = getDocs();
            fieldsList = getFields();
        }

        public IActionResult OnPost()
        {
            try
            {
                MatchCollection units = Regex.Matches(currentText, @"\{(\d*\|){2}.*?\}");
                string document_edited = units[0].Value.Split('|')[0].Replace("{", "");
                string text_edited = units[0].Value.Split('|')[1];
                var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents")).GetFiles();
                Document editedDocument = new Document();
                foreach (var file in files)
                {
                    using (StreamReader r = new StreamReader(file.FullName))
                    {
                        Document analyzedDocument = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                        if (analyzedDocument.documentID == document_edited)
                        {
                            editedDocument = analyzedDocument;
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
            }
            catch
            {

            }
            docList = getDocs();
            fieldsList = getFields();
            return RedirectToPage();
        }


    }
}
