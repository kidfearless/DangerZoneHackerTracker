using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Dynamic;
using Biaui.Controls;
using Microsoft.Win32;
using Path = System.IO.Path;
using Microsoft.Data.Sqlite;
using Formatting = Newtonsoft.Json.Formatting;
using Newtonsoft.Json.Linq;
using System.Collections;
using AsyncFriendlyStackTrace;

namespace DangerZoneHackerTracker
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		//private GridView GridView => DataBindingObject.View as GridView;
		//public double ColumnWidth => GridView.Columns.Count / Math.Max(DataBindingObject.ActualWidth, 1);
		Settings Settings = Settings.Init();
		CheaterSet Cheaters = CheaterSet.Init();
		private readonly Color HackerColor = Color.FromRgb(160, 0, 0);
		private readonly Color BaseColor = Color.FromArgb(0, 0, 0, 0);
		public MainWindow()
		{
			InitializeComponent();
			for (int i = 0; i < 66; i++)
			{
				this.ConnectedUserGrid.RowDefinitions.Add(new());
			}
			App.Current.ClientConnected.AddEvent(OnClientConnected);
			App.Current.ClientDisconnected.AddEvent(OnClientDisconnected);
			App.Current.MapChanged.AddEvent(OnMapChange);
			App.Current.HostChanged.AddEvent(OnHostChange);
#if DEBUG
			/*App.Current.Users.Add(new User()
			{
				CheatList = "cheats",
				Index = 1,
				IsCheater = true,
				Name = "KiD Fearless",
				Notes = "spawns ivy",
				SteamID = new SteamID(),
				Submitter = "KiD Fearless",
				ThreatLevel = 10
			});
			App.Current.Users.Add(new User()
			{
				CheatList = "cheats",
				Index = 2,
				IsCheater = true,
				Name = "KiD Fearless",
				Notes = "spawns ivy",
				SteamID = new SteamID(),
				Submitter = "KiD Fearless",
				ThreatLevel = 10
			});*/
#endif

		}

		private void OnMapChange(string oldValue, string newValue)
		{
			LblMap.Content = $"Map: {newValue.Replace("_", "__")}";


			ConnectedUserGrid.Children.Clear();
			ConnectedUserGrid.Children.Add(LblPicture);
			ConnectedUserGrid.Children.Add(LblName);
			ConnectedUserGrid.Children.Add(LblSteam);
			ConnectedUserGrid.Children.Add(LblSubmitter);
			ConnectedUserGrid.Children.Add(LblCheats);
			ConnectedUserGrid.Children.Add(LblNotes);
			ConnectedUserGrid.Children.Add(LblThreatLevel);
			ConnectedUserGrid.Children.Add(LblButtonsAdd);

		}

		private void OnHostChange(string oldValue, string newValue)
		{
			LblServer.Content = $"Server: {newValue}";
		}

		private void OnClientDisconnected(User user)
		{
			LblPlayerCount.Content = $"Players: {App.Current.Users.Count}";
			Debug.WriteLine($"Disconnected user {user.Name}");
			RemoveGrid(user.Index);
		}

		private void OnClientConnected(User user)
		{
			RemoveGrid(user.Index);
			LblPlayerCount.Content = $"Players: {App.Current.Users.Count}";
			Debug.WriteLine($"Connected user {user.Name}");
			AddGrid(user);
		}

		/*
				private void Users_CollectionChanged(EventableSortedSet<User> sender, CollectionChangedAction action, User? itemChanged)
				{
					LblPlayerCount.Content = $"Players: {sender.Count}";

					switch (action)
					{
						case CollectionChangedAction.Add:

							break;
						case CollectionChangedAction.Remove:

							break;
						case CollectionChangedAction.Modified:
							Debug.WriteLine($"Modified user {itemChanged.Name}");

							ModifyGrid(itemChanged);
							break;
						case CollectionChangedAction.Multiple:
							Debug.WriteLine($"List Modified {itemChanged.Name}");

							RebuildGrid();
							break;
						default:
							break;
					}
				}
		*/

		private void RebuildGrid()
		{
			//throw new NotImplementedException();
		}

		private void ModifyGrid(User itemChanged)
		{
			//throw new NotImplementedException();
		}

		private void RemoveGrid(int row)
		{
			List<UIElement> items = new();
			foreach (UIElement item in ConnectedUserGrid.Children)
			{
				if (Grid.GetRow(item) == row)
				{
					items.Add(item);
				}
			}
			foreach (var item in items)
			{
				ConnectedUserGrid.Children.Remove(item);
			}
		}

		private void AddGrid(User user)
		{
			/*<Label Style="{StaticResource TableRowStyle}" x:Name="LblPicture" Grid.Column="0"></Label>
            <Label Style="{StaticResource TableRowStyle}" x:Name="LblName" Grid.Column="1" >Name</Label>
            <Label Style="{StaticResource TableRowStyle}" x:Name="LblSteam" Grid.Column="2" >Steam ID</Label>
            <Label Style="{StaticResource TableRowStyle}" x:Name="LblSubmitter" Grid.Column="3" >Submitter</Label>
            <Label Style="{StaticResource TableRowStyle}" x:Name="LblCheats" Grid.Column="4">Cheat List</Label>
            <Label Style="{StaticResource TableRowStyle}" x:Name="LblNotes" Grid.Column="5" >Notes</Label>
            <Label Style="{StaticResource TableRowStyle}" x:Name="LblThreatLevel" Grid.Column="6">Threat Level</Label>
            <Label Style="{StaticResource TableRowStyle}" x:Name="LblButtonsAdd" Grid.Column="7"></Label>*/
			var labelStyle = this.FindResource("TableRowLabelStyle") as Style;



			#region Helper Methods
			void AddToGrid(UIElement element, int column)
			{
				ConnectedUserGrid.Children.Add(element);
				Grid.SetColumn(element, column);
				Grid.SetRow(element, user.Index);
			}

			TextBox TextBoxCreate(string value) => new()
			{
				Text = value,
				Margin = new Thickness(5, 0, 5, 0),
			};
			#endregion

			user.UI.Name = new Label()
			{
				Content = " " + user.Name.Replace("_", "__"),
				Style = labelStyle
			};

			// single underscores have special meanings and have to be escaped
			user.UI.SteamID = new Label()
			{
				Content = user.SteamID.Render(false).Replace("_", "__"),
				Style = labelStyle
			};

			user.UI.CheatList = TextBoxCreate(user.Cheater?.CheatList);
			user.UI.ThreatLevel = TextBoxCreate(user.Cheater?.ThreatLevel.ToString());
			user.UI.Submitter = TextBoxCreate(user.Cheater?.Submitter);
			user.UI.Notes = TextBoxCreate(user.Cheater?.Notes);


			// tell our controls which grid column they should use.

			AddToGrid(user.UI.Name, Grid.GetColumn(LblName));
			AddToGrid(user.UI.SteamID, Grid.GetColumn(LblSteam));
			AddToGrid(user.UI.Submitter, Grid.GetColumn(LblSubmitter));
			AddToGrid(user.UI.CheatList, Grid.GetColumn(LblCheats));
			AddToGrid(user.UI.Notes, Grid.GetColumn(LblNotes));
			AddToGrid(user.UI.ThreatLevel, Grid.GetColumn(LblThreatLevel));

			// we want to get this information in the background, so we listen to the event for when it's ready.
			user.ProfileRetreived += User_ProfileRetreived;
			user.GetProfileData();

			void HandleText(object sender, TextChangedEventArgs e)
			{
				if (!user.IsCheater)
				{
					return;
				}

				user.Cheater.CheatList = user.UI.CheatList.Text;
				int.TryParse(user.UI.ThreatLevel.Text, out int threat2);
				user.Cheater.ThreatLevel = threat2;
				user.Cheater.Submitter = string.IsNullOrEmpty(user.UI.Submitter.Text) ? Settings["UserNameOverride"] : user.UI.Submitter.Text;
				user.Cheater.LastKnownName = user.Name;
				user.Cheater.Notes = user.UI.Notes.Text;
				Cheaters.Save();
			}
			//notes.KeyDown += HandleText;
			user.UI.Notes.TextChanged += HandleText;
			user.UI.ThreatLevel.TextChanged += HandleText;
			user.UI.CheatList.TextChanged += HandleText;
			user.UI.Submitter.TextChanged += HandleText;

			if (user.IsCheater)
			{
				var removeButton = new Button()
				{
					Content = "Remove",
					Margin = new Thickness(5, 0, 10, 0)
				};

				removeButton.Click += (object sender, RoutedEventArgs e) =>
				{
					if (user.Cheater is null)
					{
						return;
					}

					Cheaters.Remove(user.Cheater);

					user.Cheater = null;

					OnClientDisconnected(user);
					OnClientConnected(user);
				};

				AddToGrid(removeButton, Grid.GetColumn(LblThreatLevel) + 1);
			}
			else
			{
				user.UI.Button = new Button()
				{
					Content = "Add",
					Margin = new Thickness(5, 0, 10, 0)
				};

				// since we aren't tracking any of our objects outside of this function we create an anonymous function so we can reference the intended objects.
				user.UI.Button.Click += (object sender, RoutedEventArgs e) =>
				{
					int.TryParse(user.UI.ThreatLevel.Text, out int threat);
					string sub = string.IsNullOrEmpty(user.UI.Submitter.Text) ? Settings["UserNameOverride"] : user.UI.Submitter.Text;

					user.Cheater = new Cheater()
					{
						LastKnownName = user.Name,
						AccountID = user.AccountID,
						CheatList = user.UI.CheatList.Text,
						ThreatLevel = threat,
						Notes = user.UI.Notes.Text,
						Submitter = sub
					};

					Cheaters.Add(user.Cheater);

					OnClientDisconnected(user);
					OnClientConnected(user);
				};

				AddToGrid(user.UI.Button, Grid.GetColumn(LblThreatLevel) + 1);
			}
		}

		private void User_ProfileRetreived(User user, ProfileData.Profile profile)
		{
			GetProfilePicture(user);
			var fill = GetBackgroundColor(user);
			user.UI.BackgroundRect = new Rectangle()
			{
				Fill = new SolidColorBrush(fill)
			};

			Panel.SetZIndex(user.UI.BackgroundRect, -1);

			ConnectedUserGrid.Children.Add(user.UI.BackgroundRect);
			Grid.SetColumnSpan(user.UI.BackgroundRect, ConnectedUserGrid.ColumnDefinitions.Count);
			Grid.SetRow(user.UI.BackgroundRect, user.Index);
		}

		private void GetProfilePicture(User user)
		{
			var picture = user.Profile.avatarFull is null ? Steam.DefaultProfilePicture : user.Profile.avatarFull;
			user.ProfilePicture = new Image()
			{
				Source = new BitmapImage(new(picture, UriKind.Absolute)),
				Style = this.FindResource("ProfileImageStyle") as Style
			};
			user.ProfilePicture.MouseDown += (object sender, MouseButtonEventArgs e) =>
			{
				try
				{
					Process.Start(new ProcessStartInfo($"http://steamcommunity.com/profiles/{user.AccountID}") { UseShellExecute = true });
				}
				catch { }
			};

			ConnectedUserGrid.Children.Add(user.ProfilePicture);
			Grid.SetColumn(user.ProfilePicture, 0);
			Grid.SetRow(user.ProfilePicture, user.Index);
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			var result = base.MeasureOverride(availableSize);
			return result;
		}

		private void BulkAdd_Clicked(object sender, RoutedEventArgs e)
		{
			new BulkAddWindow().ShowDialog();
		}

		private void SingleAdd_Clicked(object sender, RoutedEventArgs e)
		{
			new AddCheater().ShowDialog();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Wrong Usage", "DF0010:Marks undisposed local variables.", Justification = "It is disposed")]
		private void Import_Clicked(object sender, RoutedEventArgs e)
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

			var dialog = new OpenFileDialog
			{
				Filter = "sqlite files (*.sq3)|*.sq3|JSON files (*.json)|*.json",
				Multiselect = false
			};
			try
			{
				if ((bool)dialog.ShowDialog())
				{
					var ext = Path.GetExtension(dialog.FileName);
					switch (ext)
					{
						case ".json":
							var text = File.ReadAllText(dialog.FileName);
							var cheaters = JsonConvert.DeserializeObject<IEnumerable<Cheater>>(text);
							foreach (var cheater in cheaters)
							{
								Cheaters.Add(cheater);
							}
							break;
						case ".sq3":
							var connection = new SqliteConnection($"Data Source={dialog.FileName}");
							connection.Open();

							#region Import Cheaters
							var cheaterCommand = connection.CreateCommand();
							cheaterCommand.CommandText = "SELECT * FROM Cheaters";

							using (var reader = cheaterCommand.ExecuteReader())
							{
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
										Notes = TryGetValue<string>(reader, "Notes") ?? "",
										Submitter = Settings["UserNameOverride"],
										ThreatLevel = (int)TryGetValue<long>(reader, "ThreatLevel")
									};
									Cheaters.Add(cheater);
								}
							}

							cheaterCommand.Dispose();
							connection.Dispose();
							#endregion
							break;
						default:
							ToastManager.ShowToastAsync("Could not import file", $"File \"{Path.GetFileName(dialog.FileName)}\" does not have a matching extension.");
							break;
					}
				}
			}
			catch (Exception ex)
			{
				ToastManager.ShowToastAsync($"Exception:\n{ex.Message}", ex.StackTrace.Substring(0, Math.Min(ex.StackTrace.Length, 1024)));
			}
		}

		private void Export_Clicked(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog
			{
				FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"exported_cheaters_{DateTime.Now.Ticks}.json"),
				DefaultExt = ".json",
				Filter = "JSON files (*.json)|*.json",
			};

			if ((bool)dialog.ShowDialog())
			{
				var jsonString = JsonConvert.SerializeObject(Cheaters, Formatting.Indented, new JsonSerializerSettings()
				{
					ContractResolver = new SettingsResolver()
				});
				using var writer = new StreamWriter(dialog.FileName);
				writer.Write(jsonString);
				writer.Flush();
			}
		}

		private void Settings_Clicked(object sender, RoutedEventArgs e)
		{
			new SettingsWindow().ShowDialog();
		}

		Color GetBackgroundColor(User user)
		{
			if (user.IsCheater)
			{
				return user.Cheater.ThreatLevel switch
				{
					-1 => Color.FromArgb(100, 255, 0, 0),
					0  => Color.FromArgb(100, 255, 255, 0),
					1  => Color.FromArgb(100, 255, 225, 0),
					2  => Color.FromArgb(100, 255, 200, 0),
					3  => Color.FromArgb(100, 255, 175, 0),
					4  => Color.FromArgb(100, 255, 150, 0),
					5  => Color.FromArgb(100, 255, 125, 0),
					6  => Color.FromArgb(100, 255, 100, 0),
					7  => Color.FromArgb(100, 255, 75, 0),
					8  => Color.FromArgb(100, 255, 50, 0),
					9  => Color.FromArgb(100, 255, 25, 0),
					10 => Color.FromArgb(100, 255, 0, 0),
					_  => Color.FromArgb(100, 255, 0, 0),
				};
			}
			else
			{
				int threatLevel = 0;
				if (user.Profile is not null)
				{
					if (user.Profile.privacyState != "private")
					{
						if (user.Profile.memberSince is not null)
						{
							if (!DateTime.TryParse(user.Profile.memberSince, out DateTime memberSince))
							{
								Debug.Fail("Bad Date Yo");
							}
							var difference = DateTime.Now - memberSince;

							var memberThreat = 5 - (difference.TotalDays / 365);
							threatLevel += (int)Math.Clamp(memberThreat, 0, 10);
						}
						else
						{
							threatLevel += 2;
						}

						if (user.Profile.mostPlayedGames is not null && user.Profile.mostPlayedGames.mostPlayedGame  is not null)
						{
							var game = user.Profile.mostPlayedGames.MostPlayedGames.FirstOrDefault(t => t.gameName == "Counter-Strike: Global Offensive");
							if (game is not null)
							{
								var timeThreat = 10 - (Convert.ToDouble(game.hoursPlayed) / 100);
								threatLevel += (int)Math.Clamp(timeThreat, 0, 10);
							}
						}
						else
						{
							//threatLevel += 2;
						}
					}
					else
					{
						threatLevel = -1;
					}

				}
				return threatLevel switch
				{
					2 => Color.FromArgb(25, 100, 100, 50),
					3 => Color.FromArgb(25, 100, 100, 75),
					4 => Color.FromArgb(25, 100, 100, 125),
					5 => Color.FromArgb(25, 100, 100, 150),
					6 => Color.FromArgb(25, 100, 100, 100),
					7 => Color.FromArgb(25, 100, 100, 175),
					8 => Color.FromArgb(25, 100, 100, 200),
					9 => Color.FromArgb(25, 100, 100, 225),
					10 => Color.FromArgb(25, 0, 100, 255),
					_ => Color.FromArgb(0, 0, 0, 0),
				};
			}
		}
	}
}
