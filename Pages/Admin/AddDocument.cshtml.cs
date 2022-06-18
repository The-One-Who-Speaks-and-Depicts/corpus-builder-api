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
        private IWebHostEnvironment _environment;
        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public string filePath { get; set; } = "";
        [BindProperty]
        public string textName { get; set; } = "";
        [BindProperty]
        public string connections { get; set; }
        [BindProperty]
        public List<string> FieldList { get; set; }

        public List<string> getFields()
        {
            List<string> existingFields = new List<string>();
            try
            {
                var directory = Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields");
                DirectoryInfo fieldsDirectory = new DirectoryInfo(directory);
                var fields = fieldsDirectory.GetFiles();
                existingFields.Add("Any");
                foreach (var field in fields)
                {
                    existingFields.Add(field.Name.Split(".json")[0]);
                }
            }
            catch
            {

            }
            return existingFields;
        }

        public AddDocumentModel(IWebHostEnvironment environment)
        {
            _environment = environment;
            filePath = "";
            textName = "";
            try
            {
                FieldList = getFields();
            }
            catch
            {
                Redirect("./Error");
            }
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
                document.documentMetaData = new List<Dictionary<string, List<Value>>>();
                document.documentMetaData.Add(new Dictionary<string, List<Value>>());
                if (!String.IsNullOrEmpty(connections) && !String.IsNullOrWhiteSpace(connections))
                {
                    string[] tags = connections.Split("\n");
                    foreach (string tag in tags)
                    {
                        if (!String.IsNullOrWhiteSpace(tag) && !String.IsNullOrEmpty(tag))
                        {
                            string key = tag.Split("=>")[0];
                            string[] stringValues = tag.Split("=>")[1].Split(';');
                            List<Value> typedValues = new List<Value>();
                            foreach (string stringValue in stringValues)
                            {
                                if (!String.IsNullOrEmpty(stringValue) && !String.IsNullOrWhiteSpace(stringValue))
                                {
                                    typedValues.Add(new Value(stringValue));
                                }
                            }
                            document.documentMetaData[0].Add(key, typedValues);
                        }
                    }
                }
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
                    w.Write(e.StackTrace);
                }
            }
            FieldList = getFields();
            return RedirectToPage();

        }
    }
}
