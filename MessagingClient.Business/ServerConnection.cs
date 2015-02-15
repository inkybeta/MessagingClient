using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MessagingClient.Core;
using MessagingClient.Data.Interfaces;
using Newtonsoft.Json;

namespace MessagingClient.Business
{
    public class ServerConnection : IDisposable
    {
		public IConnection Connection { get; set; }

	    public ConcurrentDictionary<string, string> GroupsAndRoles;

	    public string UserName { get; set; }
		public string Status { get; set; }
		public bool IsOnline { get; set; }

	    public string ServerName { get; set; }
		public bool IsConnected { get; set; }

	    public ServerConnection(IConnection connection, string userName, string status, bool isConnected)
	    {
		    IsOnline = false;
		    Connection = connection;
		    UserName = userName;
		    Status = status;
		    IsConnected = isConnected;
	    }

	    public CommandParameterPair RecieveMessage()
	    {
		    return Connection.RecieveMessage();
	    }

	    public async Task<CommandParameterPair> RecieveMessageAsync()
	    {
		    return await Connection.RecieveMessageAsync();
	    }

	    public async Task<bool> IsValid()
	    {
		    try
		    {
			    await Connection.SendMessageAsync(new CommandParameterPair("INFOREQ"));
			    await Connection.RecieveMessageAsync();
		    }
		    catch (Exception)
		    {
			    return false;
		    }
		    return true;
	    }

	    public async Task<bool> ConnectAsync(string username)
	    {
		    await Connection.SendMessageAsync(new CommandParameterPair("CONNECT", username, "InkyClient"));
		    while (true)
		    {
				CommandParameterPair pair = await Connection.RecieveMessageAsync();
			    if (pair == null)
			    {
				    Debug.WriteLine("ERROR");
				    continue;
			    }
			    if (pair.Command == "INVALIDUN")
			    {
				    return false;
			    }
			    if (pair.Command == "CONNECTED")
			    {
				    IsConnected = true;
				    return true;
			    }
		    }
	    }

	    public Dictionary<string, string> GetInformation()
	    {
			Connection.SendMessage(new CommandParameterPair("INFOREQ"));
		    CommandParameterPair pair = Connection.RecieveMessage();
		    if (pair.Command != "INFORESP" || pair == null)
			    return null;
		    return JsonConvert.DeserializeObject<Dictionary<string, string>>(pair.Parameters[1]);
	    }

	    public async Task<Dictionary<string, string>> GetInformationAsync()
	    {
		    await Connection.SendMessageAsync(new CommandParameterPair("INFOREQ"));
		    CommandParameterPair response;
		    while (true)
		    {
			    response = await Connection.RecieveMessageAsync();
			    if (response == null)
				    continue;
			    if (response.Command == "INFORESP")
				    break;
		    }
		    return JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Parameters[0]);
	    }

	    public async Task SendCommandAsync(CommandParameterPair pair)
	    {
		    await Connection.SendMessageAsync(pair);
	    }

	    public async Task SendMessageAsync(string message)
	    {
		    await Connection.SendMessageAsync(new CommandParameterPair("SEND", message));
	    }

	    public async Task SendOfflineAsync(bool isOffline)
	    {
		    await Connection.SendMessageAsync(new CommandParameterPair("AFK", isOffline.ToString()));
		    IsOnline = isOffline;
	    }

	    public async Task SetStatusAsync(string status)
	    {
		    await Connection.SendMessageAsync(new CommandParameterPair("STATUS", status));
		    Status = status;
	    }

	    public async Task RequestUserInformationAsync()
	    {
		    await Connection.SendMessageAsync(new CommandParameterPair("USERSREQ"));
	    }

	    public async Task CreateGroup(string groupname)
	    {
		    await Connection.SendMessageAsync(new CommandParameterPair("CREATEGRP", groupname));
	    }

	    public async Task ToggleOfflineAsync()
	    {
		    await SendOfflineAsync(!IsOnline);
	    }

	    public void Ping()
	    {
		    Connection.SendMessage(new CommandParameterPair("PING"));
	    }

	    public void Dispose()
		{
		    try
		    {
			    Connection.SendMessage(new CommandParameterPair("DISCONN"));
			    Connection.CloseConnection();
		    }
		    catch (IOException)
		    {
		    }
		}
    }
}
