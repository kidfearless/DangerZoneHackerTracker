﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Notifications.Wpf.Core;
using NAudio.Wave;
using DangerZoneHackerTracker.Models;
using System.Windows.Media.Imaging;
using System.Net;
using System.Text;
using Microsoft.Win32;
using SQLite;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Timers;
using Keyboard = DangerZoneHackerTracker.Models.Keyboard;
using Path = System.IO.Path;


/*
] status 
Connected to =[A:1:2968670214:15444]:0
hostname: Valve CS:GO US West Server (srcds118.112.3)
version : 1.37.6.5 secure
os      :  Linux
type    :  official dedicated
map     : de_dust2
players : 9 humans, 3 bots (16/0 max) (not hibernating)

# userid name uniqueid connected ping loss state rate
# 2986 2 "keynair" STEAM_1:1:79612627 00:50 677 76 active 196608
# 2987 3 "KiD Fearless" STEAM_1:1:29867327 00:20 55 0 active 786432
# 2969 5 "PhantaSize CS.Money" STEAM_1:0:144968082 07:55 57 0 active 786432
# 2970 6 "viper_king" STEAM_1:1:202926239 07:53 81 0 active 196608
# 2971 7 "Tempter" STEAM_1:0:208111545 07:31 72 0 active 196608
# 2066 8 "Me Like Fried Dog" STEAM_1:1:183699177  9:19:08 36 0 active 128000
# 2973 10 "Mr.Whiskers" STEAM_1:1:79766578 06:42 50 0 active 786432
#2983 "Derek" BOT active 64
# 2975 14 "Langris" STEAM_1:1:188869886 04:25 64 0 active 786432
#2984 "Victor" BOT active 64
#2985 "Vinny" BOT active 64
# 2927 18 "JustaDownyWithAWP" STEAM_1:1:117469735 24:09 75 0 active 196608
#end
*/
namespace DangerZoneHackerTracker
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 
	public partial class MainWindow
	{
		const int MAXPLAYERS = 65;
		Key StatusKey;
		bool IsTrackingStatusKey;
		readonly Regex SteamIDRegex;
		readonly Regex MapNameRegex;
		readonly Regex CommunityURLRegex;
		readonly Regex CommunityProfilePictureRegex;
		string CurrentMap = "";
		User[] Users = new User[MAXPLAYERS];
		System.Timers.Timer timer;
#if DEBUG
		bool Debug_IsInCallback = false;
#endif


		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		public MainWindow()
		{
			InitializeComponent();

			// Initialize properties
			SteamIDRegex = new Regex(string.Format(@"# *\d+ *(\d+) *{0}(.*){0} *(STEAM_\d:\d:\d+)", "\""));
			MapNameRegex = new Regex(@"map\s+: (\w+)");
			CommunityURLRegex = new Regex(@"(\d+)");
			CommunityProfilePictureRegex = new Regex(string.Format(@"<link rel={0}image_src{0} href={0}(.*){0}>", '"'));

			InitializeDatabase();
			CreateTimer();
		}

		private void CreateTimer()
		{
			// Create a repeating timer to check the console file
			timer = new System.Timers.Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
			timer.Elapsed += Timer_CheckStatus;
			timer.Start();
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
			db.CreateTable<Settings>();

			var setting = db.Table<Settings>().SingleOrDefault();
			if (setting != null && setting.StatusKey != null)
			{
				BtnAutoStatus.Content = $"<{setting.StatusKey}>";
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
				new System.Threading.Timer(ReadConsole, sender, TimeSpan.FromSeconds(0.2).Milliseconds, Timeout.Infinite);

				// we have a key bound to status and are in game, then start activating our status key
				if (StatusKey != Key.None && Models.Window.GetActiveWindowTitle() == "Counter-Strike: Global Offensive")
				{
					Keyboard.SendKey(KeyConvert.ToDirectXKeyCode(StatusKey), false, Keyboard.InputType.Keyboard);
					Keyboard.SendKey(KeyConvert.ToDirectXKeyCode(StatusKey), true, Keyboard.InputType.Keyboard);
				}
			}
			catch { }
		}

		/// <summary>
		/// Reads and processes the console.log file
		/// </summary>
		/// <param name="nill">Unused parameter that the Timer class requires</param>
		private void ReadConsole(object nill)
		{
#if DEBUG
			if (Debug_IsInCallback)
			{
				return;
			}
			Debug_IsInCallback = true;
#endif
			// prevent multiple instances of ReadConsole from running at the same time.
			timer.Stop();
			// Open the file in a way that won't bother csgo.
			var path = Models.Window.GetProcessLocation("csgo");
			if (string.IsNullOrEmpty(path))
			{
#if DEBUG
				path = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";
#else
				return;
#endif
			}
			var dir = Path.Combine(Path.GetDirectoryName(path), "csgo");
			using var stream = File.Open(Path.Combine(dir, "console.log"), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			using var reader = new StreamReader(stream, true);
			if (reader.BaseStream.Length <= 0)
			{
				return;
			}

			using var db = new DatabaseConnection();
			var tempUsers = Users.Where(u => u != null).ToList();
			int preCount = tempUsers.Count;

			string line;
			// iterate each line of the file
			while ((line = reader.ReadLine()) != null)
			{
				if (CheckForMapChange(line))
				{
					continue;
				}

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

				var foundUser = tempUsers.FirstOrDefault(u => u.SteamID.AccountID == user.SteamID.AccountID);
				if (foundUser != null)
				{
					tempUsers.Remove(foundUser);
				}
				if (Users[user.Index] != null)
				{
					if (Users[user.Index].SteamID.AccountID != user.SteamID.AccountID)
					{
						OnClientDisconnected(Users[user.Index]);
						Users[user.Index] = null;
						OnClientConnected(user);
					}
					// handle edge case
					else if (!user.Alerted && user.Cheater != null)
					{
						ShowToastAsync(
						title: $"Hacker {user.Name} Found In Game",
						message: $"Threat Level: {user.Cheater.ThreatLevel}\n" +
							$"Known Cheats: {user.Cheater.CheatList}\n",
						duration: TimeSpan.FromSeconds(10.0));
						PlayHax();
						user.Alerted = true;
					}
				}
				else if (Users[user.Index] == null)
				{
					OnClientConnected(user);
				}

				Users[user.Index] = user;
			}

			if (preCount != 0)
			{
				foreach (var user in tempUsers)
				{
					OnClientDisconnected(user);
				}
			}

			//clear the file to save on performance.
#if !DEBUG
			stream.SetLength(0);
#endif
			stream.Close();
#if DEBUG
			Debug_IsInCallback = false;
#endif
			timer.Start();
		}

		private bool CheckForMapChange(string line)
		{
			var mapMatch = MapNameRegex.Match(line);
			// The first group match will be the current map on the server
			if (mapMatch.Success && mapMatch.Groups[1].Value != CurrentMap)
			{
				OnMapChanged(oldMap: CurrentMap, currentMap: mapMatch.Groups[1].Value);
				CurrentMap = mapMatch.Groups[1].Value;
				return true;
			}
			return false;
		}

		private async void OnClientConnected(User user)
		{
			var db = new DatabaseConnection();
			// create a Cheater table object to work from. Changes to this object will be reflected in the db.
			var cheater = db.Table<Cheater>().SingleOrDefault(e => e.AccountID == user.SteamID.AccountID);
			if (cheater != null)
			{
				ShowToastAsync(
					title: $"Hacker {user.Name} Found In Game",
					message:$"Threat Level: {cheater.ThreatLevel}\n" +
							$"Known Cheats: {cheater.CheatList}\n" +
							$"Previous Name: {cheater.LastKnownName}",
					duration: TimeSpan.FromSeconds(10.0));
				cheater.LastKnownName = user.Name;
				PlayHax();
				user.Alerted = true;
				user.Cheater = cheater;
				db.InsertOrReplace(cheater, typeof(Cheater));
			}

			await Dispatcher.Invoke(async () =>
			{
				user.Grid = await CreateUserRowAsync(user);
				StackPanel.Children.Add(user.Grid);
			});
		}

		private void OnClientDisconnected(User user)
		{
			// can only update controls from the main thread, which we are not in. So we invoke an inline function to do it for us.
			Dispatcher.Invoke(() =>
			{
				if (user.Grid != null)
				{
					StackPanel.Children.Remove(user.Grid);
				}
			});
			Users[user.Index] = null;
		}

		private void OnMapChanged(string oldMap, string currentMap)
		{
			foreach (var user in Users)
			{
				if (user != null)
				{
					user.Alerted = false;
					user.Diconnected = true;
				}
			}
		}

		#region event handlers
		private void ExportClicked(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog();
			dialog.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "exported_cheaters.sq3");
			dialog.DefaultExt = ".sq3";
			dialog.Filter = "sqlite files (*.sq3)|*.sq3|All Files (*.*)|*.*";

			if ((bool)dialog.ShowDialog())
			{
				// Try to save the database file
				try
				{
					if (File.Exists(dialog.FileName))
					{
						File.Delete(dialog.FileName);
					}
					File.Copy(Constants.DatabasePath, dialog.FileName);
				}
				// If we fail try to save it under a unique name
				catch
				{
					try
					{
						File.Copy(Constants.DatabasePath, dialog.FileName + "." + Guid.NewGuid());
					}
					catch { }
				}
			}

		}

		private void ImportClicked(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.Filter = "sqlite files (*.sq3)|*.sq3|All Files (*.*)|*.*";
			dialog.Multiselect = false;
			if ((bool)dialog.ShowDialog())
			{
				using var otherDB = new SQLiteConnection(dialog.FileName, Constants.Flags);
				var cheaterList = otherDB.Table<Cheater>().ToList();
				using var myDB = new DatabaseConnection();
				foreach (var cheater in cheaterList)
				{
					myDB.InsertOrReplace(cheater, typeof(Cheater));
				}
			}

			// TODO: Implement
		}

		/// <summary>
		/// Callback function for when the status button is pressed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void StatusButtonClicked(object sender, RoutedEventArgs e)
		{
			IsTrackingStatusKey = true;
			BtnAutoStatus.Content = "<...>";
		}

		/// <summary>
		/// Callback function for when the Add Cheater button is pressed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddCheaterButtonClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				SteamID steamAccount;
				if (TxtSteamID.Text.Contains("community"))
				{
					var steam64 = CommunityURLRegex.Match(TxtSteamID.Text).Groups[1].Value;
					steamAccount = new SteamID(Convert.ToUInt64(steam64));
				}
				else
				{
					steamAccount = new SteamID(TxtSteamID.Text);
				}

				using var db = new DatabaseConnection();
				int.TryParse(TxtThreatLevel.Text.Trim(), out int threat);
				db.InsertOrReplace(new Cheater()
				{
					AccountID = steamAccount.AccountID,
					ThreatLevel = threat,
					CheatList = TxtCheats.Text,
					LastKnownName = TxtName.Text
				});
			}
			catch
			{

			}

			// reset the default placeholders
			TxtCheats.Text = "Spinbot, Aimbot, Wallhacks";
			TxtName.Text = "The Suspect";
			TxtSteamID.Text = "STEAM:0:1:12354847";
			TxtThreatLevel.Text = "1-10";
		}

		/// <summary>
		/// Callback function for when a key is pressed inside the entire application.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (IsTrackingStatusKey)
			{
				if (e.Key == Key.Escape)
				{
					BtnAutoStatus.Content = "<N/A>";

					StatusKey = Key.None;
				}
				else
				{
					BtnAutoStatus.Content = $"<{e.Key}>";
					StatusKey = e.Key;
					using var db = new DatabaseConnection();
					db.InsertOrReplace(new Settings() { StatusKey = e.Key }, typeof(Settings));
				}

				IsTrackingStatusKey = false;
			}
		}

		#endregion
		#region Placeholder handlers
		private void SteamID_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtSteamID.Text == "STEAM:0:1:12354847")
			{
				TxtSteamID.Text = "";
			}
		}
		private void SteamID_LostFocus(object sender, RoutedEventArgs e)
		{
			if (TxtSteamID.Text == "")
			{
				TxtSteamID.Text = "STEAM:0:1:12354847";
			}
		}

		private void ThreatLevel_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtThreatLevel.Text == "1-10")
			{
				TxtThreatLevel.Text = "";
			}
		}
		private void ThreatLevel_LostFocus(object sender, RoutedEventArgs e)
		{
			if (TxtThreatLevel.Text == "")
			{
				TxtThreatLevel.Text = "1-10";
			}
		}

		private void Name_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtName.Text == "The Suspect")
			{
				TxtName.Text = "";
			}
		}
		private void Name_LostFocus(object sender, RoutedEventArgs e)
		{
			if (TxtName.Text == "")
			{
				TxtName.Text = "The Suspect";
			}
		}

		private void Cheats_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtCheats.Text == "Spinbot, Aimbot, Wallhacks")
			{
				TxtCheats.Text = "";
			}
		}
		private void Cheats_LostFocus(object sender, RoutedEventArgs e)
		{
			if (TxtCheats.Text == "")
			{
				TxtCheats.Text = "Spinbot, Aimbot, Wallhacks";
			}
		}
		#endregion
		#region helper functions
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
			var outputDevice = new WaveOutEvent();
			var audioFile = new AudioFileReader("Resources/haaaaxedit.mp3");
			outputDevice.PlaybackStopped += (object sender, StoppedEventArgs e) =>
			{
				audioFile.Dispose();
				audioFile = null;
			};
			outputDevice.Init(audioFile);
			outputDevice.Play();
		}
		/// <summary>
		/// Helper method to create a row in the grid with the data we want in it.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="steamid"></param>
		/// <returns></returns>
		private async Task<Grid> CreateUserRowAsync(User user)
		{
			/*
			 *	<Label Grid.Column="0">pic</Label>
			 *	<Label Grid.Column="1">Name</Label>
				<Label Grid.Column="2">Steam</Label>
				<Label Grid.Column="3">Cheat List</Label>
				<Label Grid.Column="4">Threat Level</Label>
				<Label Grid.Column="5">Add</Label>
			*/
			Grid grid = new Grid();
			if (user.Cheater != null)
			{
				grid.Background = new SolidColorBrush(Color.FromRgb(160, 0, 0));
			}
			//column definitions have to be unique... so this exists
			grid.ColumnDefinitions.Add(new ColumnDefinition()
			{
				Width = new GridLength(0.5, GridUnitType.Star)
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition()
			{
				Width = new GridLength(1.5, GridUnitType.Star)
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition()
			{
				Width = new GridLength(1, GridUnitType.Star)
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition()
			{
				Width = new GridLength(1, GridUnitType.Star)
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition()
			{
				Width = new GridLength(1, GridUnitType.Star)
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition()
			{
				Width = new GridLength(0.2, GridUnitType.Star)
			});

			// create our controls
			Label lName = new Label()
			{
				Content = user.Name.Replace("_", "__"),
				Margin = new Thickness(0.0, 48 / 4, 48 / 4, 5.0)
			};
			// single underscores have special meanings and have to be escaped
			Label steamID = new Label()
			{
				Content = user.SteamID.Render(false).Replace("_", "__"),
				Margin = new Thickness(0.0, 48 / 4, 48 / 4, 5.0)

			};
			var cheatList = new TextBox()
			{
				Margin = new Thickness(5.0, 0.0, 5.0, 0.0),
				Text = user.Cheater != null ? user.Cheater.CheatList : ""
			};
			var threatLevel = new TextBox()
			{
				Margin = new Thickness(5.0, 0.0, 5.0, 0.0),
				Text = user.Cheater != null ? user.Cheater.ThreatLevel.ToString() : ""
			};

			var profilePicture = await GetProfilePictureAsync(user.SteamID);
			profilePicture.MouseDown += (object sender, MouseButtonEventArgs e) =>
			{
				try
				{
					Process.Start(new ProcessStartInfo($"http://steamcommunity.com/profiles/{user.SteamID.ConvertToUInt64()}") { UseShellExecute = true });
				}
				catch { }
			};
			grid.Children.Add(profilePicture);
			Grid.SetColumn(profilePicture, 0);

			var addButton = new Button()
			{
				Content = "Add",
				Margin = new Thickness(0, 0, 10, 0)
			};

			// since we aren't tracking any of our objects outside of this function we create an anonymous function so we can reference the intended objects.
			addButton.Click += (object sender, RoutedEventArgs e) =>
			{
				using var db = new DatabaseConnection();
				int.TryParse(threatLevel.Text, out int threat);
				db.InsertOrReplace(new Cheater()
				{
					LastKnownName = user.Name,
					AccountID = user.SteamID.AccountID,
					CheatList = cheatList.Text,
					ThreatLevel = threat
				});

				cheatList.Text = "";
				threatLevel.Text = "";
			};

			// add our controls to the grid
			grid.Children.Add(lName);
			grid.Children.Add(steamID);
			grid.Children.Add(cheatList);
			grid.Children.Add(threatLevel);
			grid.Children.Add(addButton);

			// tell our controls which grid column they should use.
			Grid.SetColumn(lName, 1);
			Grid.SetColumn(steamID, 2);
			Grid.SetColumn(cheatList, 3);
			Grid.SetColumn(threatLevel, 4);
			Grid.SetColumn(addButton, 5);

			return grid;
		}

		private Image CreateImage(string source, double width = 48.0, double height = 48.0)
		{
			var image = new Image();

			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(source, UriKind.Absolute);
			bitmap.EndInit();

			image.Source = bitmap;
			image.Width = width;
			image.Height = height;

			return image;
		}

		private async Task<string> GetProfilePictureURL(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			var response = (HttpWebResponse)await request.GetResponseAsync();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				Stream receiveStream = response.GetResponseStream();
				StreamReader readStream;

				if (string.IsNullOrWhiteSpace(response.CharacterSet))
				{
					readStream = new StreamReader(receiveStream);
				}
				else
				{
					readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
				}

				string line;
				while ((line = await readStream.ReadLineAsync()) != null)
				{
					var match = CommunityProfilePictureRegex.Match(line);
					if (match.Success)
					{
						response.Close();
						readStream.Close();

						return match.Groups[1].Value;
					}
				}

				response.Close();
				readStream.Close();
			}

			return string.Empty;
		}

		private async Task<Image> GetProfilePictureAsync(SteamID steam)
		{
			var url = $"http://steamcommunity.com/profiles/{steam.ConvertToUInt64()}";
			var profilePicURL = await GetProfilePictureURL(url);
			var image = CreateImage(profilePicURL);
			return image;
		}
		#endregion

	}
}