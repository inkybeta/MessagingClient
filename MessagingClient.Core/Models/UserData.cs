using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingClient.Core.Models
{
	public class UserData
	{
		public string UserName { get; set; }
		public string Status { get; set; }
		public bool IsOnline { get; set; }
		public string ClientType { get; set; }

		public UserData(string userName, string status, bool isOnline, string clientType)
		{
			UserName = userName;
			Status = status;
			IsOnline = isOnline;
			ClientType = clientType;
		}
	}
}
