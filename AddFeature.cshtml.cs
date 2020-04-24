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

namespace CroatianProject.Pages.Admin
{
    public class AddFeatureModel : PageModel
    {
        [BindProperty]
        public List<string> textList { get; set; }
        [BindProperty]
        public string textName { get; set; }
        public List<string> textByWords = new List<string>();
        [BindProperty]
        public List<string> fieldsList { get; set; } = new List<string>();
        [BindProperty]
        public string currentWordId { get; set; }
        [BindProperty]
        public string currentField { get; set; }
        [BindProperty]
        public string currentFieldValue { get; set; }

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
        public List<string> getTexts()
        {
            var directory = Path.Combine(_environment.ContentRootPath, "database", "texts");
            DirectoryInfo textsDirectory = new DirectoryInfo(directory);
            var texts = textsDirectory.GetDirectories();
            List<string> existingTexts = new List<string>();
            existingTexts.Add("Any");
            foreach (var text in texts)
            {
                existingTexts.Add(text.Name);
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
                textList = getTexts();
                fieldsList = getFields();
            }
            catch
            {
                Redirect("./Error");
            }
        }
        public void OnPostShow()
        {
            var directory = Path.Combine(_environment.ContentRootPath, "database", "texts");
            List<DirectoryInfo> searchedTexts = new List<DirectoryInfo>();
            if (textName != "Any")
            {
                searchedTexts.Add(SearchForText(textName, directory));            
            List<Realization> acquiredForms = new List<Realization>();
            foreach (var text in searchedTexts)
            {
                DirectoryInfo dirWords = new DirectoryInfo(Path.Combine(directory, text.Name, "paragraphs"));
                var dirResults = dirWords.GetDirectories();
                foreach (var wordDirectory in dirResults)
                {
                    var words = wordDirectory.GetFiles();
                    string s;
                    foreach (var word in words)
                    {

                        using (var f = new StreamReader(word.FullName))
                        {
                            while ((s = f.ReadLine()) != null)
                            {
                                acquiredForms.Add(JsonConvert.DeserializeObject<Realization>(s));
                            }
                        }
                    }
                }
            }
            foreach (var foundWord in acquiredForms)
            {
                    try
                    {
                        string fieldsOfWord = "";
                        foreach (var field in foundWord.realizationFields)
                        {
                            fieldsOfWord += field;
                            fieldsOfWord += ":";
                            foreach (var fieldValue in field.Value)
                            {
                                fieldsOfWord += fieldValue;
                                fieldsOfWord += "";
                            }
                            fieldsOfWord += "\n";
                        }
                        textByWords.Add("<span class=\"word\" id=\"" + foundWord.documentID + "|" + foundWord.clauseID + "|" + foundWord.realizationID + "\"> " + foundWord.lexeme + fieldsOfWord + "</span>");
                    }
                    catch
                    {
                        textByWords.Add("<span class=\"word\" id=\"" + foundWord.documentID + "|" + foundWord.clauseID + "|" + foundWord.realizationID + "\"> " + foundWord.lexeme + "</span>");
                    }
                
            }
            }
            textList = getTexts();
            fieldsList = getFields();
        }
        
        public void OnPostChange()
        {
            List<string> ids = currentWordId.Split('|').ToList<string>();
            var directory = Path.Combine(_environment.ContentRootPath, "database", "texts");
            DirectoryInfo textsDirectory = new DirectoryInfo(directory);
            var texts = textsDirectory.GetDirectories();
            foreach (var text in texts)
            {
                var textFiles = text.GetFiles();
                foreach (var file in textFiles)
                {
                    string s;
                    using (var f = new StreamReader(file.FullName))
                    {
                        while ((s = f.ReadLine()) != null)
                        {
                            var currentText = JsonConvert.DeserializeObject<Text>(s);
                            if (currentText.documentID == ids[0])
                            {
                                var paragraphsDirectory = Path.Combine(text.FullName, "paragraphs");
                                var paragraphDirectories = new DirectoryInfo(paragraphsDirectory).GetDirectories(); 
                                foreach (var paragraph in paragraphDirectories)
                                {
                                    var paragraphNames = paragraph.Name.Split(']');
                                    paragraphNames = paragraphNames[0].Split('[');
                                    var paragraphName = paragraphNames[1];
                                    if (paragraphName == ids[1])
                                    {
                                        var words = paragraph.GetFiles();
                                        foreach (var word in words)
                                        {
                                            var wordName = word.Name.Split(".json")[0];
                                            wordName = Regex.Replace(wordName, @"word", "");
                                            if (wordName == ids[2])
                                            {
                                                string s1;
                                                Realization current = new Realization();
                                                using (var f1 = new StreamReader(word.FullName))
                                                {
                                                    while ((s1 = f1.ReadLine()) != null)
                                                    {
                                                        current = JsonConvert.DeserializeObject<Realization>(s1);
                                                        
                                                    }
                                                }
                                                try
                                                {
                                                    bool fieldExists = false;
                                                    foreach (var field in current.realizationFields)
                                                    {
                                                        if (field.Key == currentField)
                                                        {
                                                            if (current.realizationFields[field.Key].Contains(currentFieldValue))
                                                            {
                                                                fieldExists = true;
                                                                break;
                                                            }
                                                            current.realizationFields[field.Key].Add(currentFieldValue);
                                                            FileStream fs = new FileStream(word.FullName, FileMode.Create);
                                                            using (StreamWriter w = new StreamWriter(fs))
                                                            {
                                                                w.Write(current.Jsonize());
                                                            }
                                                            fieldExists = true;
                                                            break;
                                                        }
                                                    }
                                                    if (!fieldExists)
                                                    {
                                                        current.realizationFields.Add(currentField, new List<string>());
                                                        current.realizationFields[currentField].Add(currentFieldValue);
                                                        FileStream fs = new FileStream(word.FullName, FileMode.Create);
                                                        using (StreamWriter w = new StreamWriter(fs))
                                                        {
                                                            w.Write(current.Jsonize());
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                    current.realizationFields = new Dictionary<string, List<string>>();
                                                    current.realizationFields.Add(currentField, new List<string>());
                                                    current.realizationFields[currentField].Add(currentFieldValue);
                                                    FileStream fs = new FileStream(word.FullName, FileMode.Create);
                                                    using (StreamWriter w = new StreamWriter(fs))
                                                    {
                                                        w.Write(current.Jsonize());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }                    
                }
            }

            textList = getTexts();
            fieldsList = getFields();
        }

        
    }
}