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
        private IHostingEnvironment _environment;

        public List<string> getFields()
        {
            List<string> existingFields = new List<string>();
            try
            {
                var directory = Path.Combine(_environment.ContentRootPath, "database", "fields");
                DirectoryInfo fieldsDirectory = new DirectoryInfo(directory);
                var fields = fieldsDirectory.GetDirectories();
                existingFields.Add("Any");
                foreach (var field in fields)
                {
                    existingFields.Add(field.Name);
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
                if (values.Length > 1)
                {
                    field.Multiply();
                }
                else
                {
                    field.MakeSingle();
                }               
                foreach (var value in values)
                {
                    field.AddValue(Regex.Replace(value, @"\r", ""));
                }
                
                var dirData = Path.Combine(_environment.ContentRootPath, "database");
                Directory.CreateDirectory(dirData);
                var dirFields = Path.Combine(dirData, "fields");
                Directory.CreateDirectory(dirFields);
                string fieldInJSON = field.Jsonize();
                var dirFieldsData = Path.Combine(dirFields, field.name);
                Directory.CreateDirectory(dirFieldsData);
                var FieldDBfile = Path.Combine(dirFieldsData, field.name + ".json");
                FileStream fs = new FileStream(FieldDBfile, FileMode.Create);
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(fieldInJSON);
                }
                FieldList = getFields();
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