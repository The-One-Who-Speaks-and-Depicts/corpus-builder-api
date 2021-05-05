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
    [Route("/api/v1/[controller]")]
    public class DocumentController : ControllerBase
    {        
        

        private readonly ILogger<DocumentController> _logger;

        public DocumentController(ILogger<DocumentController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Manuscript> Get()
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
                    
                    var manuscripts =  Session.Query<Manuscript>().ToList(); 
                    return manuscripts;
                }
            }
        }

        [HttpGet]
        [Route("/api/v1/[controller]/{name}")]
        public Manuscript Get(string name)
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
                    
                    var manuscripts =  Session.Query<Manuscript>().ToList(); 
                    return manuscripts.Where(m => m.name == name).FirstOrDefault();
                }
            }
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
                    if (!String.IsNullOrEmpty(filePath))
                    {
                        addedManuscript.filePath = filePath;
                    }
                    if (!String.IsNullOrEmpty(googleDocPath))
                    {
                        addedManuscript.googleDocPath = googleDocPath;
                    }
                    if (!String.IsNullOrEmpty(name))
                    {
                        addedManuscript.name = name;
                    }
                    if (!String.IsNullOrEmpty(fields))
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
