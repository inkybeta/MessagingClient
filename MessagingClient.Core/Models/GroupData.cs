namespace MessagingClient.Core.Models
{
	public class GroupData
	{
		public string Name { get; set; }
		public string Password { get; set; }
		public bool RequiresPassword { get; set; }
		public bool RequiresSecure { get; set; }
		public bool LoggedIn { get; set; }

		public GroupData(string name, string password, bool requiresPassword, bool requiresSecure, bool loggedIn)
		{
			Name = name;
			Password = password;
			RequiresPassword = requiresPassword;
			RequiresSecure = requiresSecure;
			LoggedIn = loggedIn;
		}
	}
}