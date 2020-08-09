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

        public void OnPut(string currentText)
        {
            Debug.WriteLine(currentText);
            /*
          string pattern = @"\{.*?\}";
          Regex rgx = new Regex(pattern);
          var words = rgx.Matches(currentText);
          var dirTexts = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "texts"));
          var texts = dirTexts.GetDirectories();
          List<string> documentIDs = new List<string>();
          for (int i = 0; i < words.Count; i++)
          {
            string pattern_id = @"[0-9]{1,}\|[0-9]{1,}\|[0-9]{1,}";
            Regex rgx_id = new Regex(pattern_id);
            var acquiredId = Regex.Match(words[i].Value, pattern_id);
            var commonId = acquiredId.Value.Split('|');
            var textId = commonId[0];
            if (!documentIDs.Contains(textId))
            {
              documentIDs.Add(textId);
            }
          }
          List<Text> checkedTexts = new List<Text>();
          for (int i = 0; i < texts.Length; i++)
          {
            using (StreamReader rText = new StreamReader(texts[i].GetFiles()[0].FullName))
            {
              var currText = JsonConvert.DeserializeObject<Text>(rText.ReadToEnd());
              if (documentIDs.Contains(currText.documentID))
              {
                checkedTexts.Add(currText);
              }
            }
          }
          Dictionary<DictionaryUnit, string> dictionaries = new Dictionary<DictionaryUnit, string>();
          for (int i = 0; i < checkedTexts.Count; i++)
          {
            var dirDict = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "dictionary", checkedTexts[i].textID));
            var dictFiles = dirDict.GetFiles();
            for (int j = 0; j < dictFiles.Length; j++)
            {
              using (StreamReader rDict = new StreamReader(dictFiles[j].FullName))
              {
                dictionaries[JsonConvert.DeserializeObject<DictionaryUnit>(rDict.ReadToEnd())] = dictFiles[j].FullName;
              }
            }
          }

          for (int w = 0; w < words.Count; w++)
          {
            string pattern_id = @"[0-9]{1,}\|[0-9]{1,}\|[0-9]{1,}";
            Regex rgx_id = new Regex(pattern_id);
            var acquiredId = Regex.Match(words[w].Value, pattern_id);
            var commonId = acquiredId.Value.Split('|');
            var textId = commonId[0];
            var clauseId = commonId[1];
            var wordId = commonId[2];
            for (int t = 0; t < checkedTexts.Count; t++)
            {
                if (textId == checkedTexts[t].documentID)
                {
                  var dirParagraphs = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "texts", checkedTexts[t].textID, "paragraphs"));
                  var paragraphs = dirParagraphs.GetDirectories();
                  for (int p = 0; p < paragraphs.Length; p++)
                  {
                    if (paragraphs[p].Name.Contains('[' + clauseId + ']'))
                    {
                      var wordFiles = paragraphs[p].GetFiles();
                      for (int f = 0; f < wordFiles.Length; f++)
                      {
                        Realization transferredRealization = new Realization();
                        using (StreamReader rWord = new StreamReader(wordFiles[f].FullName))
                        {
                          transferredRealization = JsonConvert.DeserializeObject<Realization>(rWord.ReadToEnd());
                        }
                          if (transferredRealization.realizationID == wordId)
                          {
                            if (transferredRealization.realizationFields == null && !words[w].Value.Contains(" => "))
                            {
                              continue;
                            }
                            else if (transferredRealization.realizationFields != null && !words[w].Value.Contains(" => "))
                            {
                              transferredRealization.realizationFields = null;
                            }
                            else if (transferredRealization.realizationFields == null && words[w].Value.Contains(" => "))
                            {
                              var allFields = words[w].Value.Split(" => ")[1];
                              var splitFields = allFields.Split(";");
                              transferredRealization.realizationFields = new Dictionary<string, List<string>>();
                              foreach (var field in splitFields)
                              {
                                List<string> fieldValues = new List<string>();
                                if (field.Contains(":"))
                                {
                                  var FieldAndValue = field.Split(":");
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
                                  transferredRealization.realizationFields[FieldAndValue[0]] = fieldValues;
                                }
                              }
                            }
                            else
                            {
                              var allFields = words[w].Value.Split(" => ")[1];
                              var splitFields = allFields.Split(";");
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
                                      keyPresent = true;
                                    }
                                  }
                                  if (!keyPresent)
                                  {
                                    transferredRealization.realizationFields[FieldAndValue[0]] = fieldValues;
                                  }
                                }
                              }
                            }
                          }
                        using (StreamWriter wr = new StreamWriter(wordFiles[f].FullName))
                        {
                          wr.WriteLine(transferredRealization.Jsonize());
                        }
                        var dictionariesPassing = dictionaries.Where(dictionary => dictionary.Key.realizations.Where(realization => realization.documentID == transferredRealization.documentID && realization.clauseID == transferredRealization.clauseID && realization.realizationID == transferredRealization.realizationID).ToList().Count > 0);

                        foreach (var item in dictionariesPassing)
                        {
                          try
                          {

                              List<Realization> ChangedRealizations = item.Key.realizations
                                                                    .Where(realization => realization.documentID == transferredRealization.documentID && realization.clauseID == transferredRealization.clauseID && realization.realizationID == transferredRealization.realizationID)
                                                                    .ToList();
                              if (ChangedRealizations.Count == 1)
                              {
                                if (item.Key.realizations.Count == 1)
                                {
                                  List<Realization> newRealizations = new List<Realization>();
                                  newRealizations.Add(transferredRealization);
                                  item.Key.realizations = newRealizations;
                                  using (StreamWriter wrDict = new StreamWriter(item.Value))
                                  {
                                    wrDict.WriteLine(item.Key.Jsonize());
                                  }
                                }
                                else
                                {
                                  List<Realization> NonChangedRealizations = item.Key.realizations
                                                                        .Where(realization => realization.documentID != transferredRealization.documentID || realization.clauseID != transferredRealization.clauseID || realization.realizationID != transferredRealization.realizationID)
                                                                        .ToList();
                                  NonChangedRealizations.Add(transferredRealization);
                                  item.Key.realizations = NonChangedRealizations;
                                  using (StreamWriter wrDict = new StreamWriter(item.Value))
                                  {
                                      wrDict.WriteLine(item.Key.Jsonize());
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
          */
        }


    }
}
