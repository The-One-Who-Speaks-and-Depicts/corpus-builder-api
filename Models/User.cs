namespace corpus_builder_api.Models
{
	class User
	{
		public string Id {get; set; }
		public string password;

		public User(string _id, string _password)
		{
			Id = _id;
			password = _password;
		}

		public bool CheckPassword(string _password)
		{
			return this.password == _password;
		}
	}
}