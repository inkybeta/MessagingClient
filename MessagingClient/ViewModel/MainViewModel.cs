using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MessagingClient.Business;
using MessagingClient.Data;
using MessagingClient.Model;
using MessagingClient.View;

namespace MessagingClient.ViewModel
{
	/// <summary>
	/// This class contains properties that the main View can data bind to.
	/// <para>
	/// See http://www.galasoft.ch/mvvm
	/// </para>
	/// </summary>
	public class MainViewModel : ViewModelBase
	{
		/// <summary>
		/// The <see cref="CanEdit" /> property's name.
		/// </summary>
		public const string IsSubmittingPropertyName = "CanEdit";

		private ServerChat Chat;
		private bool _canEdit = true;

		/// <summary>
		/// Sets and gets the CanEdit property.
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
				RaisePropertyChanged(IsSubmittingPropertyName);
			}
		}

		private IDataService DataService { get; set; }
		private INavigationService NavigationService { get; set; }

		/// <summary>
		/// The <see cref="LoginTitle" /> property's name.
		/// </summary>
		public const string LoginTitlePropertyName = "LoginTitle";

		private string _loginTitle = "Enter the server address and the username you want to use!";

		/// <summary>
		/// Gets the LoginTitle property.
		/// Changes to that property's value raise the PropertyChanged event. 
		/// </summary>
		public string LoginTitle
		{
			get { return _loginTitle; }

			set
			{
				if (_loginTitle == value)
					return;
				_loginTitle = value;
				RaisePropertyChanged(LoginTitlePropertyName);
			}
		}

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel(IDataService dataService)
		{
			DataService = dataService;
			DataService.GetData(
				(item, error) =>
				{
					if (error != null)
					{
						MessageBox.Show(error.Message);
						return;
					}
				});
			Messenger.Default.Register<int>(this, (k) =>
			{
				if (k == 0)
				{
					CanEdit = true;
				}
			});
		}

		/// <summary>
		/// The <see cref="ServerAddress" /> property's name.
		/// </summary>
		public const string ServerAddressPropertyName = "ServerAddress";

		private string _serverAddress = "";

		/// <summary>
		/// Sets and gets the ServerAddress property.
		/// Changes to that property's value raise the PropertyChanged event. 
		/// </summary>
		public string ServerAddress
		{
			get
			{
				return _serverAddress;
			}

			set
			{
				if (_serverAddress == value)
				{
					return;
				}

				_serverAddress = value;
				RaisePropertyChanged(ServerAddressPropertyName);
			}
		}

		/// <summary>
		/// The <see cref="ServerLabel" /> property's name.
		/// </summary>
		public const string ServerLabelPropertyName = "ServerLabel";

		private string _serverLabel = "Server Address:";

		/// <summary>
		/// Sets and gets the SeServerLabel property.
		/// Changes to that property's value raise the PropertyChanged event. 
		/// </summary>
		public string ServerLabel
		{
			get
			{
				return _serverLabel;
			}

			set
			{
				if (_serverLabel == value)
				{
					return;
				}

				_serverLabel = value;
				RaisePropertyChanged(ServerLabelPropertyName);
			}
		}

		/// <summary>
		/// The <see cref="UserNameLabel" /> property's name.
		/// </summary>
		public const string UserNameLabelPropertyName = "UserNameLabel";

		private string _userNameLabel = "Username:";

		/// <summary>
		/// Sets and gets the UsUserNameLabel property.
		/// Changes to that property's value raise the PropertyChanged event. 
		/// </summary>
		public string UserNameLabel
		{
			get
			{
				return _userNameLabel;
			}

			set
			{
				if (_userNameLabel == value)
				{
					return;
				}

				_userNameLabel = value;
				RaisePropertyChanged(UserNameLabelPropertyName);
			}
		}

		/// <summary>
		/// The <see cref="UserName" /> property's name.
		/// </summary>
		public const string UserNamePropertyName = "UserName";
		private string _userName = "";
		public string UserName
		{
			get { return _userName; }

			set
			{
				if (_userName == value)
					return;

				_userName = value;
				RaisePropertyChanged(UserNameLabelPropertyName);
			}
		}

		private RelayCommand _login;
		public RelayCommand Login
		{
			get
			{
				return _login ?? (_login = new RelayCommand(
					ExecuteMyCommand,
					CanExecuteMyCommand));
			}
		}

		private async void ExecuteMyCommand()
		{
			try
			{
				if (!Login.CanExecute(null))
					return;
				CanEdit = false;
				if (String.IsNullOrEmpty(_userName) | String.IsNullOrEmpty(_serverAddress))
				{
					ServerAddressColor = Brushes.Red;
					UserNameColorBrush = Brushes.Red;
					ErrorMessage = "Fields cannot be empty";
					CanEdit = true;
					return;
				}
				ServerConnection connection;
				try
				{
					var client = new TcpClient();
					client.Connect(ServerAddress, 2015);
					connection = new ServerConnection(new InsecureConnection(client), UserName, "No status set", true);
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.Message);
					ServerAddressColor = Brushes.Red;
					ErrorMessage = "Invalid Address.";
					CanEdit = true;
					return;
				}
				if (!(await connection.IsValid()))
				{
					ServerAddressColor = Brushes.Red;
					ErrorMessage = "Unable to connect to server";
					CanEdit = true;
					return;
				}
				var info = await connection.GetInformationAsync();
				if (info.ContainsKey("SSLENABLED") && info["SSLENABLED"].ToLower() == "true")
				{
					MessageBoxResult result = MessageBox.Show(
						"This server enables secure connections. However, this bars you from chatting with non secure clients.",
						"Enable security?", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes)
					{
						var client = new TcpClient(ServerAddress, 2015);
						connection.Connection.CloseConnection();
						connection.Connection = new SecureConnection(new SslStream(client.GetStream()));
					}
				}
				connection.ServerName = info.ContainsKey("SERVERNAME") ? info["SERVERNAME"] : ServerAddress;
				if (!(await connection.ConnectAsync(UserName)))
				{
					ErrorMessage = "Sorry, that username is already taken on the server";
					connection.Dispose();
					CanEdit = true;
					return;
				}
				Chat = new ServerChat();
				Messenger.Default.Send(connection);
				Chat.Show();
			}
			catch (IOException)
			{
				ErrorMessage = "The server has diconnected you. Either your username was bad or you have been banned";
			}
		}

		private bool CanExecuteMyCommand()
		{
			return true;
		}

		/// <summary>
		/// The <see cref="ErrorMessage" /> property's name.
		/// </summary>
		public const string ErrorMessagePropertyName = "ErrorMessage";
		private string _errorMessage = "";
		public string ErrorMessage
		{
			get { return _errorMessage; }

			private set
			{
				if (_errorMessage == value)
					return;

				_errorMessage = value;
				RaisePropertyChanged(ErrorMessagePropertyName);
			}
		}

		/// <summary>
		/// The <see cref="ServerAddressColor" /> property's name.
		/// </summary>
		public const string ServerAddressColorPropertyName = "ServerAddressColor";
		private SolidColorBrush _serverAddressColor = Brushes.Black;
		public SolidColorBrush ServerAddressColor
		{
			get { return _serverAddressColor; }

			private set
			{
				if (_serverAddressColor.Equals(value))
				{
					return;
				}

				_serverAddressColor = value;
				RaisePropertyChanged(ServerAddressColorPropertyName);
			}
		}

		/// <summary>
		/// The <see cref="UserNameColorBrush" /> property's name.
		/// </summary>
		public const string MyPropertyPropertyName = "UserNameColorBrush";
		private SolidColorBrush _userNameColorBrush = Brushes.Black;
		public SolidColorBrush UserNameColorBrush
		{
			get { return _userNameColorBrush; }

			private set
			{
				if (_userNameColorBrush.Equals(value))
				{
					return;
				}

				_userNameColorBrush = value;
				RaisePropertyChanged(MyPropertyPropertyName);
			}
		}
	}
}