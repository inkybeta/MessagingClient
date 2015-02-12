using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MessagingClient.Model;

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
		private bool isSubmitting = true;
		private readonly IDataService _dataService;

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
			_dataService = dataService;
			_dataService.GetData(
				(item, error) =>
				{
					if (error != null)
					{
						MessageBox.Show(error.Message);
						return;
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

		private RelayCommand _login;

		/// <summary>
		/// Gets the Login.
		/// </summary>
		public RelayCommand Login
		{
			get
			{
				return _login ?? (_login = new RelayCommand(
					ExecuteMyCommand,
					CanExecuteMyCommand));
			}
		}

		private void ExecuteMyCommand()
		{
			if (!Login.CanExecute(null))
				return;
			
		}

		private bool CanExecuteMyCommand()
		{
			return true;
		}

		public override void Cleanup()
		{
		    base.Cleanup();
		}
	}
}