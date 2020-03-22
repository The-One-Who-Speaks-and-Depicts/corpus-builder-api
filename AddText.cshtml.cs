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
       
        public string ExceptionMessage { get; private set; } = "";
        public int Rows { get; set; } = 20;
        public int Cols { get; set; } = 50;
        

        private IHostingEnvironment _environment;
        public AddTextModel(IHostingEnvironment environment)
        {
        _environment = environment;
        }


        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public string filePath { get; set; } = "";
        [BindProperty]
        public string textName { get; set; }

        public async Task OnPostAsync()
        {
            var dirUploads = Path.Combine(_environment.ContentRootPath, "uploads");
            var dirData = Path.Combine(_environment.ContentRootPath, "database");
            Directory.CreateDirectory(dirUploads);
            Directory.CreateDirectory(dirData);            
            try
            {
                var file = Path.Combine(dirUploads, Upload.FileName);
                var dirTexts = Path.Combine(dirData, "texts");
                Directory.CreateDirectory(dirTexts);
                DirectoryInfo directoryTextsInfo = new DirectoryInfo(dirTexts);
                Text text = new Text(directoryTextsInfo.GetFiles().Length.ToString(), textName, file);                
                string textInJSON = text.Jsonize();                
                var TextDBfile = Path.Combine(dirTexts, textName + ".json");
                FileStream fs = new FileStream(TextDBfile, FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(textInJSON);
                }               
                using (var fileStream = new FileStream(file, FileMode.Create))
                {
                    await Upload.CopyToAsync(fileStream);
                }
            }
            catch (Exception e)
            {
                FileStream fs = new FileStream("result1.txt", FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(e.Message);
                }
            }
            
        }

        [BindProperty]
        public string processedString { get; set; }
       

        public async Task OnPostProcess()
        {
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();
            var paths = engine.GetSearchPaths();
            var packagePath = Path.Combine(_environment.ContentRootPath, "Packages");
            paths.Add(packagePath);
            engine.SetSearchPaths(paths);
            var pythonFilePath = Path.Combine(_environment.ContentRootPath, "Scripts\\analysis.py");
            engine.ExecuteFile(pythonFilePath, scope);            
            dynamic function = scope.GetVariable("analysis");
            IList<object> result = function(processedString);
            IList<object> paragraphs = (IList<object>)result[0]; // параграфы
            IList<object> tagged_by_paragraphs = (IList<object>)result[1]; // параграфы по словах по параграфам
            IList<object> tagged_alphabetically = (IList<object>)result[2]; // слова в алфавитном
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

