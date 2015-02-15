/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:MessagingClient.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using System.Diagnostics.CodeAnalysis;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MessagingClient.Design;
using MessagingClient.Model;
using Microsoft.Practices.ServiceLocation;

namespace MessagingClient.ViewModel
{
	/// <summary>
	/// This class contains static references to all the view models in the
	/// application and provides an entry point for the bindings.
	/// <para>
	/// See http://www.galasoft.ch/mvvm
	/// </para>
	/// </summary>
	public class ViewModelLocator
	{
		static ViewModelLocator()
		{
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
			if(!SimpleIoc.Default.IsRegistered<Messenger>("WindowCommands"))
				SimpleIoc.Default.Register(() => new Messenger(), "WindowCommands");
			if (ViewModelBase.IsInDesignModeStatic)
			{
				SimpleIoc.Default.Register<IDataService, DesignDataService>();
			}
			else
			{
				SimpleIoc.Default.Register<IDataService, DataService>();
			}

			SimpleIoc.Default.Register<MainViewModel>();
			SimpleIoc.Default.Register<ServerChatViewModel>();
		}

		/// <summary>
		/// Gets the Main property.
		/// </summary>
		[SuppressMessage("Microsoft.Performance",
			"CA1822:MarkMembersAsStatic",
			Justification = "This non-static member is needed for data binding purposes.")]
		public MainViewModel Main
		{
			get { return SimpleIoc.Default.GetInstance<MainViewModel>(); }
		}

		public ServerChatViewModel Server
		{
			get { return SimpleIoc.Default.GetInstance<ServerChatViewModel>(); }
		}

		/// <summary>
		/// Cleans up all the resources.
		/// </summary>
		public static void Cleanup()
		{
			SimpleIoc.Default.Reset();
		}
	}
}