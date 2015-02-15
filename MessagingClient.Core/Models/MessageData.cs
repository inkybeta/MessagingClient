namespace MessagingClient.Core.Models
{
	public class MessageData
	{
		public string User { get; set; }
		public string Message { get; set; }

		public MessageData(string user, string message)
		{
			User = user;
			Message = message;
		}
	}
}
