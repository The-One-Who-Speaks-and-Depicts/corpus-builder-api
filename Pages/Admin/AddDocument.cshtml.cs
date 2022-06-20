using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuscriptsProcessor.Units;
using ManuscriptsProcessor.Values;
using ManuscriptsProcessor;

namespace CroatianProject.Pages.Admin
{
    public class AddDocumentModel : PageModel
    {
        private IWebHostEnvironment _environment;
        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public string filePath { get; set; } = "";
        [BindProperty]
        public string textName { get; set; } = "";
        [BindProperty]
        public string connections { get; set; }
        [BindProperty]
        public List<string> FieldList
        {
            get
            {
                return MyExtensions.GetFields(Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields"));
            }
        }



        public AddDocumentModel(IWebHostEnvironment environment)
        {
            _environment = environment;
            filePath = "";
            textName = "";
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var dirUploads = Path.Combine(_environment.ContentRootPath, "uploads");
            Directory.CreateDirectory(dirUploads);
            var dirData = Path.Combine(_environment.ContentRootPath, "database");
            Directory.CreateDirectory(dirData);
            try
            {
                var file = Path.Combine(dirUploads, Upload.FileName);
                var dirTexts = Path.Combine(dirData, "manuscripts");
                Directory.CreateDirectory(dirTexts);
                DirectoryInfo directoryTextsInfo = new DirectoryInfo(dirTexts);
                var manuscript = new Manuscript(directoryTextsInfo.GetFiles().Length.ToString(), textName, file, filePath);
                manuscript.tagging = new List<Dictionary<string, List<Value>>>();
                manuscript.tagging.Add(new Dictionary<string, List<Value>>());
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
                            manuscript.tagging[0].Add(key, typedValues);
                        }
                    }
                }
                string manuscriptInJSON = manuscript.Jsonize();
                var manuscriptDBFile = Path.Combine(dirTexts, directoryTextsInfo.GetFiles().Length.ToString() + "_" + textName + ".json");
                FileStream fs = new FileStream(manuscriptDBFile, FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(manuscriptInJSON);
                }
                using (var fileStream = new FileStream(file, FileMode.Create))
                {
                    await Upload.CopyToAsync(fileStream);
                }
            }
            catch (Exception e)
            {
                // implement logging
                FileStream fs = new FileStream("result1.txt", FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(e.StackTrace);
                }
            }
            return RedirectToPage();

        }
    }
}
