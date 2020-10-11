
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
using System.Reflection;
using Windows.UI.Xaml.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Threading;


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
		public static MainWindow Current;
		public static EventHandler WindowInitialized;
		bool IsTrackingStatusKey;

		readonly Regex CommunityProfilePictureRegex;


		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		public MainWindow()
		{
			InitializeComponent();

			CommunityProfilePictureRegex = new Regex(string.Format(@"<link rel={0}image_src{0} href={0}(.*){0}>", '"'));


			Current = this;
			WindowInitialized?.Invoke(this, EventArgs.Empty);

			// this ugly thing calls the events on the display thread
			App.Current.UserConnected += (User user, object args) => Dispatcher.Invoke(() => OnUserConnected(user, args));
			App.Current.UserDisconnected += (User user, object args) => Dispatcher.Invoke(() => OnUserDisconnected(user, args));
		}

		private void OnUserDisconnected(User user, object args = null)
		{
			user.Dispose();
		}

		private void OnUserConnected(User user, object args = null)
		{
			CreateUserRow(user);
		}

		public void SetPlayerCount(int count)
		{
			LblPlayerCount.Dispatcher.Invoke(() => LblPlayerCount.Content = $"Players: {count}");
		}

		public void SetServerName(string name)
		{
			LblServer.Dispatcher.Invoke(() => LblServer.Content = $"Server: {name}");
		}


		#region event handlers
		private void ExportClicked(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog
			{
				FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "exported_cheaters.sq3"),
				DefaultExt = ".sq3",
				Filter = "sqlite files (*.sq3)|*.sq3|All Files (*.*)|*.*"
			};

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
			var dialog = new OpenFileDialog
			{
				Filter = "sqlite files (*.sq3)|*.sq3|All Files (*.*)|*.*",
				Multiselect = false
			};
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
			new AddCheater().Show();
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
				var key = e.SystemKey == Key.None ? e.Key : e.SystemKey;
				if (key == Key.Escape)
				{
					BtnAutoStatus.Content = "<N/A>";

					App.Current.StatusKey = Key.None;
				}
				else
				{
					BtnAutoStatus.Content = $"<{key}>";
					App.Current.StatusKey = key;
					using var db = new DatabaseConnection();
					var setting = db.Table<Settings>().First();
					setting.UpdateStatusKey(key, db);
				}

				IsTrackingStatusKey = false;
			}
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			new BulkAddWindow().Show();
		}
		#endregion

		/// <summary>
		/// Helper method to create a row in the grid with the data we want in it.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="steamid"></param>
		/// <returns></returns>
		private void CreateUserRow(User user)
		{
			/*
			 *	<Label Grid.Column="0">pic</Label>
			 *	<Label Grid.Column="1">Name</Label>
				<Label Grid.Column="2">Steam</Label>
				<Label Grid.Column="3">Cheat List</Label>
				<Label Grid.Column="4">Threat Level</Label>
				<Label Grid.Column="5">Add</Label>
			*/

			// create our controls
			Label lName = new Label()
			{
				Content = " " + user.Name.Replace("_", "__"),
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

			GetProfilePictureAsync(user);


			// add our controls to the grid
			ConnectedUserGrid.Children.Add(lName);
			ConnectedUserGrid.Children.Add(steamID);
			ConnectedUserGrid.Children.Add(cheatList);
			ConnectedUserGrid.Children.Add(threatLevel);


			user.Elements.Add(lName);
			user.Elements.Add(steamID);
			user.Elements.Add(cheatList);
			user.Elements.Add(threatLevel);


			// tell our controls which grid column they should use.
			lName.SetGridColumn(Grid.GetColumn(LblName));
			steamID.SetGridColumn(Grid.GetColumn(LblSteam));
			cheatList.SetGridColumn(Grid.GetColumn(LblCheats));
			threatLevel.SetGridColumn(Grid.GetColumn(LblThreatLevel));



			lName.SetGridRow(user.Index);
			steamID.SetGridRow(user.Index);
			cheatList.SetGridRow(user.Index);
			threatLevel.SetGridRow(user.Index);



			if (user.Cheater != null)
			{
				for (int i = 0; i < ConnectedUserGrid.ColumnDefinitions.Count; i++)
				{
					var rect = new Rectangle()
					{
						Fill = new SolidColorBrush(Color.FromRgb(160, 0, 0))
					};

					rect.SetZIndex(-1);

					user.Elements.Insert(0, rect);
					ConnectedUserGrid.Children.Add(rect);
					rect.SetGridColumn(i);
					rect.SetGridRow(user.Index);
				}

				var removeButton = new Button()
				{
					Content = "Remove",
					Margin = new Thickness(0, 0, 10, 0)
				};

				removeButton.Click += (object sender, RoutedEventArgs e) =>
				{
					if (user.Cheater == null)
					{
						return;
					}

					using var db = new DatabaseConnection();
					var cheater = db.Table<Cheater>().FirstOrDefault(u => u.AccountID == user.SteamID.AccountID);
					if (cheater != null)
					{
						var query = $"DELETE FROM Cheaters WHERE {nameof(Cheater.AccountID)} = {cheater.AccountID}";
						db.Query<Cheater>(query);
					}

					cheatList.Text = "";
					threatLevel.Text = "";
					OnUserDisconnected(user);
					user.Cheater = null;
					OnUserConnected(user);
				};


				ConnectedUserGrid.Children.Add(removeButton);
				user.Elements.Add(removeButton);
				removeButton.SetGridColumn(Grid.GetColumn(LblThreatLevel) + 1);
				removeButton.SetGridRow(user.Index);
			}
			else
			{
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

					user.Cheater = new Cheater()
					{
						LastKnownName = user.Name,
						AccountID = user.SteamID.AccountID,
						CheatList = cheatList.Text,
						ThreatLevel = threat
					};
					db.InsertOrReplace(user.Cheater);

					cheatList.Text = "";
					threatLevel.Text = "";

					OnUserDisconnected(user);
					OnUserConnected(user);
				};

				ConnectedUserGrid.Children.Add(addButton);
				user.Elements.Add(addButton);
				addButton.SetGridColumn(Grid.GetColumn(LblThreatLevel) + 1);
				addButton.SetGridRow(user.Index);
			}
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

		private async Task GetProfilePictureAsync(User user)
		{
			var url = $"http://steamcommunity.com/profiles/{user.SteamID.ConvertToUInt64()}";
			var profilePicURL = await GetProfilePictureURL(url);
			user.Image = CreateImage(profilePicURL);
			user.Image.MouseDown += (object sender, MouseButtonEventArgs e) =>
			{
				try
				{
					Process.Start(new ProcessStartInfo($"http://steamcommunity.com/profiles/{user.SteamID.ConvertToUInt64()}") { UseShellExecute = true });
				}
				catch { }
			};

			ConnectedUserGrid.Children.Add(user.Image);
			user.Image.SetGridColumn(0);
			user.Image.SetGridRow(user.Index);
		}

		private void SettingsMenu_Clicked(object sender, RoutedEventArgs e)
		{
			new SettingsWindow().ShowDialog();
		}
	}
}