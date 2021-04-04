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

namespace CroatianProject.Pages
{
    [BindProperties]
    public class ViewTextModel : PageModel
    {
        private IHostingEnvironment _environment;
        public string textName { get; set; }
        public string docName { get; set; }
        public List<string> textList { get; set; }
        public List<string> docList { get; set; }
        public List<string> fieldsList { get; set; } = new List<string>();
        public string textByWords { get; set; }

        public ViewTextModel(IHostingEnvironment environment)
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
        public List<string> getDocs()
        {
            List<string> existingTexts = new List<string>();
            try
            {
                var directory = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents"));
                var docs = directory.GetFiles();
                foreach (var doc in docs)
                {
                    string document = "";
                    using (StreamReader r = new StreamReader(doc.FullName))
                    {
                        var deserialized = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                        document += deserialized.documentID + "_" + deserialized.documentName + ":";
                        for (int i = 0; i < deserialized.texts.Count; i++)
                        {
                            if (i < (deserialized.texts.Count - 1))
                            {
                                document += deserialized.texts[i].textID + "_" + deserialized.texts[i].textName + "|";
                            }
                            else
                            {
                                document += deserialized.texts[i].textID + "_" + deserialized.texts[i].textName;
                            }
                        }
                    }
                    existingTexts.Add(document + "\n");
                }
            }
            catch
            {

            }
            return existingTexts;
        }

        public DirectoryInfo SearchForText(string textName, string directory)
        {
            DirectoryInfo dirTexts = new DirectoryInfo(directory);
            var searchedDirectory = dirTexts.GetDirectories().Where((dir) => dir.Name == textName).First();
            return searchedDirectory;
        }

        public void OnPost(string docName, string textName)
        {
            try
            {
                docName = docName.Split('_', 2)[0];
                textName = textName.Split('_', 2)[0];
                var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents")).GetFiles();
                Text text = new Text();
                foreach (var file in files)
                {
                    using (StreamReader r = new StreamReader(file.FullName))
                    {
                        Document analyzedDocument = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                        if (analyzedDocument.documentID == docName)
                        {
                            foreach (var t in analyzedDocument.texts)
                            {
                                if (t.textID == textName)
                                {
                                    text = t;
                                    break;
                                }
                            }
                        }
                    }
                }
                textByWords = text.Output();
            }
            catch
            {

            }
            docList = getDocs();
            fieldsList = getFields();
        }
    }
}
