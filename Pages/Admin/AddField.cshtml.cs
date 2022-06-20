using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuscriptsProcessor.Fields;
using ManuscriptsProcessor;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CroatianProject.Pages.Admin
{
    public class AddFieldModel : PageModel
    {
        [BindProperty]
        public List<string> FieldList
        {
            get
            {
                return MyExtensions.GetFields(Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields"));
            }
        }
        [BindProperty]
        public string FieldName { get; set; }
        [BindProperty]
        public string FieldDesc { get; set; }
        [BindProperty]
        public string FieldVals { get; set; }
        [BindProperty]
        public string Multiply { get; set; }
        [BindProperty]
        public string[] MultiplyOptions
        {
            get
            {
                return new[] { "Single", "Multiple" };
            }
        }
        [BindProperty]
        public string type { get; set; }
        [BindProperty]
        public string[] ValueTypeOptions
        {
            get
            {
                return new[] { "Manuscript", "Section",  "Segment", "Clause", "Token", "Grapheme" };
            }
        }
        [BindProperty]
        public string[] UserFilledOptions
        {
            get
            {
                return new[] { "User-restricted", "User-filled" };
            }
        }
        [BindProperty]
        public string Filled { get; set; }
        [BindProperty]
        public string connections { get; set; }

        private IWebHostEnvironment _environment;
        public AddFieldModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public void OnPostAdd()
        {
            try
            {
                Field field = new Field(FieldName, FieldDesc);
                if (Multiply == "Multiple")
                {
                    field.Multiply();
                }
                else
                {
                    field.MakeSingle();
                }
                field.changeType(type);
                field.MakeUserFilled();
                if (Filled == "User-restricted")
                {
                    field.MakeRestricted();
                    string[] values = FieldVals.Split('\n');
                    foreach (var value in values)
                    {
                        field.AddValue(Regex.Replace(value, @"\r", ""));
                    }
                }
                var dirData = Path.Combine(_environment.ContentRootPath, "wwwroot", "database");
                Directory.CreateDirectory(dirData);
                var dirFields = Path.Combine(dirData, "fields");
                Directory.CreateDirectory(dirFields);
                string fieldInJSON = field.Jsonize();
                var FieldDBfile = Path.Combine(dirFields, Regex.Replace(field.name, @"[*""><:/\\|?\.,]", "")  + ".json");
                FileStream fs = new FileStream(FieldDBfile, FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(fieldInJSON);
                }
            }
            catch (Exception e)
            {
                FileStream fs = new FileStream("result1.txt", FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(e.Message);
                }
            }

        }

        public void OnPostConnect()
        {
            var addedConnections = from connection in connections.Split('\n')
                                   select connection.Trim();
            var fieldFiles = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields")).GetFiles();
            List<Field> fields = new List<Field>();
            foreach (var fieldFile in fieldFiles)
            {
                using (StreamReader r = new StreamReader(fieldFile.FullName))
                {
                    fields.Add(JsonConvert.DeserializeObject<Field>(r.ReadToEnd()));
                }
            }
            foreach (var connection in addedConnections)
            {
                string mother = connection.Split("->")[0];
                string[] children = connection.Split("->")[1].Split(',');
                string name = mother.Split(':')[0];
                string value = mother.Split(':')[1];
                foreach (var field in fields)
                {
                    if (field.name == name)
                    {
                        if (field.connectedFields != null)
                        {
                            if (field.connectedFields.Keys.Contains(value))
                            {
                                foreach (var child in children)
                                {
                                    if (!field.connectedFields[value].Contains(child))
                                    {
                                        var motherType = field.type;
                                        foreach (var potentialChild in fields)
                                        {
                                            if (potentialChild.name == child && potentialChild.type == motherType)
                                            {
                                                field.connectedFields[value].Add(child);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                field.connectedFields[value] = new List<string>();
                                foreach (var child in children)
                                {
                                    var motherType = field.type;
                                    foreach (var potentialChild in fields)
                                    {
                                        if (potentialChild.name == child && potentialChild.type == motherType)
                                        {
                                            field.connectedFields[value].Add(child);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            field.connectedFields = new Dictionary<string, List<string>>();
                            field.connectedFields[value] = new List<string>();
                            foreach (var child in children)
                            {
                                var motherType = field.type;
                                foreach (var potentialChild in fields)
                                {
                                    if (potentialChild.name == child && potentialChild.type == motherType)
                                    {
                                        field.connectedFields[value].Add(child);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var field in fields)
            {
                using (StreamWriter w = new StreamWriter(Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields", field.name + ".json")))
                {
                    w.Write(field.Jsonize());
                }
            }
        }


    }
}
