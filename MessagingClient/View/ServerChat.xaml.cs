using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MessagingClient.Core;

namespace MessagingClient.View
{
	/// <summary>
	/// Description for ServerChat.
	/// </summary>
	public partial class ServerChat : Window
	{

		private bool _isClosing = false;
		/// <summary>
		/// Initializes a new instance of the ServerChat class.
		/// </summary>
		public ServerChat()
		{
			InitializeComponent();
			Messenger messenger = SimpleIoc.Default.GetInstance<Messenger>("WindowCommands");
			messenger.Register<WindowCommand>(this, (k) =>
			{
				if (k.Command == Command.Scroll)
					Dispatcher.Invoke(
						() =>
						{
							if (MessageBox.Items.Count != 0)
								MessageBox.ScrollIntoView(MessageBox.Items[MessageBox.Items.Count - 1]);
						});
			});
			Messenger.Default.Register<int>(this, (k) =>
			{
				if (k == 0 && !_isClosing)
				{
					Dispatcher.Invoke(this.Close);
				}
			});
			Closing += (sender, args) =>
			{
				_isClosing = true;
				Messenger.Default.Send(0);
			};
		}
	}
}