using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CorpusDraftCSharp;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CroatianProject.Pages.Admin
{
    public class AddFieldModel : PageModel
    {
        [BindProperty]
        public List<string> FieldList { get; set; }
        [BindProperty]
        public string FieldName { get; set; }
        [BindProperty]
        public string FieldDesc { get; set; }
        [BindProperty]
        public string FieldVals { get; set; }
        [BindProperty]
        public string Multiply { get; set; }
        [BindProperty]
        public string[] MultiplyOptions { get; set; } = new[] { "Single", "Multiple"};
        [BindProperty]
        public string Type { get; set; }
        [BindProperty]
        public string[] ValueTypeOptions { get; set; } = new[] { "Value of document", "Value of text", "Value of text unit", "Value of single word", "Value of letter" };
        [BindProperty]
        public string[] UserFilledOptions { get; set; } = new[] { "User-restricted", "User-filled" };
        [BindProperty]
        public string Filled { get; set; }
        [BindProperty]
        public string connections { get; set; }

        private IHostingEnvironment _environment;

        public string[] SetOptions()
        {
            return new[] { "Single", "Multiple" };
        }

        public string[] SetValues()
        {
            return new[] { "Value of document", "Value of text", "Value of text unit", "Value of single token", "Value of letter" };
        }

        public string[] SetFullfillment()
        {
            return new[] { "User-restricted", "User-filled" };
        }


        public List<string> getFields()
        {
            List<string> existingFields = new List<string>();
            try
            {
                var directory = Path.Combine(_environment.ContentRootPath, "wwwroot", "database", "fields");
                DirectoryInfo fieldsDirectory = new DirectoryInfo(directory);
                var fields = fieldsDirectory.GetFiles();
                existingFields.Add("Any");
                foreach (var field in fields)
                {
                    existingFields.Add(field.Name.Split(".json")[0]);
                }
            }
            catch
            {

            }            
            return existingFields;
        }
        public AddFieldModel(IHostingEnvironment environment)
        {
            _environment = environment;
            try
            {
                FieldList = getFields();
                MultiplyOptions = SetOptions();
                ValueTypeOptions = SetValues();
                UserFilledOptions = SetFullfillment();
            }
            catch
            {
                Redirect("./Error");
            }
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
                if (Type == SetValues()[0])
                {
                    field.changeType("Document");
                }
                else if (Type == SetValues()[1])
                {
                    field.changeType("Text");
                }
                else if (Type == SetValues()[2])
                {
                    field.changeType("Clause");
                }
                else if (Type == SetValues()[3])
                {
                    field.changeType("Realization");
                }
                else if (Type == SetValues()[4])
                {
                    field.changeType("Grapheme");
                }
                field.MakeUserFilled();
                if (Filled == SetFullfillment()[0])
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
                var FieldDBfile = Path.Combine(dirFields, field.name + ".json");
                FileStream fs = new FileStream(FieldDBfile, FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(fieldInJSON);
                }
                FieldList = getFields();
                MultiplyOptions = SetOptions();
                ValueTypeOptions = SetValues();
                UserFilledOptions = SetFullfillment();
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
            FieldList = getFields();
            MultiplyOptions = SetOptions();
            ValueTypeOptions = SetValues();
            UserFilledOptions = SetFullfillment();
        }


    }
}