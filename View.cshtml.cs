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
using System.Text.RegularExpressions;

namespace CroatianProject.Pages
{
    public class ViewModel : PageModel
    {
        
        [BindProperty]
        public string wordSearched { get; set; }
        [BindProperty]
        public string feature { get; set; }
        public List<string> wordsWithTags = new List<string>();
        [BindProperty]
        public List<Document> docList { get; set; }
        [BindProperty]
        public List<string> fieldsList { get; set; }
        [BindProperty]
        public string textName { get; set; }
        [BindProperty]
        public string docName { get; set; }

        private IHostingEnvironment _environment;
        public ViewModel(IHostingEnvironment environment)
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
        public List<Document> getDocs()
        {
            List<Document> existingDocs = new List<Document>();            
            try
            {
                var directory = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents"));
                var docs = directory.GetFiles();
                foreach (var doc in docs)
                {
                    Document document = new Document();
                    using (StreamReader r = new StreamReader(doc.FullName))
                    {
                        var deserialized = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                        document = deserialized;
                    }
                    existingDocs.Add(document);
                }
            }
            catch
            {

            }
            return existingDocs;
        }
        

        public void OnPostShow()
        {
            List<Field> existingFields = new List<Field>();
            List<Realization> acquiredRealizations = new List<Realization>();
            var docDir = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents"));
            var docs = docDir.GetFiles();
            List<Document> requiredDocuments = new List<Document>();
            foreach (var doc in docs)
            {
                Document document = new Document();
                using (StreamReader r = new StreamReader(doc.FullName))
                {
                    var deserialized = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                    document = deserialized;
                }
                requiredDocuments.Add(document);
            }
            if (!String.IsNullOrEmpty(feature))
            {
                var directory = Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields");
                DirectoryInfo fieldsDirectory = new DirectoryInfo(directory);
                var fields = fieldsDirectory.GetFiles();
                foreach (var field in fields)
                {
                    using (StreamReader r = new StreamReader(field.FullName))
                    {
                        existingFields.Add(JsonConvert.DeserializeObject<Field>(r.ReadToEnd()));
                        
                    }
                }
                List<string> requiredFields = feature.Split("||").ToList();
                List<string> documentFields = new List<string>();
                List<string> textFields = new List<string>();
                List<string> clauseFields = new List<string>();
                List<string> realizationFields = new List<string>();
                List<string> graphemeFields = new List<string>();
                for (int i = 0; i < existingFields.Count; i++)
                {
                    if (existingFields[i].type == "Document")
                    {
                        documentFields.AddRange(requiredFields.Where(f => (f.Split(':')[0] == existingFields[i].name) || (f.Split(':')[0].Replace("!", "") == existingFields[i].name)));
                    }
                    else if (existingFields[i].type == "Text")
                    {
                        textFields.AddRange(requiredFields.Where(f => (f.Split(':')[0] == existingFields[i].name) || (f.Split(':')[0].Replace("!", "") == existingFields[i].name)));
                    }
                    else if (existingFields[i].type == "Clause")
                    {
                        clauseFields.AddRange(requiredFields.Where(f => (f.Split(':')[0] == existingFields[i].name) || (f.Split(':')[0].Replace("!", "") == existingFields[i].name)));
                    }
                    else if (existingFields[i].type == "Realization")
                    {
                        realizationFields.AddRange(requiredFields.Where(f => (f.Split(':')[0] == existingFields[i].name) || (f.Split(':')[0].Replace("!", "") == existingFields[i].name)));
                    }
                    else if (existingFields[i].type == "Grapheme")
                    {
                        graphemeFields.AddRange(requiredFields.Where(f => (f.Split(':')[0] == existingFields[i].name) || (f.Split(':')[0].Replace("!", "") == existingFields[i].name)));
                    }
                }                
                
                bool fieldsMatch = true;
                if (documentFields.Count > 0)
                {
                    requiredDocuments = requiredDocuments.Where(r => ContainsField(documentFields, r.documentMetaData)).ToList();
                    Console.WriteLine(documentFields);
                    if (requiredDocuments.Count < 1)
                    {
                        fieldsMatch = false;
                    }
                }                
                if (fieldsMatch)
                {
                    List<Text> extractedTexts = new List<Text>();
                    for (int d = 0; d < requiredDocuments.Count; d++)
                    {
                        extractedTexts.AddRange(requiredDocuments[d].texts);
                    }
                    if (textFields.Count > 0)
                    {
                        extractedTexts = extractedTexts.Where(t => ContainsField(textFields, t.textMetaData)).ToList();
                        if (extractedTexts.Count < 1)
                        {
                            fieldsMatch = false;
                        }
                    }                    
                    if (fieldsMatch)
                    {
                        List<Clause> extractedClauses = new List<Clause>();
                        for (int t = 0; t < extractedTexts.Count; t++)
                        {
                            extractedClauses.AddRange(extractedTexts[t].clauses);
                        }
                        if (clauseFields.Count > 0)
                        {
                            extractedClauses = extractedClauses.Where(c => ContainsField(clauseFields, c.clauseFields)).ToList();
                            if (extractedClauses.Count < 1)
                            {
                                fieldsMatch = false;
                            }
                        }
                        if (fieldsMatch)
                        {
                            List<Realization> extractedRealizations = new List<Realization>();
                            for (int c = 0; c < extractedClauses.Count; c++)
                            {
                                extractedRealizations.AddRange(extractedClauses[c].realizations);
                            }                            
                            if (realizationFields.Count > 0)
                            {
                                extractedRealizations = extractedRealizations.Where(r => ContainsField(realizationFields, r.realizationFields)).ToList();
                            }
                            if (extractedRealizations.Count < 1)
                            {
                                fieldsMatch = false;
                            }
                            if (graphemeFields.Count > 0)
                            {
                                extractedRealizations = extractedRealizations.Where(r => r.letters.Any(l => ContainsField(graphemeFields, l.graphemeFields))).ToList();
                            }
                            if (extractedRealizations.Count < 1)
                            {
                                fieldsMatch = false;
                            }
                            if (fieldsMatch)
                            {
                                acquiredRealizations = extractedRealizations;
                            }
                        }
                    }
                }
            }
            if (wordSearched != null)
            {
                if (acquiredRealizations.Count > 0)
                {
                    acquiredRealizations = acquiredRealizations.Where(r => r.MaskMatches(wordSearched)).ToList();
                }
                else
                {
                    for (int i = 0; i < requiredDocuments.Count; i++)
                    {
                        for (int j = 0; j < requiredDocuments[i].texts.Count; j++)
                        {
                            for (int c = 0; c < requiredDocuments[i].texts[j].clauses.Count; c++)
                            {
                                acquiredRealizations.AddRange(requiredDocuments[i].texts[j].clauses[c].realizations.Where(r => r.MaskMatches(wordSearched)).ToList());
                            }
                        }
                    }
                }
                
            }
            if (acquiredRealizations.Count > 0)
            {
                acquiredRealizations = acquiredRealizations.OrderBy(r => r.documentID).ThenBy(r => r.textID).ThenBy(r => r.clauseID).ThenBy(r => r.realizationID).ToList();
                for (int r = 0; r < acquiredRealizations.Count; r++)
                {
                    List<Realization> neighbours = requiredDocuments.Where(d => d.documentID == acquiredRealizations[r].documentID).Select(d => d.texts).ToList()[0]
                        .Where(t => t.textID == acquiredRealizations[r].textID).Select(t => t.clauses).ToList()[0]
                        .Where(c => c.clauseID == acquiredRealizations[r].clauseID).ToList()[0]
                        .realizations;
                    string document = requiredDocuments.Where(d => d.documentID == acquiredRealizations[r].documentID).ToList()[0].documentName;
                    string text = requiredDocuments.Where(d => d.documentID == acquiredRealizations[r].documentID).Select(d => d.texts).ToList()[0]
                        .Where(t => t.textID == acquiredRealizations[r].textID).ToList()[0].textName;
                    string KWIC = "";
                    KWIC += r.ToString() + ". " + document + ": " + text + "<br />";
                    for (int i = 0; i < neighbours.OrderBy(n => n.realizationID).ToList().Count; i++)
                    {
                        if (neighbours[i].realizationID != acquiredRealizations[r].realizationID)
                        {
                            KWIC += neighbours[i].Output();
                        }
                        else
                        {
                            KWIC += "<div style=\"font-weight:bold;\">" + neighbours[i].Output() + "</div>";
                        }
                    }
                    KWIC += "<br /><br /><br />";
                    wordsWithTags.Add(KWIC);
                }
            }
            docList = getDocs();
            fieldsList = getFields();
        }
        

        public bool ContainsField(List<string> fields, List<Dictionary<string, List<Value>>> unitFields)
        {
            bool taggingFound = false;
            try
            {
                for (int i = 0; i < unitFields.Count; i++)
                {
                    int coincidingFields = 0;
                    for (int f = 0; f < fields.Count; f++)
                    {
                        string key = fields[f].Split(':')[0];
                        string value = fields[f].Split(':')[1];
                        if (unitFields[i].ContainsKey(key))
                        {
                            for (int v = 0; v < unitFields[i][key].Count; v++)
                            {
                                string regexQuery = "^" + String.Join(".*", value.Split('*')) + "$";
                                if (unitFields[i][key][v].name == value || Regex.IsMatch(unitFields[i][key][v].name, regexQuery))
                                {
                                    Console.WriteLine(regexQuery + "==" + value);
                                    coincidingFields++;
                                }
                            }
                        }
                    }
                    if (coincidingFields == fields.Count)
                    {
                        taggingFound = true;
                        break;
                    }
                }
            }
            catch (NullReferenceException)
            {
                return false;
            }
            
            return taggingFound;
        }

        public void OnPostShowAlphabetically()
        {
            throw new NotImplementedException();
            /*
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
                                if (realization.MaskMatches(wordSearched))
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
                                if (realization.MaskMatches(wordSearched))
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
                    //wordsWithTags.Add(searchUnit);                   
                }

                textList = getTexts();
                fieldsList = getFields();
            }
            */
        }
    }
}