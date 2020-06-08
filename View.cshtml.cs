﻿using System;
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
        public List<List<Dictionary<string, string>>>wordsWithTags = new List<List<Dictionary<string, string>>>();
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
                var neededWords = new List<List<Realization>>();
                if (String.IsNullOrEmpty(feature))
                {
                    if (wordSearched == null)
                    {
                        neededWords.Add(acquiredForms);
                    }
                    else
                    {
                        neededWords.Add(acquiredForms.Where((realization) => realization.lexeme == wordSearched).ToList());
                    }
                }
                else
                {
                    if (wordSearched == null)
                    {                     
                        
                        List<string> groupsOfFeaures = feature.Split(" & ").ToList();
                        for (int l = 0; l < groupsOfFeaures.Count; l++)
                        {                            
                            List<string> features = groupsOfFeaures[l].Split(' ').ToList();
                            for (int i = 0; i < features.Count; i++)
                            {
                                var currentList = new List<Realization>();
                                if (!String.IsNullOrEmpty(features[i]))
                                {                                    
                                    var searchedFeature = features[i];
                                    if (searchedFeature[0] == '!')
                                    {
                                        searchedFeature = searchedFeature.Replace("!", "");
                                        var searchedKeyAndValue = searchedFeature.Split(":");
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
                                                    var wordsWithNoValues = currentList.Where(((realization) => realization.realizationFields == null)).ToList();
                                                    var wordsWithValues = currentList.Where((realization) => realization.realizationFields != null);
                                                    var wordsWithoutKey = wordsWithValues.Where((realization) => !realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                                    var wordsWithOtherValue = wordsWithValues.Where((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                                    wordsWithOtherValue = wordsWithOtherValue.Where((realization) => !realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1])).ToList();
                                                    currentList.Clear();
                                                    currentList.AddRange(wordsWithoutKey);
                                                    currentList.AddRange(wordsWithOtherValue);
                                                    currentList.AddRange(wordsWithNoValues);
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
                                                var wordsWithValue = wordsWithKey.Where((realization) => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1])).ToList();
                                                currentList.AddRange(wordsWithValue);
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
                                                    var wordswithFields = currentList.Where((realization) => realization.realizationFields != null).ToList();
                                                    var wordsWithKey = wordswithFields.Where((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                                    var wordsWithValue = wordsWithKey.Where((realization) => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1])).ToList();
                                                    currentList.Clear();
                                                    currentList.AddRange(wordsWithValue);
                                                }
                                                catch (Exception e)
                                                {
                                                    Debug.WriteLine(e.Message);
                                                }
                                            }
                                        }
                                    }                                    
                                }
                                if (currentList.Count > 0)
                                {
                                    List<Realization> finalList = currentList.OrderBy(realization => realization.documentID).ThenBy(realization => realization.clauseID).ThenBy(realization => realization.realizationID).ToList();
                                    neededWords.Add(finalList);
                                }

                            }                            
                        }                        
                    }
                    else
                    {
                        var neededLexemes = acquiredForms.Where((realization) => (realization.MaskMatches(wordSearched))).ToList();
                        List<string> groupsOfFeatures = feature.Split(" & ").ToList();

                        for (int l = 0; l < groupsOfFeatures.Count; l++)
                        {
                            List<string> features = feature.Split(' ').ToList();
                            for (int i = 0; i < features.Count; i++)
                            {
                                var currentList = new List<Realization>();
                                if (!String.IsNullOrEmpty(features[i]))
                                {
                                    var searchedFeature = features[i];
                                    if (searchedFeature[0] == '!')
                                    {
                                        searchedFeature = searchedFeature.Replace("!", "");
                                        var searchedKeyAndValue = searchedFeature.Split(":");
                                        if (i == 0)
                                        {
                                            try
                                            {
                                                var wordsWithFeatures = new List<Realization>();
                                                foreach (var unit in neededLexemes)
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
                                                var wordsWithNoFields = neededLexemes.Where((realization) => realization.realizationFields == null);
                                                currentList.AddRange(wordsWithoutKey);
                                                currentList.AddRange(wordsWithOtherValue);
                                                currentList.AddRange(wordsWithNoFields);
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
                                                    var wordsWithNoValues = currentList.Where(((realization) => realization.realizationFields == null)).ToList();
                                                    var wordsWithValues = currentList.Where((realization) => realization.realizationFields != null);
                                                    var wordsWithoutKey = wordsWithValues.Where((realization) => !realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                                    var wordsWithOtherValue = wordsWithValues.Where((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
                                                    wordsWithOtherValue = wordsWithOtherValue.Where((realization) => !realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1])).ToList();
                                                    currentList.Clear();
                                                    currentList.AddRange(wordsWithoutKey);
                                                    currentList.AddRange(wordsWithOtherValue);
                                                    currentList.AddRange(wordsWithNoValues);
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
                                        if (i == 0)
                                        {
                                            try
                                            {
                                                var wordsWithFeatures = new List<Realization>();
                                                foreach (var unit in neededLexemes)
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
                                                var wordsWithValue = wordsWithKey.Where((realization) => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1])).ToList();
                                                currentList.AddRange(wordsWithValue);
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
                                                    var wordswithFields = currentList.Where((realization) => realization.realizationFields != null).ToList();
                                                    var wordsWithKey = wordswithFields.Where((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0])).ToList();
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
                                if (currentList.Count > 0)
                                {
                                    List<Realization> finalList = currentList.OrderBy(realization => realization.documentID).ThenBy(realization => realization.clauseID).ThenBy(realization => realization.realizationID).ToList();
                                    neededWords.Add(finalList);
                                }
                            }                            
                        }                     
                        
                        
                    }
                }
                foreach (var foundWords in neededWords)
                {
                    string finalWordValues = " ";
                    int fieldCounter = 0;
                    List<Dictionary<string, string>> searchUnit = new List<Dictionary<string, string>>();
                    foreach (var foundWord in foundWords)
                    {
                        Dictionary<string, string> foundItem = new Dictionary<string, string>();
                        if (foundWord.realizationFields == null)
                        {
                            foundItem.Add(foundWord.lexeme, "");
                        }
                        else
                        {
                            foreach (var field in foundWord.realizationFields)
                            {
                                try
                                {
                                    finalWordValues += field.Key;
                                    finalWordValues += ":";
                                    for (int i = 0; i < field.Value.Count; i++)
                                    {
                                        finalWordValues += field.Value[i];
                                        finalWordValues += " ";
                                    }
                                    fieldCounter++;
                                    if (fieldCounter < foundWord.realizationFields.Count)
                                    {
                                        finalWordValues += " ||";
                                    }
                                    foundItem.Add(foundWord.lexeme + "  ", finalWordValues);
                                }
                                catch
                                {

                                }
                            }
                        }
                        searchUnit.Add(foundItem);
                        
                    }
                    wordsWithTags.Add(searchUnit);
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
                var neededWords = new List<List<DictionaryUnit>>();
                if (String.IsNullOrEmpty(feature))
                {
                    if (wordSearched == null)
                    {
                        neededWords.Add(acquiredForms);
                    }
                    else
                    {
                        var acquiredWords = new List<DictionaryUnit>();
                        foreach (var unit in acquiredForms)
                        {
                            foreach (var realization in unit.realizations)
                            {
                                if (realization.lexeme == wordSearched)
                                {
                                    acquiredWords.Add(unit);
                                }
                            }
                        }
                        neededWords.Add(acquiredWords);
                    }
                }
                else
                {
                    if (wordSearched == null)
                    {
                        List<string> groupOfFeatures = feature.Split(" & ").ToList();
                        for (int l = 0; l < groupOfFeatures.Count; l++)
                        {
                            
                            List<string> features = groupOfFeatures[l].Split(' ').ToList();
                            for (int i = 0; i < features.Count; i++)
                            {
                                var currentList = new List<DictionaryUnit>();
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
                                                var wordsWithFeatures = new List<DictionaryUnit>();
                                                foreach (var unit in acquiredForms)
                                                {
                                                    try
                                                    {
                                                        unit.realizations.All((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]));
                                                        wordsWithFeatures.Add(unit);
                                                    }
                                                    catch
                                                    {
                                                        currentList.Add(unit);
                                                    }
                                                }
                                                var wordsWithoutKey = wordsWithFeatures.Where((unit) => !unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                var wordsWithOtherValue = wordsWithFeatures.Where((unit) => unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                wordsWithOtherValue = wordsWithOtherValue.Where((unit) => !unit.realizations.All(realization => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1]))).ToList();
                                                currentList.AddRange(wordsWithoutKey);
                                                currentList.AddRange(wordsWithOtherValue);
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
                                                    var wordsWithFeatures = new List<DictionaryUnit>();
                                                    var wordsWithNoFeatures = new List<DictionaryUnit>();
                                                    foreach (var unit in currentList)
                                                    {
                                                        try
                                                        {
                                                            unit.realizations.All((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]));
                                                            wordsWithFeatures.Add(unit);
                                                        }
                                                        catch
                                                        {
                                                            wordsWithNoFeatures.Add(unit);
                                                        }
                                                    }
                                                    var wordsWithoutKey = wordsWithFeatures.Where((unit) => !unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                    var wordsWithOtherValue = wordsWithFeatures.Where((unit) => unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                    wordsWithOtherValue = wordsWithOtherValue.Where((unit) => !unit.realizations.All(realization => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1]))).ToList();
                                                    currentList.Clear();
                                                    currentList.AddRange(wordsWithoutKey);
                                                    currentList.AddRange(wordsWithOtherValue);
                                                    if (wordsWithFeatures.Count > 0)
                                                    {
                                                        currentList.AddRange(wordsWithNoFeatures);
                                                    }
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
                                                var wordsWithFeatures = new List<DictionaryUnit>();
                                                foreach (var unit in acquiredForms)
                                                {
                                                    try
                                                    {
                                                        unit.realizations.Any((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]));
                                                        wordsWithFeatures.Add(unit);
                                                    }
                                                    catch
                                                    {
                                                        Debug.WriteLine("Word without fields caught!");
                                                    }
                                                }
                                                var wordsWithKey = wordsWithFeatures.Where((unit) => unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                var wordsWithValue = wordsWithKey.Where((unit) => unit.realizations.Any(realization => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1]))).ToList();
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
                                                    var wordsWithFeatures = new List<DictionaryUnit>();
                                                    foreach (var unit in currentList)
                                                    {
                                                        try
                                                        {
                                                            unit.realizations.All((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]));
                                                            wordsWithFeatures.Add(unit);
                                                        }
                                                        catch
                                                        {
                                                            Debug.WriteLine("Word without fields caught!");
                                                        }
                                                    }
                                                    var wordsWithKey = wordsWithFeatures.Where((unit) => unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                    var wordsWithValue = wordsWithKey.Where((unit) => unit.realizations.Any(realization => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1]))).ToList();
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
                                if (currentList.Count > 0)
                                {
                                    List<DictionaryUnit> finalList = currentList.OrderBy(unit => unit.lemma).ToList();
                                    neededWords.Add(finalList);
                                }

                            }
                        }                        

                        
                        
                    }
                    else
                    {
                        var neededLexemes = new List<DictionaryUnit>();
                        foreach (var unit in acquiredForms)
                        {
                            foreach (var realization in unit.realizations)
                            {
                                if (realization.lexeme == wordSearched)
                                {
                                    neededLexemes.Add(unit);
                                }
                            }
                        }
                        var groupsOfFeatures = feature.Split(" & ").ToList();
                        for (var l = 0; l < groupsOfFeatures.Count; l++)
                        {
                            var currentList = new List<DictionaryUnit>();
                            List<string> features = groupsOfFeatures[l].Split(' ').ToList();
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
                                                var wordsWithFeatures = new List<DictionaryUnit>();
                                                foreach (var unit in neededLexemes)
                                                {
                                                    try
                                                    {
                                                        unit.realizations.All((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]));
                                                        wordsWithFeatures.Add(unit);
                                                    }
                                                    catch
                                                    {
                                                        currentList.Add(unit);
                                                    }
                                                }
                                                var wordsWithoutKey = wordsWithFeatures.Where((unit) => !unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                var wordsWithOtherValue = wordsWithFeatures.Where((unit) => unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                wordsWithOtherValue = wordsWithOtherValue.Where((unit) => !unit.realizations.All(realization => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1]))).ToList();
                                                currentList.AddRange(wordsWithoutKey);
                                                currentList.AddRange(wordsWithOtherValue);
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
                                                    var wordsWithFeatures = new List<DictionaryUnit>();
                                                    var wordsWithNoFeatures = new List<DictionaryUnit>();
                                                    foreach (var unit in currentList)
                                                    {
                                                        try
                                                        {
                                                            unit.realizations.All((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]));
                                                            wordsWithFeatures.Add(unit);
                                                        }
                                                        catch
                                                        {
                                                            wordsWithNoFeatures.Add(unit);
                                                        }
                                                    }
                                                    var wordsWithoutKey = wordsWithFeatures.Where((unit) => !unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                    var wordsWithOtherValue = wordsWithFeatures.Where((unit) => unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                    wordsWithOtherValue = wordsWithOtherValue.Where((unit) => !unit.realizations.All(realization => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1]))).ToList();
                                                    currentList.Clear();
                                                    currentList.AddRange(wordsWithoutKey);
                                                    currentList.AddRange(wordsWithOtherValue);
                                                    if (wordsWithFeatures.Count > 0)
                                                    {
                                                        currentList.AddRange(wordsWithNoFeatures);
                                                    }
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
                                                var wordsWithFeatures = new List<DictionaryUnit>();
                                                foreach (var unit in neededLexemes)
                                                {
                                                    try
                                                    {
                                                        unit.realizations.Any((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]));
                                                        wordsWithFeatures.Add(unit);
                                                    }
                                                    catch
                                                    {
                                                        Debug.WriteLine("Word without fields caught!");
                                                    }
                                                }
                                                var wordsWithKey = wordsWithFeatures.Where((unit) => unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                var wordsWithValue = wordsWithKey.Where((unit) => unit.realizations.Any(realization => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1]))).ToList();
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
                                                    var wordsWithFeatures = new List<DictionaryUnit>();
                                                    foreach (var unit in currentList)
                                                    {
                                                        try
                                                        {
                                                            unit.realizations.All((realization) => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]));
                                                            wordsWithFeatures.Add(unit);
                                                        }
                                                        catch
                                                        {
                                                            Debug.WriteLine("Word without fields caught!");
                                                        }
                                                    }
                                                    var wordsWithKey = wordsWithFeatures.Where((unit) => unit.realizations.Any(realization => realization.realizationFields.ContainsKey(searchedKeyAndValue[0]))).ToList();
                                                    var wordsWithValue = wordsWithKey.Where((unit) => unit.realizations.Any(realization => realization.realizationFields[searchedKeyAndValue[0]].Contains(searchedKeyAndValue[1]))).ToList();
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
                                if (currentList.Count > 0)
                                {
                                    List<DictionaryUnit> finalList = currentList.OrderBy(unit => unit.lemma).ToList();
                                    neededWords.Add(finalList);
                                }

                            }
                        }                        

                        
                    }
                }
                foreach (var foundGroup in neededWords)
                {
                    List<Dictionary<string, string>> searchUnit = new List<Dictionary<string, string>>();
                    foreach (var word in foundGroup)
                    {
                        var foundItem = new Dictionary<string, string>();
                        foundItem.Add(word.lemma, "");
                        searchUnit.Add(foundItem);
                    }
                    wordsWithTags.Add(searchUnit);                   
                }

                textList = getTexts();
                fieldsList = getFields();
            }            
        }
    }
}