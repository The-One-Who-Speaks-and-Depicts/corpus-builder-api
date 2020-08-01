using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronPython.Hosting;
using LiteDB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Scripting.Hosting;
using CorpusDraftCSharp;
using Newtonsoft.Json;

namespace CroatianProject.Pages
{
    public class AddTextModel : PageModel
    {
       
        public int Rows { get; set; } = 20;
        public int Cols { get; set; } = 50;
        

        private IHostingEnvironment _environment;
        public AddTextModel(IHostingEnvironment environment)
        {
        _environment = environment;
        }


        [BindProperty]
        public string googleDocPath { get; set; } = "";
        [BindProperty]
        public Document analyzedDocument { get; set; } = new Document();
        [BindProperty]
        public string documentPicked { get; set; }
        [BindProperty]
        public List<Document> documents
        {
            get
            {
                List<Document> deserializedDocuments = new List<Document>();
                try
                {
                    DirectoryInfo docDirectory = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents"));
                    var jsonedDocuments = docDirectory.GetFiles();
                    foreach (var doc in jsonedDocuments)
                    {
                        using (StreamReader r = new StreamReader(doc.FullName))
                        {
                            deserializedDocuments.Add(JsonConvert.DeserializeObject<Document>(r.ReadToEnd()));
                        }
                    }
                    return deserializedDocuments;
                }
                catch
                {
                    return deserializedDocuments;
                }
                
            }
        }
        [BindProperty]
        public string processedString { get; set; }
        [BindProperty]
        public string textName { get; set; }
        [BindProperty]
        public string stopSymbols { get; set; }
        [BindProperty]
        public bool decapitalisation { get; set; }

        [HttpGet]
        public void OnGet(string documentPicked)
        {
            try
            {
                var documentID = documentPicked.Split('_')[0];
                foreach (var doc in documents)
                {
                    if (doc.documentID == documentID)
                    {
                        googleDocPath = doc.googleDocPath;
                        analyzedDocument = doc;
                    }
                }
            }
            catch
            {

            }            
        }
       

        public void OnPostProcess()
        {
            
            // TBD: issue of first priority
            /*
            try
            {                
                ScriptEngine engine = Python.CreateEngine();
                ScriptScope scope = engine.CreateScope();
                var paths = engine.GetSearchPaths();
                var packagePath = Path.Combine(_environment.ContentRootPath, "Packages");
                paths.Add(packagePath);
                engine.SetSearchPaths(paths);
                var pythonFilePath = Path.Combine(_environment.ContentRootPath, "Scripts", "analysis.py");
                engine.ExecuteFile(pythonFilePath, scope);
                dynamic function = scope.GetVariable("analysis");
                IList<object> result = function(processedString);
                IList<object> paragraphs = (IList<object>)result[0]; // параграфы
                IList<object> tagged_by_paragraphs = (IList<object>)result[1]; // параграфы по словах по параграфам
                IList<object> tagged_alphabetically = (IList<object>)result[2]; // слова в алфавитном
                var dirTexts = Path.Combine(_environment.ContentRootPath, "database", "texts");
                DirectoryInfo textDirectoryInfo = new DirectoryInfo(dirTexts);
                if (!textDirectoryInfo.Exists)
                {
                    throw new Exception("Не существует директории с текстами!");
                }
                var directoriesInfo = textDirectoryInfo.GetDirectories();
                List<Text> texts = new List<Text>();
                foreach (var directory in directoriesInfo)
                {
                    var textsInfo = directory.GetFiles();
                    foreach (var text in textsInfo)
                    {
                        FileStream fs = new FileStream(text.FullName, FileMode.Open);
                        using (StreamReader r = new StreamReader(fs))
                        {
                            string jsonizedText = r.ReadLine();
                            texts.Add(JsonConvert.DeserializeObject<Text>(jsonizedText));
                        }
                    }                    
                }                
                if (texts.Count < 1)
                {
                    Exception e = new Exception("В базе данных нет текстов!");
                    throw e;
                }
                List<int> textIDs = new List<int>();
                foreach(Text text in texts)
                {
                    if (MyExtensions.isNum(text.documentID))
                    {
                        textIDs.Add(int.Parse(text.documentID)); 
                    }
                    else
                    {
                        Exception e = new Exception(new string("Cannot parse " + text.documentID));
                        throw e;
                    }
                    
                }
                textIDs.Sort();
                var currentText = texts.Where(text => text.documentID == textIDs[textIDs.Count - 1].ToString()).First();
                var sections = new List<Clause>();
                int currentParagraphID = 0; 
                foreach (var paragraph in paragraphs)
                {
                    var section = new Clause(currentText, currentParagraphID.ToString(), paragraph.ToString());
                    var dirTextData = Path.Combine(dirTexts, currentText.textID);
                    Directory.CreateDirectory(dirTextData);
                    var dirParagraphData = Path.Combine(dirTextData, "paragraphs");
                    Directory.CreateDirectory(dirParagraphData);
                    string paragraphInJSON = section.Jsonize(); 
                    var ClauseDBfile = Path.Combine(dirParagraphData, "paragraph" + currentParagraphID.ToString() + ".json");
                    FileStream fs = new FileStream(ClauseDBfile, FileMode.Create);
                    using (StreamWriter w = new StreamWriter(fs))
                    {
                        w.Write(paragraphInJSON);
                    }
                    currentParagraphID += 1;
                    sections.Add(section);
                }
                List<Realization> realizations = new List<Realization>();
                int currentParagraph = 0;
                foreach (var paragraph in tagged_by_paragraphs)
                {
                    IList<object> words = (IList<object>)paragraph; // слова в параграфах
                    int currentWord = 0;
                    foreach (var word in words)
                    {
                        //IList<object> tuple = (IList<object>)word; // кортеж для каждого слова
                        string lexeme = (string)word;
                        lexeme = lexeme.Trim();
                        var token = new Realization(sections[currentParagraph], currentWord.ToString(), lexeme);
                        var dirTextData = Path.Combine(dirTexts, currentText.textID);
                        Directory.CreateDirectory(dirTextData);
                        var dirParagraphData = Path.Combine(dirTextData, "paragraphs");
                        Directory.CreateDirectory(dirParagraphData);
                        string tokenInJSON = token.Jsonize();
                        var dirWordData = Path.Combine(dirParagraphData, "words[" + sections[currentParagraph].clauseID + "]");
                        Directory.CreateDirectory(dirWordData);
                        var wordDBfile = Path.Combine(dirWordData, "word" + currentWord.ToString() + ".json");
                        FileStream fs = new FileStream(wordDBfile, FileMode.Create);
                        using (StreamWriter w = new StreamWriter(fs))
                        {
                            w.Write(tokenInJSON);
                        }
                        currentWord += 1;
                        realizations.Add(token);
                    }
                    currentParagraph += 1;
                }
                SortedSet<string> dictionaryUnits_realizations = new SortedSet<string>();
                foreach (Realization realization in realizations)
                {
                    dictionaryUnits_realizations.Add(realization.lexeme);
                }
                List<string> dictionary = new List<string>();
                foreach (var word in tagged_alphabetically)
                {
                    string new_word = (string)word;
                    new_word = new_word.Trim();
                    dictionary.Add(new_word);
                }
                List<DictionaryUnit> alphabeticalDictionary = new List<DictionaryUnit>();
                foreach (string unit_realization in dictionaryUnits_realizations)
                {
                    var units = dictionary.Where((unit) => unit == unit_realization.ToLower());
                    foreach (var unit in units)
                    {
                        var dictionaryUnits = realizations.Where((realization) => realization.lexeme.ToLower() == unit);
                        List<Realization> dictionaryUnitsConverted = new List<Realization>();
                        foreach (var dictionaryunit in dictionaryUnits)
                        {
                            dictionaryUnitsConverted.Add(dictionaryunit);
                        }
                        alphabeticalDictionary.Add(new DictionaryUnit(unit_realization, dictionaryUnitsConverted));
                    }                    
                }
                var dirData = Path.Combine(_environment.ContentRootPath, "database");
                Directory.CreateDirectory(dirData);
                var dirDictionary = Path.Combine(dirData, "dictionary");
                Directory.CreateDirectory(dirDictionary);
                var dirTextDictionary = Path.Combine(dirDictionary, currentText.textID);
                Directory.CreateDirectory(dirTextDictionary);
                var unitNumber = 0;
                foreach (var wordInOrder in alphabeticalDictionary)
                {
                    var wordInDictfile = Path.Combine(dirTextDictionary, "dictionaryUnit[" + unitNumber + "]" + ".json");
                    FileStream fs = new FileStream(wordInDictfile, FileMode.Create);
                    using (StreamWriter w = new StreamWriter(fs))
                    {
                        w.Write(wordInOrder.Jsonize());
                    }
                    unitNumber++;
                }
            }
            catch (Exception e)
            {
                // implement logging
                FileStream fs = new FileStream("result1.txt", FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(e.Message);
                }
            }
            */
            
        }
        


    }
}

