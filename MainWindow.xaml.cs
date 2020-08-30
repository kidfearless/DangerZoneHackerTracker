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
using SteamIDs_Engine;
using Notifications.Wpf.Core;
using NAudio.Wave;
using DangerZoneHackerTracker.Models;
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
	struct ConnectedUser
	{
		public string Name;
		public string SteamID;
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 
	public partial class MainWindow
	{
		Key StatusKey;
		bool IsTrackingStatusKey;
		const string path = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\";
		Regex SteamIDRegex;
		List<string> AlertedPlayers;
		List<ConnectedUser> Users;


		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		public MainWindow()
		{
			InitializeComponent();
			
			// Initialize properties
			Users = new List<ConnectedUser>();
			AlertedPlayers = new List<string>();
			SteamIDRegex = new Regex(string.Format(@"(\\?{0}.*\\?{0}) (STEAM_\d:\d:\d+)", "\""));

			// Create Table
			using var db = new DatabaseConnection();
			db.CreateTable<Cheater>();

			// Create a repeating timer to check the console file
			var timer = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
			timer.Elapsed += Timer_CheckStatus;
			timer.Start();

			// check console on program run
			Timer_CheckStatus(null, null);
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
				Title =		$"Hacker {cheater.LastKnownName} Found In Game",
				Message =	$"Threat Level: {cheater.ThreatLevel}\n" +
							$"Known Cheats: {cheater.CheatList}",
				Type = NotificationType.Error
			});
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
				new Timer(ReadConsole, null, TimeSpan.FromSeconds(0.2).Milliseconds, Timeout.Infinite);

				// we have a key bound to status and are in game, then start activating our status key
				if (StatusKey != Key.None && Models.Window.GetActiveWindowTitle().Contains("csgo"))
				{
					Keyboard.SendKey(KeyConvert.ToDirectXKeyCode(StatusKey), false, Keyboard.InputType.Keyboard);
					Keyboard.SendKey(KeyConvert.ToDirectXKeyCode(StatusKey), true, Keyboard.InputType.Keyboard);
				}
			}
			catch (Exception _)	{}
		}

		/// <summary>
		/// Reads and processes the console.log file
		/// </summary>
		/// <param name="nill">Unused parameter that the Timer class requires</param>
		private void ReadConsole(object nill)
		{
			// Open the file in a way that won't bother csgo.
			using var stream = File.Open(Path.Combine(path, "console.log"), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			using var reader = new StreamReader(stream, true);

			using var db = new DatabaseConnection();
			Debug.WriteLine($"size bf:{reader.BaseStream.Length}");
			string line;
			// iterate each line of the file
			while ((line = reader.ReadLine()) != null)
			{
				var match = SteamIDRegex.Match(line);
				// check for an exact match
				if (!match.Success || match.Groups.Count != 3)
				{
					continue;
				}

				// create our user from the match. Index 0 is the full match, every index after is our groups
				ConnectedUser user = new ConnectedUser()
				{
					Name = match.Groups[1].Value,
					SteamID = match.Groups[2].Value
				};

				// TODO: Write HashMap implementation of Users.
				// Check if user is already in list of connected users.
				if (!Users.Any(u => u.SteamID == user.SteamID) )
				{
					Users.Add(user);
				}

				// create a Cheater table object to work from. Changes to this object will be reflected in the db.
				var cheater = db.Table<Cheater>().SingleOrDefault(e => e.SteamID2 == user.SteamID);
				if (cheater != null)
				{
					// Check if we've alerted them to this player this map.
					if (!AlertedPlayers.Contains(user.SteamID))
					{
						// should i show their current name or their saved name?
						cheater.LastKnownName = user.Name;
						ShowToastAsync(cheater);
						PlayHax();
						AlertedPlayers.Add(user.SteamID);
						db.InsertOrReplace(cheater, typeof(Cheater));
					}
				}
			}

			//clear the file to save on performance.
			stream.SetLength(0);
			Debug.WriteLine($"size af:{reader.BaseStream.Length}");
			stream.Close();
			// remove duplicate users from the list
			Users = Users.Distinct().ToList();
			// can only update controls from the main thread, which we are not in. So we invoke an inline function to do it for us.
			this.Dispatcher.Invoke(() =>
			{
				// remove all children except the headers
				StackPanel.Children.RemoveRange(1, StackPanel.Children.Count - 2);
				// update the list of users in the app
				foreach (var user in Users)
				{
					StackPanel.Children.Add(this.CreateUserRow(user.Name, user.SteamID));
				}
			});
		}

		private void ExportClicked(object sender, RoutedEventArgs e)
		{
			// TODO: Implement
			ShowToastAsync(new Cheater()
			{
				LastKnownName = "KiD Fearless",
				ThreatLevel = 10,
				CheatList = "Aimbot, Bhop scripts, Spinbot, God"
			});

			PlayHax();
		}

		private void ImportClicked(object sender, RoutedEventArgs e)
		{

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
			// TODO: Accept community urls
			var steamAccount = new SteamID_Engine(TxtSteamID.Text);
			using var db = new DatabaseConnection();
			int.TryParse(TxtThreatLevel.Text.Trim(), out int threat);
			db.Insert(new Cheater()
			{
				SteamID2 = steamAccount.Steam2,
				ThreatLevel = threat,
				CheatList = TxtCheats.Text,
				LastKnownName = TxtName.Text
			});

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
				}

				IsTrackingStatusKey = false;
			}
		}
		#region Placeholder handlers
		private void SteamID_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtSteamID.Text == "STEAM:0:1:12354847")
			{
				TxtSteamID.Text = "";
			}
		}

		private void ThreatLevel_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtThreatLevel.Text == "1-10")
			{
				TxtThreatLevel.Text = "";
			}
		}

		private void Name_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtName.Text == "The Suspect")
			{
				TxtName.Text = "";
			}
		}

		private void Cheats_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtCheats.Text == "Spinbot, Aimbot, Wallhacks")
			{
				TxtCheats.Text = "";
			}
		}

		#endregion

		/// <summary>
		/// Stock to play a particular sound
		/// </summary>
		private void PlayHax()
		{
			var outputDevice = new WaveOutEvent();
			var audioFile = new AudioFileReader("Resources/haaaaxedit.mp3");
			outputDevice.PlaybackStopped += (object sender, StoppedEventArgs e) =>
			{
				outputDevice.Dispose();
				outputDevice = null;
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
		private Grid CreateUserRow(string name, string steamid)
		{
			/*
			 *	<Label Grid.Column="0">Name</Label>
				<Label Grid.Column="1">Steam</Label>
				<Label Grid.Column="2">Cheat List</Label>
				<Label Grid.Column="3">Threat Level</Label>
				<Label Grid.Column="4">Add</Label>
			*/
			Grid grid = new Grid();
			//column definitions have to be unique... so this exists
			grid.ColumnDefinitions.Add(new ColumnDefinition()
			{
				Width = new GridLength(2, GridUnitType.Star)
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
				Content = name
			};
			// single underscores have special meanings and have to be escaped
			Label steamID = new Label()
			{
				Content = steamid.Replace("_", "__")
			};
			var cheatList = new TextBox();
			var threatLevel = new TextBox();

			var addButton = new Button()
			{
				Content = "Add"
			};

			// since we aren't tracking any of our objects outside of this function we create an anonymous function so we can reference the intended objects.
			addButton.Click += (object sender, RoutedEventArgs e) =>
			{
				using var db = new DatabaseConnection();
				int.TryParse(threatLevel.Text, out int threat);
				db.InsertOrReplace(new Cheater()
				{
					LastKnownName = name,
					SteamID2 = steamid,
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
			Grid.SetColumn(lName, 0);
			Grid.SetColumn(steamID, 1);
			Grid.SetColumn(cheatList, 2);
			Grid.SetColumn(threatLevel, 3);
			Grid.SetColumn(addButton, 4);

			return grid;
		}
	}
}