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
            var paths = engine.GetSearchPaths();
            paths.Add("C:\\Users\\user\\source\\repos\\CroatianProject\\CroatianProject\\Packages\\");
            engine.SetSearchPaths(paths);
            engine.GetSysModule().SetVariable("argv", processedString);
            engine.ExecuteFile("C:\\Users\\user\\source\\repos\\CroatianProject\\CroatianProject\\Scripts\\analysis.py");

        }


    }
}

