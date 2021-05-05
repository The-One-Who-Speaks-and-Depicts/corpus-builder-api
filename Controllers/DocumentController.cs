using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using ManuscriptsProcessor.Fields;
using ManuscriptsProcessor.Values;
using ManuscriptsProcessor.Units;
using corpus_builder_api.ServiceFunctions;

namespace corpus_builder_api.Controllers
{   
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {        
        

        private readonly ILogger<DocumentController> _logger;

        public DocumentController(ILogger<DocumentController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Manuscript Get()
        {
            Manuscript mockManuscript = new Manuscript();
            return mockManuscript;
        }

        [HttpPost]
        public void Post(string filePath, string googleDocPath, string name, string fields)
        {
            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "http://localhost:8080", },
            }.Initialize();
            RavenHelper.EnsureDatabaseExists(store);

            using (store) 
            {
                SessionOptions options = new SessionOptions {Database = "Manuscripts", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {
                    Manuscript addedManuscript = new Manuscript();
                    addedManuscript.filePath = filePath;
                    addedManuscript.googleDocPath = googleDocPath;
                    addedManuscript.name = name;
                    if (fields != "")
                    {
                        List<Dictionary<string, List<Value>>> manuscriptFields = new List<Dictionary<string, List<Value>>>();
                        List<string> splitFields = fields.Split(';').ToList();
                        Dictionary<string, List<Value>> fieldsToAdd = new Dictionary<string, List<Value>>();
                        for (int i = 0; i < splitFields.Count; i++)
                        {
                            if (splitFields[i] != "")
                            {
                                List<string> fieldAndValues = splitFields[i].Split(":").ToList();
                                string field = fieldAndValues[0];
                                List<string> stringValues = fieldAndValues[1].Split("|").ToList();
                                List<Value> values = new List<Value>();
                                for (int j = 0; j < stringValues.Count; j++)
                                {
                                    if (stringValues[j] != "")
                                    {
                                        values.Add(new Value {name = stringValues[j], connectedUnits = new List<Unit>()});
                                    }
                                }
                                fieldsToAdd[field] = values;

                            }
                        }
                        manuscriptFields.Add(fieldsToAdd);
                        addedManuscript.tagging = manuscriptFields;
                    }                    
                    Session.Store(addedManuscript);
                    Session.SaveChanges();
                }
            }
        }
    }
}
