using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			Parameters = parameters;
		}
	}
}
