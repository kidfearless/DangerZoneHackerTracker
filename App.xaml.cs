using SQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace DangerZoneHackerTracker
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	

	public partial class App : Application
	{
		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

			this.DispatcherUnhandledException += App_DispatcherUnhandledException;
		}

		private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
		{
			LogException(e.Exception);
		}

		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			LogException(e.Exception);

			e.Handled = true;
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			LogException(e.ExceptionObject as Exception);
		}

		private void LogException(Exception exception)
		{
			File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DangerZoneExceptions.txt"),
				$"\n\nUnhandled Exception: {exception.GetType().Name}\n" +
				$"Message: {exception.Message}\n" +
				$"Stack Trace:\n" +
				$"{exception.StackTrace}");
		}
	}
}
