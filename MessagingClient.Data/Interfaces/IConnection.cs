using System.Threading.Tasks;
using MessagingClient.Core;

namespace MessagingClient.Data.Interfaces
{
	public interface IConnection
	{
		void SendMessage(CommandParameterPair pair);
		Task SendMessageAsync(CommandParameterPair pair);
		void CloseConnection();
		CommandParameterPair RecieveMessage();
		Task<CommandParameterPair> RecieveMessageAsync();
	}
}