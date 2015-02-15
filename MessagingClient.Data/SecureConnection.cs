using System;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using MessagingClient.Core;
using MessagingClient.Core.Utilities;
using MessagingClient.Data.Interfaces;

namespace MessagingClient.Data
{
	public class SecureConnection : IConnection
	{
		private SslStream Stream { get; set; }

		public SecureConnection(SslStream client)
		{
			Stream = client;
		}

		public void SendMessage(CommandParameterPair pair)
		{
			byte[] message = Encoding.UTF8.GetBytes(MessageUtilities.EncodeMessage(pair));
			Stream.Write(BitConverter.GetBytes(message.Length), 0, 4);
			Stream.Write(message, 0, message.Length);
		}

		public async Task SendMessageAsync(CommandParameterPair pair)
		{
			byte[] message = Encoding.UTF8.GetBytes(MessageUtilities.EncodeMessage(pair));
			await Stream.WriteAsync(BitConverter.GetBytes(message.Length), 0, 4);
			await Stream.WriteAsync(message, 0, message.Length);
		}

		public CommandParameterPair RecieveMessage()
		{
			int messageLength;
			using (var stream = new MemoryStream())
			{
				while (stream.Length != 4)
				{
					var buffer = new byte[4 - stream.Length];
					int bytesRead = Stream.Read(buffer, 0, buffer.Length);
					stream.Write(buffer, 0, bytesRead);
				}
				messageLength = BitConverter.ToInt32(stream.ToArray(), 0);
			}
			using (var stream = new MemoryStream())
			{
				while (stream.Length != messageLength)
				{
					var buffer = (messageLength - stream.Length) > 512
						? new byte[512]
						: new byte[messageLength - stream.Length];
					int bytesRead = Stream.Read(buffer, 0, buffer.Length);
					stream.Write(buffer, 0, bytesRead);
				}
				return MessageUtilities.DecodeMessage(Encoding.UTF8.GetString(stream.ToArray()));
			}
		}

		public async Task<CommandParameterPair> RecieveMessageAsync()
		{
			int messageLength;
			using (var stream = new MemoryStream())
			{
				while (stream.Length != 4)
				{
					var buffer = new byte[4 - stream.Length];
					int bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);
					await stream.WriteAsync(buffer, 0, bytesRead);
				}
				messageLength = BitConverter.ToInt32(stream.ToArray(), 0);
			}
			using (var stream = new MemoryStream())
			{
				while (stream.Length != messageLength)
				{
					var buffer = (messageLength - stream.Length) > 512
						? new byte[512]
						: new byte[messageLength - stream.Length];
					int bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);
					await stream.WriteAsync(buffer, 0, bytesRead);
				}
				return MessageUtilities.DecodeMessage(Encoding.UTF8.GetString(stream.ToArray()));
			}
		}

		public void CloseConnection()
		{
			Stream.Close();
		}
	}
}
