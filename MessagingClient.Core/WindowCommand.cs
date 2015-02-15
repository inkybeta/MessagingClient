using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingClient.Core
{
	public class WindowCommand
	{
		public Command Command { get; set; }
		public int State { get; set; }

		public WindowCommand(Command command, int state)
		{
			Command = command;
			State = state;
		}
	}

	public enum Command
	{
		Scroll,
		ScrollUntil,
		ToggleContextMenu
	}
}
