using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using MessagingClient.Core.Models;
using MessagingClient.ViewModel;
using Newtonsoft.Json;

namespace MessagingClient.Handlers
{
	public class CommandHandlers
	{
		public void UserResponse(ServerChatViewModel model, params string[] parameters)
		{
			if (parameters.Length != 1)
				return;
			Dispatcher.CurrentDispatcher.Invoke(
				() =>
				{
					var users = JsonConvert.DeserializeObject<Dictionary<string, UserData>>(parameters[0]);
					var builder = new StringBuilder();
					foreach (KeyValuePair<string, UserData> i in users)
					{
						builder.Append(String.Format("Username: {0,20}| Is Online? {1,10}| Status: {2,30} \r\n", i.Key, i.Value.IsOnline == true ? "Yes" : "No", i.Value.Status));
					}
					model.Messages.Add(new MessageData("Users:", builder.ToString()));
					model.ScrollToBottom();
				});
		}

		public void ServerMessage(ServerChatViewModel model, params string[] parameters)
		{
			if (parameters.Length != 1)
				return;
			Dispatcher.CurrentDispatcher.Invoke(
				() =>
				{
					model.Messages.Add(new MessageData("Server", parameters[0]));
					model.ScrollToBottom();
				});
		}

		public void NewMessage(ServerChatViewModel model, params string[] parameters)
		{
			if (parameters.Length != 2)
				return;
			Dispatcher.CurrentDispatcher.Invoke(
				() =>
				{
					model.Messages.Add(new MessageData(parameters[0], parameters[1]));
				});
			model.ScrollToBottom();
		}

		public void AfkUser(ServerChatViewModel model, params string[] parameters)
		{
			if (parameters.Length != 2)
				return;
			Dispatcher.CurrentDispatcher.Invoke(
				() =>
				{
					model.Messages.Add(new MessageData("Server",
						String.Format("{0} is now {1}", parameters[0], parameters[1].ToLower() == "false" ? "online" : "offline")));
					model.ScrollToBottom();
				});
		}

		public void Alert(ServerChatViewModel model, params string[] parameters)
		{
			if (parameters.Length != 2)
				return;
			Dispatcher.CurrentDispatcher.Invoke(
				() =>
				{
					model.Messages.Add(new MessageData("Server", parameters[0]));
					model.ScrollToBottom();
				});
		}

		public void Shutdown(ServerChatViewModel model, params string[] parameters)
		{
			if (parameters.Length != 2)
				return;
			Dispatcher.CurrentDispatcher.Invoke(() =>
			{
				model.Messages.Add(new MessageData("Server", "The server is shutting down"));
				Thread.Sleep(500);
				Messenger.Default.Send(0);
			});
		}

		public void NewGroup(ServerChatViewModel model, params string[] parameters)
		{
			if (parameters.Length != 1)
				return;
			GroupData data = JsonConvert.DeserializeObject<GroupData>(parameters[0]);
			Dispatcher.CurrentDispatcher.Invoke(() =>
			{
				model.Groups.Add(data);
			});
		}

		public void Ping(ServerChatViewModel model, params string[] parameters)
		{
		}
	}
}
