using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MessagingClient.Core;
using MessagingClient.Core.Utilities;
using MessagingClient.Data.Interfaces;

namespace MessagingClient.Data
{
    public class InsecureConnection : IConnection
    {
		private TcpClient Client { get; set; }
		private NetworkStream Stream { get; set; }

	    public InsecureConnection(TcpClient client)
	    {
		    Client = client;
		    Stream = client.GetStream();
	    }

	    public void SendMessage(CommandParameterPair pair)
	    {
		    byte[] message = Encoding.UTF8.GetBytes(MessageUtilities.EncodeMessage(pair));
			Stream.Write(BitConverter.GetBytes(message.Length), 0, 4);
			Stream.Write(message, 0 ,message.Length);
	    }

	    public async Task SendMessageAsync(CommandParameterPair pair)
	    {
		    string encoded = MessageUtilities.EncodeMessage(pair);
		    byte[] message = Encoding.UTF8.GetBytes(encoded);
		    await Stream.WriteAsync(BitConverter.GetBytes(message.Length), 0, 4);
			await Stream.WriteAsync(message, 0, message.Length);
	    }

	    public CommandParameterPair RecieveMessage()
	    {
		    int messageLength;
		    string message;
		    using (var stream = new MemoryStream())
		    {
			    while (stream.Length != 4)
			    {
				    var header = new byte[4 - stream.Length];
				    Stream.Read(header, 0, header.Length);
					stream.Write(header, 0 , header.Length);
			    }
			    messageLength = BitConverter.ToInt32(stream.ToArray(), 0);
		    }
		    using (var stream = new MemoryStream())
		    {
			    while (stream.Length != messageLength)
			    {
				    var buffer = stream.Length - messageLength > 512 ? new byte[512] : new byte[messageLength - stream.Length];
				    int bytesRead = Stream.Read(buffer, 0, buffer.Length);
					stream.Write(buffer, 0, bytesRead);
			    }
			    message = Encoding.UTF8.GetString(stream.ToArray());
		    }
			return MessageUtilities.DecodeMessage(message);
	    }

	    public async Task<CommandParameterPair> RecieveMessageAsync()
	    {
		    int messageLength;
		    string message;
		    using (var stream = new MemoryStream())
		    {
			    while (stream.Length != 4)
			    {
				    var header = new byte[4 - stream.Length];
				    await Stream.ReadAsync(header, 0, header.Length);
				    await stream.WriteAsync(header, 0, header.Length);
			    }
			    messageLength = BitConverter.ToInt32(stream.ToArray(), 0);
		    }
		    using (var stream = new MemoryStream())
		    {
			    while (stream.Length != messageLength)
			    {
				    var buffer = stream.Length - messageLength > 512
					    ? new byte[512]
					    : new byte[messageLength - stream.Length];
				    int bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);
				    await stream.WriteAsync(buffer, 0, bytesRead);
			    }
			    message = Encoding.UTF8.GetString(stream.ToArray());
		    }
		    return MessageUtilities.DecodeMessage(message);
	    }

	    public void CloseConnection()
		{
			Client.Client.Close();
		}
    }
}
