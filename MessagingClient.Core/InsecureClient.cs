using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessagingClient.Core
{
    public class InsecureClient
    {
		public TcpClient Client { get; set; }
		public NetworkStream Stream { get; set; }
	    public string UserName { get; set; }
	    public string Status { get; set; }
		public bool IsOnline { get; set; }
		public ConcurrentDictionary<string, string> GroupsAndRoles { get; set; }

	    public InsecureClient(TcpClient client, NetworkStream stream, string userName, string status, bool isOnline, ConcurrentDictionary<string, string> groupsAndRoles)
	    {
		    Client = client;
		    Stream = stream;
		    UserName = userName;
		    Status = status;
		    IsOnline = isOnline;
		    GroupsAndRoles = groupsAndRoles;
	    }
    }
}
