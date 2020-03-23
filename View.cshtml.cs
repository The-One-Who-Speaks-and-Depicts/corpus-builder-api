using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CroatianProject.Pages
{
    public class ViewModel : PageModel
    {
        public List<string> partsOfSpeech { get; set; } = new List<string>() { "Any", "N", "V", "ADVB" };
        public string wordSearched { get; set; }
        public string PoS { get; set; }
        public Dictionary<string, string> wordsWithTags = new Dictionary<string, string>();
        public List<string> textList { get; set; }
        public string textName { get; set; }

        private IHostingEnvironment _environment;
        public ViewModel(IHostingEnvironment environment)
        {
            _environment = environment;
            textList = getTexts();
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
            if ((textName == "Any") && (PoS == "Any") && (wordSearched.Length == 0))
            {
                return;
            }
            if (textName != "Any")
            {
                
            }
        }

        public void SearchThroughText(string textName, string directory)
        {
            DirectoryInfo dirTexts = new DirectoryInfo(directory);
            var searchedDirectory = dirTexts.GetDirectories().Where((dir) => dir.Name == textName).First();
        }

        public void OnPostShowAlphabetically()
        {
            throw new NotImplementedException();
        }
    }
}