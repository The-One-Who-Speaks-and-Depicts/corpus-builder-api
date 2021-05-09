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

        [HttpGet]
        public bool Login(string login, string password)
        {
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
                    
                    var users =  Session.Query<User>().ToList(); 
                    var user = users.Where(m => m.Id == login).FirstOrDefault();
                    return user.CheckPassword(password);
                }
            }
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
                    User storagedUser = new User(login, password);
                    Session.Store(storagedUser);                  
                    Session.SaveChanges();
                    return true;
                }
            }        	
        }
	}


}