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
        public string[] ValueTypeOptions { get; set; } = new[] { "Value of single word", "Value of letters in a single word", "Value of multiple words", "Value of letters in multiple words" };
        private IHostingEnvironment _environment;

        public string[] SetOptions()
        {
            return new[] { "Single", "Multiple" };
        }

        public string[] SetValues()
        {
            return new[] { "Value of single word", " Value of letters in single words", "Value of multiple words", "Value of letters in multiple words" };
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
                field.Activate();
                string[] values = FieldVals.Split('\n');
                if (Multiply == "Multiple")
                {
                    field.Multiply();
                }
                else
                {
                    field.MakeSingle();
                }
                field.Deletterize();
                field.MakeSingleWorded();
                if (Type == SetValues()[1] || Type == SetValues()[3])
                {
                    field.Letterize();
                }
                if (Type == SetValues()[2] || Type == SetValues()[3])
                {
                    field.MakeMWE();
                }

                foreach (var value in values)
                {
                    field.AddValue(Regex.Replace(value, @"\r", ""));
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
            }
            catch (Exception e)
            {
                FileStream fs = new FileStream("result1.txt", FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(e.Message + FieldVals.Split('\n').Length);
                }
            }
            
        }


    }
}