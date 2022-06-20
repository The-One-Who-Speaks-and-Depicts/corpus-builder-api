using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using ExcelDataReader;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ManuscriptsProcessor.Units;
using ManuscriptsProcessor.Values;

namespace ManuscriptsProcessor
{

        public static class MyExtensions
        {

        //new method of DataTable class; called on dataTable object, takes first rowNum rows (by default, 5),
        //prints them into console if print is true (by default, false), and returns new DataTable object
        //in case something is wrong, exception is thrown and empty DataTable is returned
            public static DataTable Head (this DataTable dataTable, int rowNum = 5, bool print = false)
            {
            DataTable head = new DataTable();

            try
            {
                for (int i = 0; i < rowNum; i++)
                {
                    head.Rows.Add();
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        if (head.Columns.Count <= dataTable.Columns.Count)
                        {
                            head.Columns.Add();
                        }
                        head.Rows[i][j] = dataTable.Rows[i][j];
                    }
                }

                if (print)
                {
                    for (int i = 0; i < rowNum; i++)
                    {
                        for (int j = 0; j < head.Columns.Count; j++)
                        {
                            Console.Write(head.Rows[i][j] + " ");
                        }
                        Console.WriteLine();
                    }
                }
                return head;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception is caught! {0}", e.ToString());
                return head;
            }
            }



        public static bool MaskMatches (this Token realization, string query)
        {
            string regexQuery = "^" + String.Join(".{1,}", query.Split('*')) + "$";
            Debug.WriteLine(regexQuery);
            if (Regex.IsMatch(realization.text, regexQuery) || Regex.IsMatch(realization.lexemeView, regexQuery))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ConsoleSettings()
        {
            // there should be another class, as well as for two methods above
            //Console should use Consolas font

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); //use registerprovider to say
            Console.OutputEncoding = Encoding.UTF8; //use Unicode encoding
        }
        public static int KeyCreation(string valueCreated)
        {
            try
            {
                return int.Parse(InsertString("number (0...n) of column with " + valueCreated));
            }
            catch (FormatException ex)
            {
                Console.WriteLine("You shuuld enter a number. Exception os {0}", ex.ToString());
                return KeyCreation(valueCreated);
            }
        }

        public static string ValueCreation(int key, Dictionary<string, DataTable> spreadsheets, string valueCreated)
        {
            string valueName = InsertString("name of sheet with " + valueCreated);
            try
            {
                List<string> columnGiven = RowIntoList(key, spreadsheets[valueName]);
                return valueName;
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("Name of sheet is incorrect. Try again. Your exception is {0}", ex);
                return ValueCreation(key, spreadsheets, valueCreated);
            }
            catch (IndexOutOfRangeException)
            {
                return valueName;
            }
        }

        public static int KeyCheck(int key, string value, Dictionary<string, DataTable> spreadsheets, string valueCreated)
        {
            try
            {
                RowIntoList(key, spreadsheets[value]);
                return key;
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine("Key is incorrect. Try again. Exception is {0}", ex);
                return KeyCheck(KeyCreation(valueCreated), value, spreadsheets, valueCreated);
            }
        }

        public static string InsertString(string entityInserted)
        {
            Console.Write("Insert ");
            Console.WriteLine(entityInserted);
            return Console.ReadLine();
        }
        public static bool NameCheck(string name, Dictionary<string, DataTable> spreadsheets)
        {
            foreach (string s in spreadsheets.Keys)
            {
                if (name == s)
                {
                    return false;
                }
            }
            return true;
        }

        public static string Naming(DataTable dataTable, Dictionary<string, DataTable> spreadsheets)
        {
            Console.WriteLine("The table is: ");
            dataTable.Head(1, true);
            string name = InsertString("name of table");
            if (String.IsNullOrEmpty(name) || String.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("The name of the table is void. Give the unique name, consisting of letters or numbers");
                return Naming(dataTable, spreadsheets);
            }
            else
            {
                if (!NameCheck(name, spreadsheets))
                {
                    Console.WriteLine("There is another table with such name. Give the unique name, consisting of letters or numbers");
                    return Naming(dataTable, spreadsheets);
                }
                else
                {
                    return name;
                }
            }
        }

        public static List<string> RowIntoList(int rowNumber, DataTable sourceSpreadsheet)
        {
            List<string> preparedlList = new List<string>(); //creating our list
            foreach (DataRow row in sourceSpreadsheet.Rows) //converting row into a string of list
            {
                string currentValue = row[rowNumber].ToString();
                preparedlList.Add(currentValue);
            }
            return preparedlList; //returning our list to the body of a program
        }

        public static List<string> SingleFieldIntoList(Dictionary<string, DataTable> spreadsheets, string value, string upperLevelValue, string thisLevelID, string textsheetName,
            string colName, int colNumber)
        {
            DataTable textSheet = spreadsheets[textsheetName];
            string expression = colName + " = " + "'" + thisLevelID + "'";
            DataRow[] foundRows;
            foundRows = textSheet.Select(expression);
            List<string> preparedlList = new List<string>(); //creating our list
            foreach (DataRow row in foundRows) //converting row into a string of list
            {
                string currentValue = row[colNumber].ToString();
                preparedlList.Add(currentValue);
            }
            return preparedlList; //returning our list to the body of a program
        }

        public static List<string> SingleFieldIntoListTwoCols(Dictionary<string, DataTable> spreadsheets, string thisLevelID, string upperLevelID, string textsheetName,
            string colName, string colTwoName, int colNumber)
        {
            DataTable textSheet = spreadsheets[textsheetName];
            string expression = colName + " = " + "'" + thisLevelID + "'" + " and " + colTwoName + " = " + "'" + upperLevelID + "'";
            DataRow[] foundRows;
            foundRows = textSheet.Select(expression);
            List<string> preparedlList = new List<string>(); //creating our list
            foreach (DataRow row in foundRows) //converting row into a string of list
            {
                string currentValue = row[colNumber].ToString();
                preparedlList.Add(currentValue);
            }
            return preparedlList; //returning our list to the body of a program
        }

        public static Dictionary<string, string> CreateAdditionalFields(string ID, Dictionary<string, DataTable> spreadsheets, string thisUnitValue, string fieldKey,
            string textSheetName, string colName, int colNumber)
        {
            Dictionary<string, string> fieldsRetrieved = new Dictionary<string, string>();
                    List<string> listPrepared = SingleFieldIntoList(spreadsheets, fieldKey, thisUnitValue, ID, textSheetName, colName, colNumber);
                    fieldsRetrieved[fieldKey] = listPrepared[0];
            return fieldsRetrieved;
        }

        public static Dictionary<string, string> CreateAdditionalFieldsTwoCols(string ID, string upperID, Dictionary<string, DataTable> spreadsheets, string fieldKey,
            string textSheetName, string colName, string colTwoName, int colNumber)
        {
            Dictionary<string, string> fieldsRetrieved = new Dictionary<string, string>();
            List<string> listPrepared = SingleFieldIntoListTwoCols(spreadsheets, ID, upperID, textSheetName, colName, colTwoName, colNumber);
            fieldsRetrieved[fieldKey] = listPrepared[0];
            return fieldsRetrieved;
        }

        public static List<string> ColIntoDistinctList(Dictionary<string, DataTable> spreadsheets, string value)
        {
            int textColumnNumber = KeyCreation(value);
            string textSpreadsheet = ValueCreation(textColumnNumber, spreadsheets, value);
            textColumnNumber = KeyCheck(textColumnNumber, textSpreadsheet, spreadsheets, value);
            List<string> preparedlList = new List<string>(); //creating our list
            foreach (DataRow row in spreadsheets[textSpreadsheet].Rows) //converting row into a string of list
            {
                string currentValue = row[textColumnNumber].ToString();
                preparedlList.Add(currentValue);
            }
            preparedlList = preparedlList.Distinct().ToList();
            return preparedlList;
        }

        public static List<string> GenerateThisLevelIDs(string higherOrderID, List<string> colValues)
        {
            List<string> readyList = new List<string>();
            foreach (string value in colValues)
            {
                readyList.Add(higherOrderID + value);
            }
            return readyList;
        }

        public static Dictionary<string, DataTable> Split(string filePath)
        {
            ConsoleSettings();
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                //create reader
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    //transform file into the dataset
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        // Gets or sets a value indicating whether to set the DataColumn.DataType
                        // property in a second pass.
                        UseColumnDataType = true,

                        // Gets or sets a callback to determine whether to include the current sheet
                        // in the DataSet. Called once per sheet before ConfigureDataTable.
                        FilterSheet = (tableReader, sheetIndex) => true,

                        // Gets or sets a callback to obtain configuration options for a DataTable.
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            // Gets or sets a value indicating the prefix of generated column names.
                            EmptyColumnNamePrefix = "Column",

                            // Gets or sets a value indicating whether to use a row from the
                            // data as column names.
                            UseHeaderRow = true,

                            // Gets or sets a callback to determine whether to include the
                            // current row in the DataTable.
                            FilterRow = (rowReader) =>
                            {
                                return true;
                            },

                            // Gets or sets a callback to determine whether to include the specific
                            // column in the DataTable. Called once per column after reading the
                            // headers.
                            FilterColumn = (rowReader, columnIndex) =>
                            {
                                return true;
                            }
                        }
                    });
                    Dictionary<string, DataTable> spreadsheets = new Dictionary<string, DataTable>();
                    foreach (DataTable dataTable in result.Tables)
                    {
                        spreadsheets[Naming(dataTable, spreadsheets)] = dataTable;
                    }
                    return spreadsheets;
                }
            }
        }

        public static bool isNum(string word)
        {
            try
            {
                int.Parse(word);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CheckDB()
        {
            return false;
        }

        public static List<int> SplitId(string Id)
        {
           return  Id.Split('|').Where(x => x != "").Select(x => Convert.ToInt32(x)).ToList();
        }
        public static int CompareIds(string _Id, string _otherId)
        {
            var currentId = SplitId(_Id);
            var otherId = SplitId(_otherId);
            for (int i = 0; i < currentId.Count; i++)
            {
                if (currentId[i] == otherId[i])
                {
                    continue;
                }
                return currentId[i] - otherId[i];
            }
            return 0;
        }

        public static string getFieldsInText(List<Dictionary<string, List<Value>>> fields)
        {
            string result = "";
            foreach (var optional_tagging in fields)
            {
                foreach (var field in optional_tagging)
                {
                    result += field.Key;
                    result += ":";
                    foreach (var fieldValue in field.Value)
                    {
                        result += fieldValue.name;
                        result += ";";
                    }
                    result += "||";
                }
                result += "\n";
            }
            return result;
        }

        public static string PartOutput(this IUnitGroup<ICorpusUnit> corpusUnit)
        {
            string collected = "";
            corpusUnit.subunits.Sort();
            foreach (var unit in corpusUnit.subunits)
            {
                collected += unit.Output();
            }
            return collected;
        }

        public static string UnitOutput(ICorpusUnit corpusUnit, bool atomicUnit = false)
        {
            var innerText = (corpusUnit is Grapheme || atomicUnit) ? (corpusUnit as IUnitGroup<ICorpusUnit>).PartOutput() : corpusUnit.text;
            if (corpusUnit.tagging is null || corpusUnit.tagging.Count < 1)
            {
                return "<span title= \"\" data-content=\"\" class=\"text\" id=\"" + corpusUnit.Id + "\"> " + innerText + "</span><br />";
            }
            return "<span title=\"" + getFieldsInText (corpusUnit.tagging) + "\" data-content=\"" + getFieldsInText (corpusUnit.tagging).Replace("\n", "<br />") + "\" class=\"text\" id=\"" + corpusUnit.Id + "\"> " + innerText + "</span><br />";
        }

    }
}
