using DangerZoneHackerTracker.Models;
using NAudio.Wave;
using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Keyboard = DangerZoneHackerTracker.Models.Keyboard;
using Path = System.IO.Path;


namespace DangerZoneHackerTracker
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>


	public partial class App : Application
	{
		const int MAXPLAYERS = 65;
		readonly Regex SteamIDRegex;
		readonly Regex MapNameRegex;
		readonly Regex HostNameRegex;
		string CurrentMap = "";
		readonly User[] Users = new User[MAXPLAYERS];
		System.Timers.Timer CronTimer;
		internal Key StatusKey;
		public new MainWindow MainWindow { get => MainWindow.Current; set => MainWindow.Current = value; }
		public static new App Current { get => (App)Application.Current; }
		internal delegate void UserEvent(User user, object args);
		internal delegate void CheaterEvent(User user, object args);
		internal UserEvent UserDisconnected;
		internal UserEvent UserConnected;


		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

			this.DispatcherUnhandledException += App_DispatcherUnhandledException;

			// Initialize properties
			SteamIDRegex = new Regex(string.Format(@"# *\d+ *(\d+) *{0}(.*){0} *(STEAM_\d:\d:\d+)", "\""));
			MapNameRegex = new Regex(@"map\s+: (\w+)");
			HostNameRegex = new Regex(@" *hostname *: *(.+)");

			InitializeDatabase();
			CreateTimer();

			MainWindow.WindowInitialized += MainWindow_Initialized;
			UserDisconnected += OnClientDisconnected;
			UserConnected += OnClientConnected;
		}

		private void MainWindow_Initialized(object sender, EventArgs e)
		{
			((MainWindow)sender).BtnAutoStatus.Content = $"<{StatusKey}>";
		}

		private void CreateTimer()
		{
			// Create a repeating timer to check the console file
			CronTimer = new System.Timers.Timer(TimeSpan.FromSeconds(3).TotalMilliseconds);
			CronTimer.Elapsed += Timer_CheckStatus;
			CronTimer.Start();
		}

		private void InitializeDatabase()
		{
			if (!Directory.Exists(Path.GetDirectoryName(Constants.DatabasePath)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(Constants.DatabasePath));
			}
			// Create Table
			using var db = new DatabaseConnection();
			db.CreateTable<Cheater>();
			try
			{
				db.CreateTable<Settings>();
			}
			catch (Exception)
			{
				db.DropTable<Settings>();
				db.CreateTable<Settings>();
			}

			var setting = db.Table<Settings>().SingleOrDefault();
			if (setting != null && setting.StatusKey != null)
			{
				StatusKey = (Key)setting.StatusKey;
			}
		}

		/// <summary>
		/// Callback for our repeating timer. Sends the key presses and reads the console file.
		/// </summary>
		/// <param name="sender">N/A</param>
		/// <param name="e">N/A</param>
		private void Timer_CheckStatus(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				// Do a delayed read on the console after we've done a status update.
				new Timer(ReadConsole, sender, TimeSpan.FromSeconds(0.2).Milliseconds, Timeout.Infinite);

				// we have a key bound to status and are in game, then start activating our status key
				if (StatusKey != Key.None && Models.Window.GetActiveWindowTitle() == "Counter-Strike: Global Offensive")
				{
					Keyboard.SendKey(KeyConvert.ToDirectXKeyCode(StatusKey), KeyUp: false, Keyboard.InputType.Keyboard);
					Keyboard.SendKey(KeyConvert.ToDirectXKeyCode(StatusKey), KeyUp: true, Keyboard.InputType.Keyboard);
				}
			}
			catch { }
		}

		/// <summary>
		/// Reads and processes the console.log file
		/// </summary>
		/// <param name="nill">Unused parameter that the Timer class requires</param>
		private async void ReadConsole(object nill)
		{
			//Open the file in a way that won't bother csgo.
			var path = Models.Window.GetProcessLocation("csgo");
			if (string.IsNullOrEmpty(path))
			{
#if DEBUG
				path = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";
#else
				return;
#endif
			}
			var consolePath = Path.Combine(Path.GetDirectoryName(path), "csgo");
			using var stream = File.Open(Path.Combine(consolePath, "console.log"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			using var reader = new StreamReader(stream, true);
			if (reader.BaseStream.Length <= 0)
			{
				return;
			}

			var lines = (await reader.ReadToEndAsync()).Split('\n', StringSplitOptions.RemoveEmptyEntries);

			//clear the file to save on performance.
			stream.SetLength(0);
			stream.Close();

			var replacedUsers = Users.Where(u => u != null).ToList();
			var disconnectedUsers = new Dictionary<int, User>();
			var newlyConnectedUsers = new Dictionary<int, User>();
			int preCount = replacedUsers.Count;
			int totalUsers = 0;

			foreach (var line in lines)
			{
				#region mapchange
				var mapMatch = MapNameRegex.Match(line);
				// The first group match will be the current map on the server
				if (mapMatch.Success && mapMatch.Groups[1].Value != CurrentMap)
				{
					foreach (var u in Users)
					{
						if (u != null)
						{
							Users[u.Index] = null;
							disconnectedUsers[u.Index] = u;
						}
					}
					CurrentMap = mapMatch.Groups[1].Value;
				}
				#endregion
				#region hostname
				var hostMatch = HostNameRegex.Match(line);
				if (hostMatch.Success)
				{
					MainWindow.SetServerName(hostMatch.Groups[1].Value);
				}
				#endregion
				var match = SteamIDRegex.Match(line);
				// check for an exact match
				if (!match.Success)
				{
					continue;
				}

				// create our user from the match. Index 0 is the full match, every index after is our groups
				User user = new User()
				{
					Index = Convert.ToInt32(match.Groups[1].Value),
					Name = match.Groups[2].Value,
					SteamID = new SteamID(match.Groups[3].Value)
				};

				totalUsers++;

				var foundUser = Users.SingleOrDefault(e => e != null && e.Index == user.Index && user.SteamID.AccountID == e.SteamID.AccountID);
				replacedUsers?.Remove(foundUser);

				// we already had someone here under that index
				if (Users[user.Index] != null)
				{
					// is it someone different?
					if (Users[user.Index].SteamID.AccountID != user.SteamID.AccountID)
					{
						disconnectedUsers[user.Index] = (Users[user.Index]);
						newlyConnectedUsers[user.Index] = (user);
					}
				}
				else
				{
					newlyConnectedUsers[user.Index] = (user);
				}
			}

			if (preCount != 0 && totalUsers > 0)
			{
				foreach (var user in replacedUsers)
				{
					UserDisconnected.Invoke(user, true);
				}
			}
			foreach (var user in disconnectedUsers.Values)
			{
				UserDisconnected.Invoke(user, true);
			}

			foreach (var user in newlyConnectedUsers.Values)
			{
				UserConnected.Invoke(user, true);
			}

			MainWindow.SetPlayerCount(Users.Where(u => u != null).Count());
		}

		private void OnClientConnected(User user, object announce)
		{
			Users[user.Index] = user;

			var db = new DatabaseConnection();
			// create a Cheater table object to work from. Changes to this object will be reflected in the db.
			var cheater = db.Table<Cheater>().SingleOrDefault(e => e.AccountID == user.SteamID.AccountID);
			if (cheater != null)
			{
				if ((bool)announce)
				{
					ShowToastAsync(
						title: $"Hacker {user.Name} Found In Game",
						message: $"Threat Level: {cheater.ThreatLevel}\n" +
								$"Known Cheats: {cheater.CheatList}\n" +
								$"Previous Name: {cheater.LastKnownName}",
						duration: TimeSpan.FromSeconds(10.0));
					cheater.LastKnownName = user.Name;
					PlayHax();
				}
				user.Alerted = true;
				user.Cheater = cheater;
				db.InsertOrReplace(cheater, typeof(Cheater));
			}
		}

		private void OnClientDisconnected(User user, object args = null)
		{
			Users[user.Index] = null;
		}

		/// <summary>
		/// Wrapper for sending notifications.
		/// </summary>
		/// <param name="cheater">Player that we are notifying for</param>
		public async void ShowToastAsync(Cheater cheater)
		{
			var notificationManager = new NotificationManager();
			await notificationManager.ShowAsync(new NotificationContent()
			{
				Title = $"Hacker {cheater.LastKnownName} Found In Game",
				Message = $"Threat Level: {cheater.ThreatLevel}\n" +
							$"Known Cheats: {cheater.CheatList}",
				Type = NotificationType.Error
			}, expirationTime: TimeSpan.FromSeconds(10));
		}

		public async void ShowToastAsync(string title = "", string message = "", NotificationType type = NotificationType.Error, TimeSpan? duration = null)
		{
			var notificationManager = new NotificationManager();
			await notificationManager.ShowAsync(new NotificationContent()
			{
				Title = title,
				Message = message,
				Type = type
			}, expirationTime: duration);
		}
		/// <summary>
		/// Stock to play a particular sound
		/// </summary>
		private void PlayHax()
		{
			var stream = Assembly.GetExecutingAssembly()
								 .GetManifestResourceStream("DangerZoneHackerTracker.Resources.haaaaxedit.mp3");
			var outputDevice = new WaveOutEvent();
			var audioFile = new Mp3FileReader(stream);
			outputDevice.PlaybackStopped += (object sender, StoppedEventArgs e) =>
			{
				audioFile.Dispose();
				audioFile = null;
				stream.Dispose();
				stream = null;
			};

			outputDevice.Init(audioFile);
			outputDevice.Play();
		}


		private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
		{
			LogException(e.Exception);
		}

		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			LogException(e.Exception);

			//e.Handled = true;
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			LogException(e.ExceptionObject as Exception);
		}

		private void LogException(Exception exception)
		{
			try
			{

				File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DangerZoneExceptions.txt"),
					$"\n\nUnhandled Exception: {exception.GetType().Name}\n" +
					$"Message: {exception.Message}\n" +
					$"Stack Trace:\n" +
					$"{exception.StackTrace}");
			}
			catch { }
		}
	}
}
