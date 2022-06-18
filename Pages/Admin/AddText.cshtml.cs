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
using Newtonsoft.Json;

namespace CroatianProject.Pages
{
    public class AddTextModel : PageModel
    {

        public int Rows { get; set; } = 20;
        public int Cols { get; set; } = 50;


        private IWebHostEnvironment _environment;
        public AddTextModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }


        [BindProperty]
        public string googleDocPath { get; set; } = "";
        [BindProperty]
        public static Document analyzedDocument { get; set; } = new Document();
        [BindProperty]
        public string documentPicked { get; set; }
        [BindProperty]
        public List<string> documents
        {
            get
            {
                var deserializedDocuments = new List<string>();
                var pathToDocuments = Path.Combine(_environment.ContentRootPath, "database", "documents");
                Directory.CreateDirectory(pathToDocuments);
                var docDirectory = new DirectoryInfo(pathToDocuments);
                var jsonedDocuments = docDirectory.GetFiles();
                if (jsonedDocuments.Length < 1)
                {
                    return deserializedDocuments;
                }
                return jsonedDocuments
                /* Select(x => (x) => {

                }). */
                .Select(file => file.FullName)
                .Select(name => {
                    using (StreamReader r = new StreamReader(name))
                    {
                        return JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                    }
                })
                .Select(document => document.documentName + "[" + document.documentID + "]")
                .ToList();
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
        public List<string> FieldList
        {
            get
            {
                List<string> existingFields = new List<string>();
                var pathToFields = Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields");
                Directory.CreateDirectory(Path.Combine(pathToFields));
                DirectoryInfo fieldsDirectory = new DirectoryInfo(pathToFields);
                var fields = fieldsDirectory.GetFiles();
                existingFields.Add("Any");
                Console.WriteLine(fields.Length.ToString());
                if (fields.Length < 1)
                {
                    return existingFields;
                }
                foreach (var field in fields)
                {
                    existingFields.Add(field.Name.Split(".json")[0]);
                }
                return existingFields;
            }
        }

        public void OnGet(string documentPicked)
        {
            if (!String.IsNullOrEmpty(documentPicked) && !String.IsNullOrWhiteSpace(documentPicked))
            {
                var documentID = documentPicked.Split('[')[1].Split(']')[0];
                var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "documents")).GetFiles();
                var requiredDocFile = files
                .Where(x => x.Name.Split('_')[0] == documentID)
                .Single();
                using (var r = new StreamReader(requiredDocFile.FullName))
                {
                    var requiredDoc = JsonConvert.DeserializeObject<Document>(r.ReadToEnd());
                    googleDocPath = requiredDoc.googleDocPath;
                    analyzedDocument = requiredDoc;
                }
            }
        }


        public IActionResult OnPostProcess()
        {

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
            var clauses = processedString.Split(new char[] {'\n', '\r'}).Where(x => x != "").ToList();
            var wordsByClauses = new List<DecomposedClause>();
            foreach (var clause in clauses)
            {
                var tokens = clause.Split(' ').Where(x => x != "").ToList();
                var preparedTokens = new List<DecomposedToken>();
                foreach (var token in tokens)
                {
                    var lexemeTwo = token;
                    if (!String.IsNullOrEmpty(stopSymbols) && !String.IsNullOrWhiteSpace(stopSymbols))
                    {
                        lexemeTwo = String.Join("", token.ToCharArray().Where(x => !stopSymbols.Contains(x)).ToList());
                    }
                    if (decapitalization)
                    {
                        lexemeTwo = lexemeTwo.ToLower();
                    }
                    var graphemes = token.ToCharArray().Select(x => x.ToString()).ToList();
                    preparedTokens.Add(new DecomposedToken
                    {
                        lexemeOne = token,
                        lexemeTwo = lexemeTwo,
                        graphemes = graphemes
                    });
                }
                wordsByClauses.Add(new DecomposedClause
                {
                    clause = clause,
                    tokens = preparedTokens
                });
            }
            for (int i = 0; i < wordsByClauses.Count; i++)
            {
                var addedClause = new Clause(addedText, i.ToString(), wordsByClauses[i].clause);
                var realizations = wordsByClauses[i].tokens;
                for (int j = 0; j < realizations.Count; j++)
                {
                    var addedRealization = new Realization(addedClause, j.ToString(), realizations[j].lexemeOne, realizations[j].lexemeTwo);
                    for (int k = 0; k < realizations[j].graphemes.Count; k++)
                    {
                        addedRealization.letters.Add(new Grapheme(addedRealization, k.ToString(), realizations[j].graphemes[k]));
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
            return RedirectToPage();

        }



    }
}
