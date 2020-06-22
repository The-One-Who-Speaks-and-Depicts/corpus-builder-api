using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CorpusDraftCSharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace CroatianProject.Pages
{
    [BindProperties]
    public class ViewTextModel : PageModel
    {
        private IHostingEnvironment _environment;
        public string textName { get; set; }
        public List<string> textList { get; set; }
        public List<string> fieldsList { get; set; } = new List<string>();
        public List<string> textByWords = new List<string>();

        public ViewTextModel(IHostingEnvironment environment)
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
                        acquiredForms.Add(new Realization());
                    }
                }
                acquiredForms = acquiredForms.OrderBy(realization => Convert.ToInt32(realization.documentID)).ThenBy(realization => Convert.ToInt32(realization.clauseID)).ThenBy(realization => Convert.ToInt32(realization.realizationID)).ToList();
                int currClause = 0;
                foreach (var foundWord in acquiredForms)
                {
                  if (Convert.ToInt32(foundWord.clauseID) > currClause)
                  {
                    currClause++;
                    textByWords.Add("<br />");
                  }
                    try
                    {
                        if (!String.IsNullOrEmpty(foundWord.documentID))
                        {
                            string fieldsOfWord = "";
                            string hoverFields = "";
                            foreach (var field in foundWord.realizationFields)
                            {
                                fieldsOfWord += field.Key;
                                hoverFields += field.Key;
                                fieldsOfWord += ":";
                                hoverFields += ":";
                                foreach (var fieldValue in field.Value)
                                {
                                    fieldsOfWord += fieldValue;
                                    hoverFields += fieldValue;
                                    fieldsOfWord += ";";
                                    hoverFields += ";";
                                }
                                fieldsOfWord += "<br />";
                                hoverFields += "\n";
                            }
                            textByWords.Add("<span title=\"" + hoverFields + "\" data-content=\"" + fieldsOfWord + "\" class=\"word\" id=\"" + foundWord.documentID + "|" + foundWord.clauseID + "|" + foundWord.realizationID + "\"> " + foundWord.lexeme + "</span>");

                        }
                    }
                    catch
                    {
                        textByWords.Add("<span title= \"\" data-content=\"\" class=\"word\" id=\"" + foundWord.documentID + "|" + foundWord.clauseID + "|" + foundWord.realizationID + "\"> " + foundWord.lexeme + "</span>");
                    }

                }
            }
            textList = getTexts();
            fieldsList = getFields();
        }
    }
}
