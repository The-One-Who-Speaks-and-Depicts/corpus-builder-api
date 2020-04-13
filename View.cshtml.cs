using System;
using System.Collections.Generic;
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
        public List<string> partsOfSpeech { get; set; } = new List<string>() { "Any", "N", "V", "ADVB" };
        [BindProperty]
        public string wordSearched { get; set; }
        [BindProperty]
        public string PoS { get; set; }
        public Dictionary<string, string> wordsWithTags = new Dictionary<string, string>();
        [BindProperty]
        public List<string> textList { get; set; }
        [BindProperty]
        public string textName { get; set; }

        private IHostingEnvironment _environment;
        public ViewModel(IHostingEnvironment environment)
        {
            _environment = environment;
            try
            {
                textList = getTexts();
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

        public IActionResult OnPostShow()
        {
            var directory = Path.Combine(_environment.ContentRootPath, "database", "texts");
            if ((textName == "Any") && (PoS == "Any") && (wordSearched == null))
            {
                return Redirect("./View");
            }            
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
            if (PoS == "Any")
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
                    var acquiredWords = acquiredForms.Where((realization) => realization.partOfSpeech == PoS);
                    foreach (var word in acquiredWords)
                    {                        
                        neededWords.Add(word);
                    }
                }
                else
                {
                    neededWords = acquiredForms.Where ((realization) => ((realization.partOfSpeech == PoS) && (realization.lexeme == wordSearched))).ToList();
                }
            }
            foreach (var foundWord in neededWords)
            {
                wordsWithTags.Add(foundWord.lexeme, foundWord.partOfSpeech);
            }
            return Page();
        }

        public DirectoryInfo SearchForText(string textName, string directory)
        {
            DirectoryInfo dirTexts = new DirectoryInfo(directory);
            var searchedDirectory = dirTexts.GetDirectories().Where((dir) => dir.Name == textName).First();
            return searchedDirectory;
        }

        public IActionResult OnPostShowAlphabetically()
        {
            var directory = Path.Combine(_environment.ContentRootPath, "database", "dictionary");            
            if ((textName == "Any") && (PoS == "Any") && (wordSearched == null))
            {
                return Redirect("./View");
            }            
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
            if (PoS == "Any")
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
                            if (realization.partOfSpeech == PoS)
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
                            if ((realization.partOfSpeech == PoS) && (realization.lexeme == wordSearched))
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
            return Page();
        }
    }
}