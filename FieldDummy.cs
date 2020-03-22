

namespace CorpusDraftCSharp 
{
	public class FieldDummy 
	{
		#region objectValues
		internal string fieldName;
		internal string tableName;
		internal string colName;
        internal string colTwoName;
		internal int colNumber;
		#endregion
		
		public FieldDummy(string fieldName, string tableName, string colName, int ColNum) 
		{
			this.fieldName = fieldName;
			this.tableName = tableName;
			this.colName = colName;
			this.colNumber = ColNum;
		}

        public FieldDummy (string fieldName, string tableName, string colName, string colTwoName, int ColNum)
        {
            this.fieldName = fieldName;
            this.tableName = tableName;
            this.colName = colName;
            this.colTwoName = colTwoName;
            this.colNumber = ColNum;
        }
        
	}
}