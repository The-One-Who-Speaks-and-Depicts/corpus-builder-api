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
        public void OnPost()
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
		dirResults = dirResults.OrderBy(dirResult => dirResult.FullName).ToArray();
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
	    acquiredForms = acquiredForms.OrderBy(realization => realization.documentID).ThenBy(realization => realization.clauseID).ThenBy(realization => realization.realizationID).ToList();
            foreach (var foundWord in acquiredForms)
            {
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
                        else
                        {
                            textByWords.Add("<br />");
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

        public void OnPostChange()
        {
          string pattern = @"\{.*?\}";
          Regex rgx = new Regex(pattern);
          var words = rgx.Matches(currentText);
          var dirTexts = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "texts"));
          var texts = dirTexts.GetDirectories();
          foreach (Match word in words)
          {
            string pattern_id = @"[0-9]{1,}\|[0-9]{1,}\|[0-9]{1,}";
            Regex rgx_id = new Regex(pattern_id);
            var acquiredId = Regex.Match(word.Value, pattern_id);
            var commonId = acquiredId.Value.Split('|');
            var textId = commonId[0];
            var clauseId = commonId[1];
            var wordId = commonId[2];
            foreach (var text in texts)
            {
              var files = text.GetFiles();
              using (StreamReader r = new StreamReader(files[0].FullName))
              {
                var textInJSON = JsonConvert.DeserializeObject<Text>(r.ReadToEnd());
                if (textId == textInJSON.documentID)
                {
                  var dirParagraphs = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "texts", textInJSON.textID, "paragraphs"));
                  var paragraphs = dirParagraphs.GetDirectories();
                  foreach (var paragraph in paragraphs)
                  {
                    if (paragraph.Name.Contains('[' + clauseId + ']'))
                    {
                      var wordFiles = paragraph.GetFiles();
                      foreach (var wordFile in wordFiles)
                      {
                        Realization transferredRealization = new Realization();
                        using (StreamReader rWord = new StreamReader(wordFile.FullName))
                        {
                          transferredRealization = JsonConvert.DeserializeObject<Realization>(rWord.ReadToEnd());
                        }//
                          if (transferredRealization.realizationID == wordId)
                          {
                            if (transferredRealization.realizationFields == null && !word.Value.Contains(" => "))
                            {
                              //do nothing
                            }
                            else if (transferredRealization.realizationFields != null && !word.Value.Contains(" => "))
                            {
                              transferredRealization.realizationFields = null;
                            }
                            else if (transferredRealization.realizationFields == null && word.Value.Contains(" => "))
                            {
                              var allFields = word.Value.Split(" => ")[1];
                              var splitFields = allFields.Split(";<br>");
                              foreach (var field in splitFields)
                              {
                                if (field.Contains(":"))
                                {
                                  var FieldAndValue = field.Split(":");
                                  List<string> fieldValues = new List<string>();
                                  if (FieldAndValue[1].Contains(";"))
                                  {
                                    var preliminaryValues = FieldAndValue[1].Split(";").ToList();
                                    foreach (var value in preliminaryValues)
                                    {
                                      if (value != "}" && value != "")
                                      {
                                        fieldValues.Add(value);
                                      }
                                    }
                                  }
                                  else
                                  {
                                      fieldValues.Add(FieldAndValue[1].Split('}')[0]);
                                  }
                                  transferredRealization.realizationFields = new Dictionary<string, List<string>>();
                                  transferredRealization.realizationFields[FieldAndValue[0]] = fieldValues;
                                }
                              }
                            }
                            else
                            {
                              var allFields = word.Value.Split(" => ")[1];
                              var splitFields = allFields.Split(";<br>");
                              foreach (var field in splitFields)
                              {
                                if (field.Contains(":"))
                                {
                                  var FieldAndValue = field.Split(":");
                                  List<string> fieldValues = new List<string>();
                                  if (FieldAndValue[1].Contains(";"))
                                  {
                                    var preliminaryValues = FieldAndValue[1].Split(";").ToList();
                                    foreach (var value in preliminaryValues)
                                    {
                                      if (value != "}" && value != "")
                                      {
                                        fieldValues.Add(value);
                                      }
                                    }
                                  }
                                  else
                                  {
                                      fieldValues.Add(FieldAndValue[1].Split('}')[0]);
                                  }
                                  bool keyPresent = false;
                                  foreach (var key in transferredRealization.realizationFields.Keys)
                                  {
                                    if (key == FieldAndValue[0])
                                    {
                                      foreach (var value in fieldValues)
                                      {
                                        if (!transferredRealization.realizationFields[key].Contains(value) && value != "}" && value != "")
                                        {
                                          transferredRealization.realizationFields[key].Add(value);
                                        }
                                      }
                                    }
                                    keyPresent = true;
                                  }
                                  if (!keyPresent)
                                  {
                                    transferredRealization.realizationFields[FieldAndValue[0]] = fieldValues;
                                  }
                                }
                              }
                            }
                          }
                        using (StreamWriter wr = new StreamWriter(wordFile.FullName))
                        {
                          wr.WriteLine(transferredRealization.Jsonize());
                        }
                        var dirDict = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "dictionary", textInJSON.textID));
                        var dictFiles = dirDict.GetFiles();
                        DictionaryUnit transferredDictionary = new DictionaryUnit();
                        foreach (var file in dictFiles)
                        {
                          try
                          {
                            using (StreamReader rDict = new StreamReader(file.FullName))
                            {
                              transferredDictionary = JsonConvert.DeserializeObject<DictionaryUnit>(rDict.ReadToEnd());
                            }
                              List<Realization> ChangedRealizations = transferredDictionary.realizations
                                                                    .Where(realization => realization.documentID == transferredRealization.documentID && realization.clauseID == transferredRealization.clauseID && realization.realizationID == transferredRealization.realizationID)
                                                                    .ToList();
                              if (ChangedRealizations.Count == 1)
                              {
                                if (transferredDictionary.realizations.Count == 1)
                                {
                                  List<Realization> newRealizations = new List<Realization>();
                                  newRealizations.Add(transferredRealization);
                                  transferredDictionary.realizations = newRealizations;
                                  using (StreamWriter wrDict = new StreamWriter(file.FullName))
                                  {
                                    wrDict.WriteLine(transferredDictionary.Jsonize());
                                  }
                                }
                                else
                                {
                                  List<Realization> NonChangedRealizations = transferredDictionary.realizations
                                                                        .Where(realization => realization.documentID != transferredRealization.documentID || realization.clauseID != transferredRealization.clauseID || realization.realizationID != transferredRealization.realizationID)
                                                                        .ToList();
                                  NonChangedRealizations.Add(transferredRealization);
                                  transferredDictionary.realizations = NonChangedRealizations;
                                  using (StreamWriter wrDict = new StreamWriter(file.FullName))
                                  {
                                      wrDict.WriteLine(transferredDictionary.Jsonize());
                                  }
                                }

                              }

                          }
                          catch
                          {

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
}
