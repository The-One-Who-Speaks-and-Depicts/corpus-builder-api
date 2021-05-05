using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ManuscriptsProcessor.Fields;
using ManuscriptsProcessor.Values;
using ManuscriptsProcessor.Units;

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
        public Manuscript Post(string filePath, string googleDocPath, string name)
        {
            Manuscript mockManuscript = new Manuscript();
            mockManuscript.filePath = filePath;
            mockManuscript.googleDocPath = googleDocPath;
            mockManuscript.name = name;
            return mockManuscript;
        }
    }
}
