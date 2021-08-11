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
        public Field GetOne(string name)
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
                    return fields.Where(m => m.Id == name).FirstOrDefault();
                }
            }
        }

        [HttpDelete]
        public string Delete(string id)
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
                    if (forDeletion == null)
                    {
                        return "Error: No such field in database";
                    }
                    Session.Delete(forDeletion);
                    Session.SaveChanges();
                    return "Success";
                }
            }
        }
        

        [HttpPost]
        public string Post(string name, string description, string multiplied, string restricted, string host, string possessed, string values)
        {
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
                SessionOptions options = new SessionOptions {Database = "Fields", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {
                    var existingFields =  Session.Query<Field>().ToList();
                    if (existingFields.Where(s => s.Id == name).ToList().Count > 0)
                    {
                        return "Error: Such field exists!";
                    }
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
        public string Change(string name, string description, string multiplied, string restricted, string host, string possessed, string values)
        {
            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "http://localhost:8080", },
            }.Initialize();
            RavenHelper.EnsureDatabaseExists(store);

            using (store) 
            {
                SessionOptions options = new SessionOptions {Database = "Fields", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {                    
                    Field forUpdate = Session.Load<Field>(name);
                    if (forUpdate == null)
                    {
                        return "Error: No such field in database";
                    }
                    if (!String.IsNullOrEmpty(description))
                    {
                        forUpdate.description = description;
                    }
                    if (!String.IsNullOrEmpty(multiplied))
                    {
                        forUpdate.isMultiple = multiplied == "multiplied" ? true : false;
                    }
                    if (!String.IsNullOrEmpty(restricted))
                    {
                        forUpdate.isUserFilled = restricted == "restricted" ? false : true;
                    }
                    if (!String.IsNullOrEmpty(host))
                    {
                        forUpdate.host = host;
                    }
                    if (!String.IsNullOrEmpty(possessed))
                    {
                        forUpdate.possessed = possessed;
                    }
                    if (!String.IsNullOrEmpty(values))
                    {
                        if (!forUpdate.isUserFilled)
                        {
                            List<object> newValues = new List<object>(); 
                            var addedValues = values.Split("<br />").ToList();
                            for (int i = 0; i < addedValues.Count; i++){
                                if (!String.IsNullOrEmpty(addedValues[i]))
                                {
                                    newValues.Add(addedValues[i].Trim());
                                }
                            }
                            forUpdate.values = newValues;
                        }
                    }
                    Session.SaveChanges();
                    return "Success";
                }
            }
        }

        [HttpPost]
        [Route("/api/v1/[controller]/connection")]
        public string Post (string conns)
        {
            if (String.IsNullOrEmpty(conns) || String.IsNullOrWhiteSpace(conns))
            {
                return "Error: empty query";
            }
            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "http://localhost:8080", },
            }.Initialize();
            RavenHelper.EnsureDatabaseExists(store);

            using (store) 
            {
                SessionOptions options = new SessionOptions {Database = "Fields", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {
                    var fields =  Session.Query<Field>().ToList();                    
                    var connsList = conns.Split("<br />").ToList();
                    for (int i = 0; i < connsList.Count; i++)
                    {                        
                        var joinedFields = connsList[i].Split("=>").ToList()[1].Split(',').ToList();
                        var fieldValue = connsList[i].Split("=>").ToList()[0];
                        var field = fieldValue.Split(':').ToList()[0];
                        var value = fieldValue.Split(':').ToList()[1];
                        for (int f = 0; f < fields.Count; f++)
                        {
                            var coincidenceFound = false;
                            if (field == fields[f].Id)
                            {
                                coincidenceFound = true;
                                if (fields[f].connectedFields == null)
                                {
                                    fields[f].connectedFields = new Dictionary<string, List<string>>();
                                }
                                if (!fields[f].connectedFields.ContainsKey(value))
                                {
                                    fields[f].connectedFields[value] = new List<string>();
                                }
                                for (int c = 0; c < joinedFields.Count; c++)
                                {
                                    if (!fields[f].connectedFields[value].Contains(joinedFields[c]) && fields.Any(fld => fld.Id == joinedFields[c]))
                                    {
                                        fields[f].connectedFields[value].Add(joinedFields[c]);
                                    }
                                }
                            }
                            if (coincidenceFound) break;
                        }
                    }
                    Session.SaveChanges();
                    return "Success";
                }
            }
        }

        [HttpDelete]
        [Route("/api/v1/[controller]/connection")]
        public string DeleteConnection(string conns)
        {
            if (String.IsNullOrEmpty(conns) || String.IsNullOrWhiteSpace(conns))
            {
                return "Error: empty query";
            }
            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "http://localhost:8080", },
            }.Initialize();
            RavenHelper.EnsureDatabaseExists(store);

            using (store) 
            {
                SessionOptions options = new SessionOptions {Database = "Fields", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {
                    var fields =  Session.Query<Field>().ToList();                    
                    var connsList = conns.Split(";\n").ToList();
                    for (int i = 0; i < connsList.Count; i++)
                    {                        
                        var joinedFields = connsList[i].Split("=>").ToList()[1].Split(',').ToList();
                        var fieldValue = connsList[i].Split("=>").ToList()[0];
                        var field = fieldValue.Split(':').ToList()[0];
                        var value = fieldValue.Split(':').ToList()[1];
                        for (int f = 0; f < fields.Count; f++)
                        {
                            var coincidenceFound = false;
                            if (field == fields[f].Id)
                            {
                                
                                if (fields[f].connectedFields == null)
                                {
                                    continue;
                                }
                                if (!fields[f].connectedFields.ContainsKey(value))
                                {
                                    continue;
                                }
                                for (int c = 0; c < joinedFields.Count; c++)
                                {
                                    if (fields[f].connectedFields[value].Contains(joinedFields[c]))
                                    {
                                        coincidenceFound = true;
                                        fields[f].connectedFields[value].Remove(joinedFields[c]);
                                    }
                                }
                            }
                            if (coincidenceFound) break;
                        }
                    }
                    Session.SaveChanges();
                    return "Success";
                }
            }
        }
        
    }
}
