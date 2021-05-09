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
using corpus_builder_api.ServiceFunctions;
using corpus_builder_api.Models;

namespace corpus_builder_api.Controllers
{
	[ApiController]
	[Route("/api/v1/[controller]")]

	public class UserController : ControllerBase 
	{
		private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public bool Register(string login, string password, string repeatedPassword)
        {
        	if (password != repeatedPassword)
        	{
        		return false;
        	}
        	IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "http://localhost:8080", },
            }.Initialize();
            RavenHelper.EnsureDatabaseExists(store, "Users");
            using (store) 
            {
                SessionOptions options = new SessionOptions {Database = "Users", TransactionMode = TransactionMode.ClusterWide};
                using (IDocumentSession Session = store.OpenSession(options))
                {
                    
                    Session.Store(new User(login, password));                  
                    Session.SaveChanges();
                }
            }
        	return true;
        }
	}


}