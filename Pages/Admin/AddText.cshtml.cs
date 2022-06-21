using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuscriptsProcessor.Values;
using ManuscriptsProcessor.Units;
using Newtonsoft.Json;
using ManuscriptsProcessor;

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
        public static Manuscript? analyzedManuscript { get; set; }
        [BindProperty]
        public string manuscriptPicked { get; set; }
        [BindProperty]
        public List<string> manuscripts
        {
            get
            {
                return MyExtensions.GetManuscripts(Path.Combine(_environment.ContentRootPath, "database", "manuscripts"));
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
                return MyExtensions.GetManuscripts(Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields"));
            }
        }

        public void OnGet(string manuscriptPicked)
        {
            if (!String.IsNullOrEmpty(manuscriptPicked) && !String.IsNullOrWhiteSpace(manuscriptPicked))
            {
                var manuscriptID = manuscriptPicked.Split('[')[1].Split(']')[0];
                var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "manuscripts")).GetFiles();
                var requiredDocFile = files
                .Where(x => x.Name.Split('_')[0] == manuscriptID)
                .Single();
                using (var r = new StreamReader(requiredDocFile.FullName))
                {
                    var requiredScript = JsonConvert.DeserializeObject<Manuscript>(r.ReadToEnd());
                    googleDocPath = requiredScript.googleDocPath;
                    analyzedManuscript = requiredScript;
                }
            }
        }


        public IActionResult OnPostProcess()
        {
            var manuscriptSectionsCount = analyzedManuscript is null ? "0" : analyzedManuscript.subunits.Count.ToString();
            var addedSection = new Section(analyzedManuscript, manuscriptSectionsCount, textName);
            addedSection.tagging = new List<Dictionary<string, List<Value>>>();
            addedSection.tagging.Add(new Dictionary<string, List<Value>>());
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
                        addedSection.tagging[0].Add(key, typedValues);
                    }
                }
            }
            var clauses = processedString.Split(new char[] {'\n', '\r'}).Where(x => x != "").ToList();
            var wordsByClauses = new List<DecomposedSegment>();
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
                wordsByClauses.Add(new DecomposedSegment
                {
                    clause = clause,
                    tokens = preparedTokens
                });
            }
            addedSection.subunits = new List<Segment>();
            for (int i = 0; i < wordsByClauses.Count; i++)
            {
                var addedSegment = new Segment(addedSection, i.ToString(), wordsByClauses[i].clause);
                var addedClause = new Clause(addedSegment, i.ToString(), wordsByClauses[i].clause);
                addedClause.subunits = new List<Token>();
                var realizations = wordsByClauses[i].tokens;
                for (int j = 0; j < realizations.Count; j++)
                {
                    var addedRealization = new Token(addedClause, j.ToString(), realizations[j].lexemeOne, realizations[j].lexemeTwo);
                    addedRealization.subunits = new List<Grapheme>();
                    for (int k = 0; k < realizations[j].graphemes.Count; k++)
                    {
                        addedRealization.subunits.Add(new Grapheme(addedRealization, k.ToString(), realizations[j].graphemes[k]));
                    }
                    addedClause.subunits.Add(addedRealization);
                }
                addedSegment.subunits = new List<Clause>();
                addedSection.subunits.Add(addedSegment);
            }
            analyzedManuscript.subunits = new List<Section>();
            analyzedManuscript.subunits.Add(addedSection);
            using (StreamWriter w = new StreamWriter(Path.Combine(_environment.ContentRootPath, "database", "manuscripts", analyzedManuscript.Id + "_" + analyzedManuscript.text + ".json")))
            {
                w.Write(analyzedManuscript.Jsonize());
            }
            return RedirectToPage();

        }



    }
}
