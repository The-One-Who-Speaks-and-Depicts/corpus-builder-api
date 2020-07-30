using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CorpusDraftCSharp;

namespace CroatianProject.Pages.Admin
{
    public class AddDocumentModel : PageModel
    {
        private IHostingEnvironment _environment;
        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public string filePath { get; set; } = "";
        [BindProperty]
        public string textName { get; set; } = "";

        public AddDocumentModel(IHostingEnvironment environment)
        {
            _environment = environment;
            filePath = "";
            textName = "";
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var dirUploads = Path.Combine(_environment.ContentRootPath, "uploads");
            Directory.CreateDirectory(dirUploads);
            var dirData = Path.Combine(_environment.ContentRootPath, "database");
            Directory.CreateDirectory(dirData);
            try
            {
                var file = Path.Combine(dirUploads, Upload.FileName);
                var dirTexts = Path.Combine(dirData, "documents");
                Directory.CreateDirectory(dirTexts);
                DirectoryInfo directoryTextsInfo = new DirectoryInfo(dirTexts);
                Document document = new Document(directoryTextsInfo.GetFiles().Length.ToString(), textName, file, filePath);
                string documentInJSON = document.Jsonize();
                var documentDBFile = Path.Combine(dirTexts, directoryTextsInfo.GetFiles().Length.ToString() + "_" + textName + ".json");
                FileStream fs = new FileStream(documentDBFile, FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(documentInJSON);
                }
                using (var fileStream = new FileStream(file, FileMode.Create))
                {
                    await Upload.CopyToAsync(fileStream);
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
            return RedirectToPage();

        }
    }
}