using System;
using System.Text;

namespace MessagingClient.Core.Utilities
{
	public static class MessageUtilities
	{
		public static string EncodeMessage(CommandParameterPair pair)
		{
			if (pair.Length == 0)
				return pair.Command;
			var builder = new StringBuilder();
			builder.Append(String.Format("{0} ", pair.Command));
			for (int i = 0; i < pair.Length; i++)
			{
				builder.Append(String.Format("{0}&", Uri.EscapeDataString(pair.Parameters[i])));
			}
			return builder.ToString().Substring(0, builder.Length - 1);
		}

		public static CommandParameterPair DecodeMessage(string input)
		{
			if (String.IsNullOrEmpty(input))
				return null;
			string[] parameters = input.Split(' ');
			if (parameters.Length == 1)
				return new CommandParameterPair(parameters[0]);
			if (parameters.Length < 2)
				return null;
			var command = parameters[0];
			string[] escaped = parameters[1].Split('&');
			for (int i = 0; i < escaped.Length; i++)
			{
				escaped[i] = Uri.UnescapeDataString(Uri.UnescapeDataString(escaped[i]));
			}
			return new CommandParameterPair(command, escaped);
		}
	}
}
