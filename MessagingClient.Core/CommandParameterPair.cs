namespace MessagingClient.Core
{
	public class CommandParameterPair
	{
		public string Command { get; set; }
		public string[] Parameters { get; set; }
		public int Length { get { return Parameters.Length; } }

		public CommandParameterPair(string command, params string[] parameters)
		{
			Command = command;
			Parameters = parameters ?? new string[0];
		}
	}
}
