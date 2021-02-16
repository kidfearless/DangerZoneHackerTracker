using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Dynamic;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using AsyncFriendlyStackTrace;

namespace DangerZoneHackerTracker
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application, IDisposable
	{
		Settings Settings;
		RemoteConsole Console;
		CheaterSet Cheaters;
		EventableSortedSet<User> users;
		public EventableSortedSet<User> Users { get => users; }
		EventableSortedSet<User> TempUsers;

		readonly Regex SteamIDRegex = new Regex(string.Format(@"# *\d+ *(?<Index>\d+) *{0}(?<Name>.*){0} *(?<SteamID>STEAM_\d:\d:\d+)", "\""));
		readonly Regex MapNameRegex = new Regex(@"map\s+: (\w+)");
		readonly Regex HostNameRegex = new Regex(@" *hostname *: *(.+)");

		public delegate void ClientEventCallback(User user);
		public delegate void StringChangedCallback(string oldValue, string newValue);
		public ThreadSafeEvent<ClientEventCallback> ClientConnected = new();
		public ThreadSafeEvent<ClientEventCallback> ClientDisconnected = new();
		public ThreadSafeEvent<StringChangedCallback> MapChanged = new();
		public ThreadSafeEvent<StringChangedCallback> HostChanged = new();

		public string MapName { get; set; }
		public string HostName { get; set; }

		private const string DatabaseFilename = "Cheaters.sq3";
		private static string DatabasePath
		{
			get
			{
				var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var folder = Path.Combine(basePath, "HackerTracker");
				return Path.Combine(folder, DatabaseFilename);
			}
		}

		public static new App Current => Application.Current as App;

		protected override void OnStartup(StartupEventArgs e)
		{
			//Debugger.Launch();
			AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
			base.OnStartup(e);

			users = new EventableSortedSet<User>(new UserComparer());
			TempUsers = new EventableSortedSet<User>(new UserComparer());

			Settings = Settings.Init();
			Cheaters = CheaterSet.Init();

			ImportOldDatabase();

			this.LoadCompleted += App_LoadCompleted;
			this.Activated += App_Activated;

			
			ClientConnected.AddEvent(OnClientConnected);
			ClientDisconnected.AddEvent(OnClientDisconnected);
			MapChanged.AddEvent(OnMapChanged);
		}

		private void OnMapChanged(string oldValue, string newValue)
		{
			Users.Clear();
		}

		private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
		{
			//if(e.Exception is not IOException)
			//{
			//	Logger.Log(e.Exception.ToString());
			//}
		}

#pragma warning disable CS0168 // Variable is declared but never used
		private void App_Activated(object sender, EventArgs e)
		{

			// wait until the first window is shown so that they can get the events when they are first fired.
			Console = new RemoteConsole();
			bool didConnect = Console.AwaitConnection();
			Console.LineRead += Console_LineRead;
			this.Activated -= App_Activated;
			if (!didConnect)
			{
				var wind = new MissingParamsWindow()
				{
					ShowActivated = true
				};
				wind.Show();
			}
		}
#pragma warning restore CS0168 // Variable is declared but never used

		private void App_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			
		}

		private void MainWindow_Activated(object sender, EventArgs e)
		{
			
		}

		private void ImportOldDatabase()
		{
			T TryGetValue<T>(SqliteDataReader reader, string value)
			{
				try
				{
					return (T)reader[value];
				}
				catch
				{
					return default;
				}
			}

			if (!Settings["HasImportedDB"])
			{
				if(File.Exists(DatabasePath))
				{
					using var connection = new SqliteConnection($"Data Source={DatabasePath}");
					connection.Open();

					#region Import Settings
					using var settingsCommand = connection.CreateCommand();
					settingsCommand.CommandText = "SELECT * FROM Settings";
					using var reader2 = settingsCommand.ExecuteReader();
					if (reader2.Read())
					{
						Settings["Volume"] = TryGetValue<double>(reader2, "Volume");
						Settings["UpdateRate"] = Math.Max(TryGetValue<double>(reader2, "UpdateRate"), 0.1);
						Settings["UserNameOverride"] = TryGetValue<string>(reader2, "UserNameOverride");

					}

					if (string.IsNullOrEmpty(Settings["UserNameOverride"]))
					{
						using var subkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
						Settings["UserNameOverride"] = subkey.GetValue("LastGameNameUsed") as string;
					} 
					#endregion

					#region Import Cheaters
					using var cheaterCommand = connection.CreateCommand();
					cheaterCommand.CommandText = "SELECT * FROM Cheaters";

					using var reader = cheaterCommand.ExecuteReader();
					while (reader.Read())
					{
						var steamid3 = $"[U:1:{TryGetValue<long>(reader, "AccountID")}]";
						var steam = new SteamID();
						steam.SetFromSteam3String(steamid3);
						var cheater = new Cheater()
						{
							CheatList = TryGetValue<string>(reader, "CheatList"),
							AccountID = steam.ConvertToUInt64(),
							LastKnownName = TryGetValue<string>(reader, "LastKnownName"),
							Notes = TryGetValue<string>(reader, "Notes")??"",
							Submitter = Settings["UserNameOverride"],
							ThreatLevel = (int)TryGetValue<long>(reader, "ThreatLevel")
						};
						Cheaters.Add(cheater);
					}
					#endregion
				}
				Settings["HasImportedDB"] = true;
			}
		}

		

		private void OnClientDisconnected(User user)
		{
			Debug.WriteLine($"'{user.Name}' has disconnected from the server");
			Users.Remove(user);
		}

		private void OnClientConnected(User user)
		{
			Debug.WriteLine($"'{user.Name}' has joined the server");
			user.Cheater = Cheaters.FirstOrDefault(t => t.AccountID == user.AccountID);
			Users.Add(user);
			if(user.IsCheater)
			{

				ToastManager.ShowToastAsync(
					title:   $"Hacker {user.Name} Found In Game",
					message: $"Threat Level: {user.Cheater.ThreatLevel}\n" +
							 $"Known Cheats: {user.Cheater.CheatList}\n" +
							 $"Previous Name: {user.Cheater.LastKnownName}",
						duration: TimeSpan.FromSeconds(10.0));
				user.Cheater.LastKnownName = user.Name;
				if(user.Cheater.ThreatLevel > 3)
				{
					SoundManager.PlayEmbeddedSound("haaaaxedit.mp3", Settings["Volume"]);
				}
				Cheaters.Save();
			}
		}

		private void Console_LineRead(string line)
		{
			if(line.StartsWith("hostname"))
			{
				var hostMatch = HostNameRegex.Match(line);
				if (HostName != hostMatch.Groups[1].Value)
				{
					HostChanged.Invoke(HostName, hostMatch.Groups[1].Value);
					HostName = hostMatch.Groups[1].Value;
				}
				return;
			}

			if(line == "Not connected to server")
			{
				// remove the users from the list preemptively but not from the grid until we have a new set.
				Users.Clear();
				return;
			}

			if(line == "#end")
			{
				var disconnectedUsers = users.Except(TempUsers, new UserComparer()).ToArray();
				foreach (var user in disconnectedUsers)
				{
					ClientDisconnected?.Invoke(user);
				}

				var connectedUsers = TempUsers.Except(users, new UserComparer()).ToArray();
				foreach (var user in connectedUsers)
				{
					ClientConnected?.Invoke(user);
				}

				TempUsers.Clear();

				return;
			}

			var match = SteamIDRegex.Match(line);
			if(match.Success)
			{
				User user = new User()
				{
					Index = Convert.ToInt32(match.Groups["Index"].Value),
					Name = match.Groups["Name"].Value,
					SteamID = new SteamID(match.Groups["SteamID"].Value)
				};

				TempUsers.Add(user);
				return;
			}

			match = MapNameRegex.Match(line);
			if(match.Success)
			{
				if(MapName != match.Groups[1].Value)
				{
					MapChanged.Invoke(MapName, match.Groups[1].Value);
					MapName = match.Groups[1].Value;
				}
				return;
			}


		}


		protected override void OnExit(ExitEventArgs e)
		{
			this.Dispose();
			base.OnExit(e);
		}

		public void Dispose()
		{
			Console?.Dispose();
			Console = null;
		}
	}
}
