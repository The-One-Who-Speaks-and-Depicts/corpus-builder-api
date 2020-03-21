using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronPython.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Scripting.Hosting;

namespace CroatianProject.Pages
{
    public class AddTextModel : PageModel
    {
        public string ExceptionMessage { get; private set; } = "";
        public string filePath { get; set; }
        public int Rows { get; set; } = 20;
        public int Cols { get; set; } = 50;

        private IHostingEnvironment _environment;
        public AddTextModel(IHostingEnvironment environment)
        {
        _environment = environment;
        }   
        
        [BindProperty]
        public IFormFile Upload { get; set; }
        public async Task OnPostAsync()
        {
            var dir = Path.Combine(_environment.ContentRootPath, "uploads");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, Upload.FileName);
            filePath = file;
            using (var fileStream = new FileStream(file, FileMode.Create))
            {
                await Upload.CopyToAsync(fileStream);
            }
        }

        [BindProperty]
        public string processedString { get; set; }

        public async Task OnPostProcess()
        {

            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();
            var paths = engine.GetSearchPaths();
            paths.Add("C:\\Users\\user\\source\\repos\\CroatianProject\\CroatianProject\\Packages\\");
            engine.SetSearchPaths(paths);
            engine.ExecuteFile("C:\\Users\\user\\source\\repos\\CroatianProject\\CroatianProject\\Scripts\\analysis.py", scope);            
            dynamic function = scope.GetVariable("analysis");
            IList<object> result = function(processedString);
            IList<object> paragraphs = (IList<object>)result[0]; // параграфы
            IList<object> tagged_by_paragraphs = (IList<object>)result[1]; // параграфы по словах по параграфам
            IList<object> tagged_alphabetically = (IList<object>)result[2]; // слова в алфавитном
            ExceptionMessage = "";
            foreach (var paragraph in paragraphs)
            {
               // вот тут можно обращаться к параграфам
            }
            foreach (var paragraph in tagged_by_paragraphs)
            {
                IList<object> words = (IList<object>) paragraph; // слова в параграфах
                foreach (var word in words)
                {
                    IList<object> tuple = (IList<object>) word; // кортеж для каждого слова
                    string lexeme = (string) tuple[0];
                    string PoS = (string) tuple[1];
                    //а дальше уже можно делать с ними, что угодно
                }
            }
            foreach(var unit in tagged_alphabetically)
            {
                IList<object> word = (IList<object>) unit; // список слов
                foreach(var tuple in word)
                {
                    IList<object> element = (IList<object>) tuple; // кортеж для каждого слова
                    string lexeme = (string) element[0];
                    string PoS = (string) element[1];
                    // дальше с ними можно делать, что хочется
                }
            }
        }


    }
}

