using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MessagingClient.Business;
using MessagingClient.Core;
using MessagingClient.Core.Models;
using MessagingClient.Handlers;
using Newtonsoft.Json;

namespace MessagingClient.ViewModel
{
	public delegate void CommandHandler(ServerChatViewModel model, params string[] parameters);
	public class ServerChatViewModel : ViewModelBase
	{
		private ServerConnection Server { get; set; }
		private Messenger WindowMessenger { get; set; }
		private static object _syncLock = new object();
		private static object _groupLock = new object();
		private Thread RecievingMessagesThread { get; set; }
		private CommandHandlers _commandHandlers;
		private readonly ConcurrentDictionary<string, CommandHandler> handlers = new ConcurrentDictionary<string, CommandHandler>();

		private T RaiseProperty<T>(string name, T property, T previous) where T : class
		{
			if(property == previous)
				return previous;
			if (name != null) 
				RaisePropertyChanged(name);
			return property;
		}

		public ServerChatViewModel()
		{
			this.PropertyChanged += ChangedHandler;
			WindowMessenger = SimpleIoc.Default.GetInstance<Messenger>("WindowCommands");
			Messenger.Default.Register<ServerConnection>(this, Register);
			Messenger.Default.Register<int>(this, (k) =>
			{
				if (k == 0)
				{
					RecievingMessagesThread.Abort();
					RecievingMessagesThread.Join();
					Server.Dispose();
					this.Cleanup();
				}
			});
			BindingOperations.EnableCollectionSynchronization(Messages, _syncLock);
			BindingOperations.EnableCollectionSynchronization(Groups, _groupLock);
		}

		private async void ChangedHandler(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == "Status")
			{
				await Server.SetStatusAsync(Status);
				return;
			}
		}

		internal void ScrollToBottom()
		{
			WindowMessenger.Send(new WindowCommand(Command.Scroll, 0));
		}

		public void Register(ServerConnection info)
		{
			_commandHandlers = new CommandHandlers();
			RegisterHandlers();
			RecievingMessagesThread = new Thread(Recieve);
			RecievingMessagesThread.Start(info);
			Server = info;
			WindowTitle = Server.ServerName;
			Groups.Add(new GroupData("Lobby", "", false, false, true));
		}

		private void RegisterHandlers()
		{
			handlers.TryAdd("NEWMSG", _commandHandlers.NewMessage);
			handlers.TryAdd("AFKUSER", _commandHandlers.AfkUser);
			handlers.TryAdd("ALERT", _commandHandlers.Alert);
			handlers.TryAdd("SERVERMSG", _commandHandlers.ServerMessage);
			handlers.TryAdd("USERSRESP", _commandHandlers.UserResponse);
			handlers.TryAdd("SDOWN", _commandHandlers.Shutdown);
			handlers.TryAdd("NEWGRP", _commandHandlers.NewGroup);
			handlers.TryAdd("PING", _commandHandlers.Ping);
		}

		public async void Recieve(object conn)
		{
			ServerConnection connection = (ServerConnection) conn;
			while (true)
			{
				try
				{
					var pair = await connection.RecieveMessageAsync();
					if (pair == null)
					{
						Debug.WriteLine("Something has gone wrong");
						throw new SocketException(0);
					}
					if (pair.Command.ToLower() == "INVOP")
						continue;
					CommandHandler handler;
					if (handlers.TryGetValue(pair.Command, out handler))
						handler(this, pair.Parameters);
					else
						Debug.WriteLine(String.Format("{0} has not been implemented", pair.Command));
				}
				catch (ThreadAbortException)
				{
					Messenger.Default.Send(0);
					SimpleIoc.Default.Unregister(this);
					SimpleIoc.Default.Register(() => new ServerChatViewModel());
					return;
				}
				catch (IOException)
				{
					Messenger.Default.Send(0);
					SimpleIoc.Default.Unregister(this);
					SimpleIoc.Default.Register(() => new ServerChatViewModel());
					return;
				}
				catch (ObjectDisposedException)
				{
					Messenger.Default.Send(0);
					SimpleIoc.Default.Unregister(this);
					SimpleIoc.Default.Register(() => new ServerChatViewModel());
					return;
				}
				catch (SocketException)
				{
					Messenger.Default.Send(0);
					SimpleIoc.Default.Unregister(this);
					SimpleIoc.Default.Register(() => new ServerChatViewModel());
					return;
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.Message);
					Messenger.Default.Send(0);
					SimpleIoc.Default.Unregister(this);
					SimpleIoc.Default.Register(() => new ServerChatViewModel());
					return;
				}
			}
		}

		/// <summary>
		/// The <see cref="UserMessage" /> property's name.
		/// </summary>
		public const string UserMessagePropertyName = "UserMessage";
		private string _userMessage = "";
		public string UserMessage
		{
			get { return _userMessage; }
			set
			{
				if (_userMessage == value)
				{
					return;
				}

				_userMessage = value;
				RaisePropertyChanged(UserMessagePropertyName);
			}
		}

		/// <summary>
		/// The <see cref="WindowTitle" /> property's name.
		/// </summary>
		public const string WindowTitlePropertyName = "WindowTitle";
		private string _windowTitle = "Local Host";
		public string WindowTitle
		{
			get { return _windowTitle; }

			set
			{
				if (_windowTitle == value)
				{
					return;
				}

				_windowTitle = value;
				RaisePropertyChanged(WindowTitlePropertyName);
			}
		}

		/// <summary>
        /// The <see cref="Groups" /> property's name.
        /// </summary>
        public const string GroupsPropertyName = "Groups";
        private ObservableCollection<GroupData> _groups = new ObservableCollection<GroupData>();
        public ObservableCollection<GroupData> Groups
        {
	        get { return _groups; }
	        set { _groups = RaiseProperty(GroupsPropertyName, value, _groups); }
        }

		/// <summary>
		/// The <see cref="Messages" /> property's name.
		/// </summary>
		public const string MessagesPropertyName = "Messages";
		private ObservableCollection<MessageData> _messages = new ObservableCollection<MessageData>();

		public ObservableCollection<MessageData> Messages
		{
			get { return _messages; }

			set { _messages = RaiseProperty(MessagesPropertyName, value, _messages); }
		}

		private RelayCommand _enterCommand;
		public RelayCommand EnterCommand
		{
			get
			{
				return _enterCommand ?? (_enterCommand = new RelayCommand(
					ExecuteEnterCommand,
					CanExecuteEnterCommand));
			}
		}

		private async void ExecuteEnterCommand()
		{
			try
			{
				if (!_enterCommand.CanExecute(null) || String.IsNullOrEmpty(_userMessage))
					return;
				if (Server.IsOnline)
					await Server.SendOfflineAsync(false);
				CanEdit = false;
				await Server.SendMessageAsync(_userMessage);
				UserMessage = "";
				CanEdit = true;
			}
			catch (IOException)
			{
				
			}
		}
		private bool CanExecuteEnterCommand()
		{
			return true;
		}

		/// <summary>
		/// The <see cref="CanEdit" /> property's name.
		/// </summary>
		public const string CanEditPropertyName = "CanEdit";

		private bool _canEdit = true;

		/// <summary>
		/// Sets and gets the CanEdiCanEdit property.
		/// Changes to that property's value raise the PropertyChanged event. 
		/// </summary>
		public bool CanEdit
		{
			get { return _canEdit; }
			set
			{
				if (_canEdit == value)
				{
					return;
				}

				_canEdit = value;
				RaisePropertyChanged(CanEditPropertyName);
			}
		}

		/// <summary>
		/// The <see cref="Status" /> property's name.
		/// </summary>
		public const string StatusPropertyName = "Status";

		private string _status = "No status set";

		/// <summary>
		/// Sets and gets the SStatus property.
		/// Changes to that property's value raise the PropertyChanged event. 
		/// </summary>
		public string Status
		{
			get { return _status; }
			set { _status = RaiseProperty(StatusPropertyName, value, _status); }
		}

		private RelayCommand _requestUsers;

		/// <summary>
		/// Gets the requestUsers.
		/// </summary>
		public RelayCommand RequestUsers
		{
			get
			{
				return _requestUsers ?? (_requestUsers = new RelayCommand(
					ExecuteRequestUsers,
					CanExecuteRequestUsers));
			}
		}

		private async void ExecuteRequestUsers()
		{
			if (!RequestUsers.CanExecute(null))
			{
				return;
			}
			try
			{
				await Server.RequestUserInformationAsync();
			}
			catch (IOException)
			{
				
			}
		}

		private RelayCommand _setOffline;

		/// <summary>
		/// Gets the SetOnline.
		/// </summary>
		public RelayCommand SetOffline
		{
			get
			{
				return _setOffline ?? (_setOffline = new RelayCommand(
					ExecuteSetOnline,
					CanExecuteSetOnline));
			}
		}

		private async void ExecuteSetOnline()
		{
			if (!SetOffline.CanExecute(null))
			{
				return;
			}
			try
			{
				await Server.ToggleOfflineAsync();
			}
			catch (IOException)
			{
			}
		}

		private bool CanExecuteSetOnline()
		{
			return true;
		}

		private bool CanExecuteRequestUsers()
		{
			return true;
		}

		private RelayCommand _disconnect;

		/// <summary>
		/// Gets the Disconnect.
		/// </summary>
		public RelayCommand Disconnect
		{
			get
			{
				return _disconnect ?? (_disconnect = new RelayCommand(
					ExecuteDisconnect,
					CanSendMessage));
			}
		}

		private void ExecuteDisconnect()
		{
			if (!Disconnect.CanExecute(null))
			{
				return;
			}
			Server.Dispose();
		}

		private bool CanSendMessage()
		{
			try
			{
				Server.Ping();
				return true;
			}
			catch (IOException)
			{
				return false;
			}
		}
	}
}
