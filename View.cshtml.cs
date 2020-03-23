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
            foreach (var text in texts)
            {
                existingTexts.Add(text.Name);
            }                
            return existingTexts;
        }

        public void OnPostShow()
        {
            throw new NotImplementedException();
        }

        public void OnPostShowAlphabetically()
        {
            throw new NotImplementedException();
        }
    }
}