﻿using System;
using System.Windows;
using MessagingClient.ViewModel;

namespace MessagingClient
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Initializes a new instance of the MainWindow class.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			Closing += (s, e) =>
			{
				ViewModelLocator.Cleanup();
				Environment.Exit(0);
			};
		}
	}
}