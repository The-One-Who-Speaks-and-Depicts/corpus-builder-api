using ManuscriptsProcessor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using ManuscriptsProcessor.Units;
using ManuscriptsProcessor.Values;

namespace CroatianProject.Pages
{
    [BindProperties]
    public class ViewTextModel : PageModel
    {
        private IWebHostEnvironment _environment;
        public string textName { get; set; }
        public string docName { get; set; }
        public List<string> textList { get; set; }
        public List<string> scriptsList
        {
            get
            {
                return MyExtensions.GetManuscripts(Path.Combine(_environment.ContentRootPath, "database", "manuscripts"));
            }
        }
        public List<string> fieldsList
        {
            get
            {
                return MyExtensions.GetFields(Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields"));
            }
        }
        public string textByWords { get; set; }

        public ViewTextModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public DirectoryInfo SearchForText(string textName, string directory)
        {
            DirectoryInfo dirTexts = new DirectoryInfo(directory);
            var searchedDirectory = dirTexts.GetDirectories().Where((dir) => dir.Name == textName).First();
            return searchedDirectory;
        }

        public void OnPost(string docName, string textName)
        {
            docName = docName.Split('_', 2)[0];
            textName = textName.Split('_', 2)[0];
            var files = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "database", "manuscripts")).GetFiles();
            if (files.Length < 1) return;
            var section = new Section();
            foreach (var file in files)
            {
                using (StreamReader r = new StreamReader(file.FullName))
                {
                    var analyzedManuscript = JsonConvert.DeserializeObject<Manuscript>(r.ReadToEnd());
                    if (analyzedManuscript.subunits is null || analyzedManuscript.subunits.Count < 1) continue;
                    if (analyzedManuscript.Id == docName)
                    {
                        foreach (var s in analyzedManuscript.subunits)
                        {
                            if (s.Id.Split('_')[1] == textName)
                            {
                                section = s;
                                break;
                            }
                        }
                    }
                }
            }
            textByWords = section.Output();
        }
    }
}
