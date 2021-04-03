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
                               document += deserialized.texts[i].textID + "_" + deserialized.texts[i].textName + "|";
                            }
                            else
                            {
                                document += deserialized.texts[i].textID + "_" + deserialized.texts[i].textName;
                            }
                        }
                    }
                    existingTexts.Add(document);
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

        private IHostingEnvironment _environment;
        public AddFeatureModel(IHostingEnvironment environment)
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

        public void OnPost()
        {
            MatchCollection units = Regex.Matches(currentText, @"\{(\d*\|){2}.*?\}");
            string document_edited = units[0].Value.Split('|')[0].Replace("{", "");
            string text_edited = units[0].Value.Split('|')[1];
            var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents")).GetFiles();
            Text text = new Text();
            foreach (var file in files)
            {
                using (StreamReader r = new StreamReader(file.FullName))
                {
                    Document analyzedDocument = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                    if (analyzedDocument.documentID == document_edited)
                    {
                        foreach (var t in analyzedDocument.texts)
                        {
                            if (t.textID == text_edited)
                            {
                                text = t;
                                break;
                            }
                        }
                    }
                }
            }
            foreach (Clause clause in text.clauses)
            {
                List<string> forEditing = units.Select(c => c.Value).Where(v => v.Split('|')[2] == clause.clauseID).ToList();
                for (int i  = 0; i < forEditing.Count; i++)
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
                                    string field = addedFields[j].Split(':')[0];
                                    string values = addedFields[j].Split(':')[1];
                                    List<Value> addedValues = new List<Value>();
                                    if (values.Contains(','))
                                    {
                                        List<string> splitValues = values.Split(" ,").ToList();
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
                            newFields.Add(addedField);
                            clause.clauseFields = newFields;
                            
                        }
                        catch (IndexOutOfRangeException)
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
                    }
                    else if (Regex.IsMatch(forEditing[i], @"\{(\d*\|){4}(\d*)(\s.*?\}|\})"))
                    {
                        string realizationID = forEditing[i].Split('|')[3];
                        string graphemeID = forEditing[i].Split('|')[4];
                        Grapheme grapheme = clause.realizations.Where(r => r.realizationID == realizationID).Select(r => r.letters).ToList()[0].Where(g => g.graphemeID == graphemeID).ToList()[0];
                        try
                        {
                            List<string> addedFields = forEditing[i].Split(" => ")[1].Replace("{", "").Split(";<br />").ToList();
                            List<Dictionary<string, List<Value>>> newFields = new List<Dictionary<string, List<Value>>>();
                            Dictionary<string, List<Value>> addedField = new Dictionary<string, List<Value>>();
                            for (int j = 0; j < addedFields.Count; j++)
                            {
                                if (addedFields[j] != "")
                                {
                                    string field = addedFields[j].Split(':')[0];
                                    string values = addedFields[j].Split(':')[1];
                                    List<Value> addedValues = new List<Value>();
                                    if (values.Contains(','))
                                    {
                                        List<string> splitValues = values.Split(" ,").ToList();
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
                            newFields.Add(addedField);
                            grapheme.graphemeFields = newFields;

                        }
                        catch (IndexOutOfRangeException)
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
                    }
                }
            }
            docList = getDocs();
            fieldsList = getFields();
        }


    }
}
