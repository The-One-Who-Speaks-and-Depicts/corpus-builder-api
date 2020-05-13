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
    public class ViewModel : PageModel
    {
        
        [BindProperty]
        public string wordSearched { get; set; }
        [BindProperty]
        public string feature { get; set; }
        public Dictionary<string, string> wordsWithTags = new Dictionary<string, string>();
        [BindProperty]
        public List<string> textList { get; set; }
        [BindProperty]
        public List<string> fieldsList { get; set; }
        [BindProperty]
        public string textName { get; set; }

        private IHostingEnvironment _environment;
        public ViewModel(IHostingEnvironment environment)
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

        public List<string> getFields()
        {
            List<string> existingFields = new List<string>();
            try
            {
                var directory = Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields");
                DirectoryInfo fieldsDirectory = new DirectoryInfo(directory);
                var fields = fieldsDirectory.GetFiles();
                existingFields.Add("Any");
                for (int i = 0; i < fields.Length; i++)
                {
                    existingFields.Add(fields[i].Name.Split(".json")[0]);
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

        public void OnPostShow()
        {
            var directory = Path.Combine(_environment.ContentRootPath, "database", "texts");
            if ((textName == "Any") && (String.IsNullOrEmpty(feature)) && (wordSearched == null))
            {
                textList = getTexts();
                fieldsList = getFields();
            }
            else
            {
                List<DirectoryInfo> searchedTexts = new List<DirectoryInfo>();
                if (textName != "Any")
                {
                    searchedTexts.Add(SearchForText(textName, directory));
                }
                else
                {
                    foreach (var text in getTexts())
                    {
                        if (text != "Any")
                        {
                            searchedTexts.Add(SearchForText(text, directory));
                        }
                    }
                }
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
                var neededWords = new List<Realization>();
                if (String.IsNullOrEmpty(feature))
                {
                    if (wordSearched == null)
                    {
                        neededWords = acquiredForms;
                    }
                    else
                    {
                        neededWords = acquiredForms.Where((realization) => realization.lexeme == wordSearched).ToList();
                    }
                }
                else
                {
                    if (wordSearched == null)
                    {                        
                        var currentList = new List<Realization>();
                        List<string> features = feature.Split(' ').ToList();
                        Debug.WriteLine(feature);
                        Debug.WriteLine(features.Count);
                        for (int i = 0; i < features.Count; i++)
                        {
                            if (!String.IsNullOrEmpty(features[i]))
                            {
                                var searchedFeature = features[i];
                                if (searchedFeature[0] == '!')
                                {
                                    searchedFeature = searchedFeature.Replace("!", "");
                                    var searchedKeyAndValue = searchedFeature.Split(":");
                                    Debug.WriteLine(searchedKeyAndValue[0] + searchedKeyAndValue[1]);
                                    if (i == 0)
                                    {
                                        try
                                        {
                                            var wordsWithFeatures = new List<Realization>();
                                            foreach (var unit in acquiredForms)
                                            {
                                                try
                                                {
                                                    unit.realizationFields.ContainsKey(searchedKeyAndValue[0]);
                                                    wordsWithFeatures.Add(unit);
                                                }
                                                catch
                                                {
                                                    Debug.WriteLine("Word without fields caught!");
                                                }
                                            }
                                            var wordsWithoutKey = wordsWithFeatures.Where((realization) => !realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                            var wordsWithOtherValue = wordsWithFeatures.Where((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                            wordsWithOtherValue = wordsWithOtherValue.Where((realization) => !realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1])).ToList();
                                            var wordsWithNoFields = acquiredForms.Where((realization) => realization.realizationFields == null);
                                            currentList.AddRange(wordsWithoutKey);
                                            currentList.AddRange(wordsWithOtherValue);
                                            currentList.AddRange(wordsWithNoFields);
                                            Debug.WriteLine(currentList.Count);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine(e.Message);
                                        }

                                    }
                                    else
                                    {
                                        if (currentList.Count > 0)
                                        {
                                            try
                                            {
                                                var wordsWithoutKey = currentList.Where((realization) => !realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                                var wordsWithOtherValue = currentList.Where((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                                wordsWithOtherValue = wordsWithOtherValue.Where((realization) => !realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1])).ToList();
                                                currentList.Clear();
                                                currentList.AddRange(wordsWithoutKey);
                                                currentList.AddRange(wordsWithOtherValue);
                                                Debug.WriteLine(currentList.Count);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine(e.Message);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var searchedKeyAndValue = searchedFeature.Split(":");
                                    Debug.WriteLine(searchedKeyAndValue[0] + searchedKeyAndValue[1]);
                                    if (i == 0)
                                    {
                                        try
                                        {
                                            var wordsWithFeatures = new List<Realization>();
                                            foreach (var unit in acquiredForms)
                                            {
                                                try
                                                {
                                                    unit.realizationFields.ContainsKey(searchedKeyAndValue[0]);
                                                    wordsWithFeatures.Add(unit);
                                                }
                                                catch
                                                {
                                                    Debug.WriteLine("Word without fields caught!");
                                                }
                                            }
                                            var wordsWithKey = wordsWithFeatures.Where((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                            Debug.WriteLine(wordsWithKey.Count);
                                            var wordsWithValue = wordsWithKey.Where((realization) => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1])).ToList();
                                            currentList.AddRange(wordsWithValue);
                                            Debug.WriteLine(currentList.Count);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine(e.Message);
                                        }
                                    }
                                    else
                                    {
                                        if (currentList.Count > 0)
                                        {
                                            try
                                            {
                                                var wordsWithKey = currentList.Where((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                                var wordsWithValue = wordsWithKey.Where((realization) => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1])).ToList();
                                                currentList.Clear();
                                                currentList.AddRange(wordsWithValue);
                                                Debug.WriteLine(currentList.Count);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine(e.Message);
                                            }
                                        }
                                    }
                                }
                            }                         
                            

                        }
                        if (currentList.Count > 0)
                        {
                            neededWords.AddRange(currentList);
                        }
                    }
                    else
                    {
                        neededWords = acquiredForms.Where((realization) => ((realization.partOfSpeech == feature) && (realization.lexeme == wordSearched))).ToList();
                    }
                }
                foreach (var foundWord in neededWords)
                {
                    wordsWithTags.Add(foundWord.lexeme, foundWord.partOfSpeech);
                }
                textList = getTexts();
                fieldsList = getFields();
            }
            
        }

        public DirectoryInfo SearchForText(string textName, string directory)
        {
            DirectoryInfo dirTexts = new DirectoryInfo(directory);
            var searchedDirectory = dirTexts.GetDirectories().Where((dir) => dir.Name == textName).First();
            return searchedDirectory;
        }

        public void OnPostShowAlphabetically()
        {
            var directory = Path.Combine(_environment.ContentRootPath, "database", "dictionary");            
            if ((textName == "Any") && (feature == "Any") && (wordSearched == null))
            {
                textList = getTexts();
                fieldsList = getFields();
            }
            else
            {
                List<DirectoryInfo> searchedTexts = new List<DirectoryInfo>();
                if (textName != "Any")
                {
                    searchedTexts.Add(SearchForText(textName, directory));
                }
                else
                {
                    foreach (var text in getTexts())
                    {
                        if (text == "Any")
                        {
                            continue;
                        }
                        searchedTexts.Add(SearchForText(text, directory));
                    }
                }
                List<DictionaryUnit> acquiredForms = new List<DictionaryUnit>();
                foreach (var text in searchedTexts)
                {
                    DirectoryInfo dirWords = new DirectoryInfo(Path.Combine(directory, text.Name));
                    var words = dirWords.GetFiles();
                    string s;
                    foreach (var word in words)
                    {
                        using (var f = new StreamReader(word.FullName))
                        {
                            while ((s = f.ReadLine()) != null)
                            {
                                acquiredForms.Add(JsonConvert.DeserializeObject<DictionaryUnit>(s));
                            }
                        }
                    }

                }
                var neededWords = new List<DictionaryUnit>();
                if (feature == "Any")
                {
                    if (wordSearched == null)
                    {
                        neededWords = acquiredForms;
                    }
                    else
                    {
                        foreach (var unit in acquiredForms)
                        {
                            foreach (var realization in unit.realizations)
                            {
                                if (realization.lexeme == wordSearched)
                                {
                                    neededWords.Add(unit);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (wordSearched == null)
                    {
                        foreach (var unit in acquiredForms)
                        {
                            foreach (var realization in unit.realizations)
                            {
                                if (realization.partOfSpeech == feature)
                                {
                                    neededWords.Add(unit);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var unit in acquiredForms)
                        {
                            foreach (var realization in unit.realizations)
                            {
                                if ((realization.partOfSpeech == feature) && (realization.lexeme == wordSearched))
                                {
                                    neededWords.Add(unit);
                                }
                            }
                        }
                    }
                }
                foreach (var foundWord in neededWords)
                {
                    wordsWithTags.Add(foundWord.lemma, foundWord.realizations[0].partOfSpeech);
                }

                textList = getTexts();
                fieldsList = getFields();
            }            
        }
    }
}