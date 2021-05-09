namespace corpus_builder_api.Models
{
	class User
	{
		private string id;
		private string password;

		public User(string _id, string _password)
		{
			id = _id;
			password = _password;
		}

		public bool CheckPassword(string _password)
		{
			return this.password == _password;
		}
	}
}