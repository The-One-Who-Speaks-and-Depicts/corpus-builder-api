using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
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
    public class FieldController : ControllerBase
    {        
        

        private readonly ILogger<FieldController> _logger;

        public FieldController(ILogger<FieldController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Field> Get()
        {
            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "http://localhost:8080", },
            }.Initialize();
            RavenHelper.EnsureDatabaseExists(store, "Fields");

            using (store) 
            {
                SessionOptions options = new SessionOptions {Database = "Fields", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {
                    
                    var fields =  Session.Query<Field>().ToList(); 
                    return fields;
                }
            }
        }

        [HttpGet]
        [Route("/api/v1/[controller]/names")]
        public IEnumerable<string> GetNames()
        {
            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "http://localhost:8080", },
            }.Initialize();
            RavenHelper.EnsureDatabaseExists(store, "Fields");

            using (store) 
            {
                SessionOptions options = new SessionOptions {Database = "Fields", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {
                    
                    var fields =  Session.Query<Field>().ToList().Select(m => m.Id).ToList(); 
                    return fields;
                }
            }
        }

        [HttpGet]
        [Route("/api/v1/[controller]/one")]
        public Field GetOne(string id)
        {
            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "http://localhost:8080", },
            }.Initialize();
            RavenHelper.EnsureDatabaseExists(store, "Fields");

            using (store) 
            {
                SessionOptions options = new SessionOptions {Database = "Fields", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {
                    
                    var fields =  Session.Query<Field>().ToList(); 
                    return fields.Where(m => m.Id == id).FirstOrDefault();
                }
            }
        }

        [HttpDelete]
        public void Delete(string id)
        {
            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "http://localhost:8080", },
            }.Initialize();
            RavenHelper.EnsureDatabaseExists(store, "Fields");

            using (store) 
            {
                SessionOptions options = new SessionOptions {Database = "Fields", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {                    
                    Field forDeletion = Session.Load<Field>(id);
                    Session.Delete(forDeletion);
                    Session.SaveChanges();
                }
            }
        }

        // TODO: Change from here; add connections

        [HttpPost]
        public string Post(string name, string description, string multiplied, string restricted, string host, string possessed, string values)
        {
            // check for existing field!
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(description) || String.IsNullOrEmpty(multiplied) || String.IsNullOrEmpty(restricted) || String.IsNullOrEmpty(host) || String.IsNullOrEmpty(possessed))
            {
                return "Error: one of the required fields is empty";
            }
            if (restricted == "restricted" && String.IsNullOrEmpty(values))
            {
                return "Error: no values for user-restricted field";
            }
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
                    Field addedField = new Field();
                    addedField.Id = name;
                    addedField.description = description;
                    addedField.isMultiple = multiplied == "multiplied" ? true : false;
                    addedField.isUserFilled = restricted == "restricted" ? false : true;
                    addedField.host = host;
                    addedField.possessed = possessed;
                    if (!addedField.isUserFilled)
                    {
                        List<object> newValues = new List<object>(); 
                        var addedValues = values.Split("<br />").ToList();
                        for (int i = 0; i < addedValues.Count; i++){
                            if (!String.IsNullOrEmpty(addedValues[i]))
                            {
                                newValues.Add(addedValues[i].Trim());
                            }
                        }
                        addedField.values = newValues;
                    }
                    Session.Store(addedField);
                    Session.SaveChanges();
                    return "Success";
                }
            }
        }

        [HttpPatch]
        public void Change(string id, string filePath, string googleDocPath, string fields)
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
                    Manuscript forUpdate = Session.Load<Manuscript>(id);
                    if (!String.IsNullOrEmpty(filePath))
                    {
                        forUpdate.filePath = filePath;
                    }
                    if (!String.IsNullOrEmpty(googleDocPath))
                    {
                        forUpdate.googleDocPath = googleDocPath;
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
                        forUpdate.tagging = manuscriptFields;
                    }
                    Session.SaveChanges();
                }
            }
        }
        
    }
}
