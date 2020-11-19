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
            try
            {
                FieldList = getFields();
            }
            catch
            {
                Redirect("./Error");
            }
        }


        [BindProperty]
        public string googleDocPath { get; set; } = "";
        [BindProperty]
        public static Document analyzedDocument { get; set; } = new Document();
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
        public bool decapitalization { get; set; }
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


        public IActionResult OnPostProcess()
        {            
            try
            {                
                ScriptEngine engine = Python.CreateEngine();
                ScriptScope scope = engine.CreateScope();
                var paths = engine.GetSearchPaths();
                engine.SetSearchPaths(paths);
                var pythonFilePath = Path.Combine(_environment.ContentRootPath, "Scripts", "analysis.py");
                engine.ExecuteFile(pythonFilePath, scope);
                dynamic function = scope.GetVariable("analysis");
                IList<object> result = function(processedString, stopSymbols, decapitalization.ToString());
                Text addedText = new Text(analyzedDocument, analyzedDocument.texts.Count.ToString(), textName);
                addedText.textMetaData = new List<Dictionary<string, List<Value>>>();
                addedText.textMetaData.Add(new Dictionary<string, List<Value>>());
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
                            addedText.textMetaData[0].Add(key, typedValues);
                        }
                    }
                }
                for (int i = 0; i < result.Count; i++)
                {
                    var clauseFullData = (IList<object>)result[i];
                    Clause addedClause = new Clause(addedText, i.ToString(), (string)clauseFullData[0]);
                    var realizations = (IList<object>) clauseFullData[1];
                    for (int j = 0; j < realizations.Count; j++)
                    {                        
                        var realizationFullData = (IList<object>) realizations[j];
                        Realization addedRealization = new Realization(addedClause, j.ToString(), (string)realizationFullData[0], (string)realizationFullData[1]);
                        var graphemes = (IList<object>) realizationFullData[2];
                        for (int k = 0; k < graphemes.Count; k++)
                        {
                            addedRealization.letters.Add(new Grapheme(addedRealization, k.ToString(), (string) graphemes[k]));
                        }
                        addedClause.realizations.Add(addedRealization);
                    }
                    addedText.clauses.Add(addedClause);
                }
                analyzedDocument.texts.Add(addedText);
                using (StreamWriter w = new StreamWriter(Path.Combine(_environment.ContentRootPath, "database", "documents", analyzedDocument.documentID + "_" + analyzedDocument.documentName + ".json")))
                {
                    w.Write(analyzedDocument.Jsonize());
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
