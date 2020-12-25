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
		public MainWindow()
		{
			InitializeComponent();
			for(int i = 0; i < 66; i++)
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
				if(Grid.GetRow(item) == row)
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			GetProfilePictureAsync(user);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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
				Margin = new Thickness(5,0,5,0),
			};
			#endregion

			Label lName = new Label()
			{
				Content = " " + user.Name.Replace("_", "__"),
				Style = labelStyle
			};

			// single underscores have special meanings and have to be escaped
			Label steamID = new Label()
			{
				Content = user.SteamID.Render(false).Replace("_", "__"),
				Style = labelStyle
			};

			var cheatList = TextBoxCreate(user.Cheater?.CheatList);
			var threatLevel = TextBoxCreate(user.Cheater?.ThreatLevel.ToString());
			var submitter = TextBoxCreate(user.Cheater?.Submitter);
			var notes = TextBoxCreate(user.Cheater?.Notes);


			// tell our controls which grid column they should use.

			AddToGrid(lName, Grid.GetColumn(LblName));
			AddToGrid(steamID, Grid.GetColumn(LblSteam));
			AddToGrid(submitter, Grid.GetColumn(LblSubmitter));
			AddToGrid(cheatList, Grid.GetColumn(LblCheats));
			AddToGrid(notes, Grid.GetColumn(LblNotes));
			AddToGrid(threatLevel, Grid.GetColumn(LblThreatLevel));

			void HandleText(object sender, TextChangedEventArgs e)
			{
				if(!user.IsCheater)
				{
					return;
				}

				user.Cheater.CheatList = cheatList.Text;
				int.TryParse(threatLevel.Text, out int threat2);
				user.Cheater.ThreatLevel = threat2;
				user.Cheater.Submitter = string.IsNullOrEmpty(submitter.Text) ? Settings["UserNameOverride"] : submitter.Text;
				user.Cheater.LastKnownName = user.Name;
				user.Cheater.Notes = notes.Text;
				Cheaters.Save();
			}
			//notes.KeyDown += HandleText;
			notes.TextChanged += HandleText;
			threatLevel.TextChanged += HandleText;
			cheatList.TextChanged += HandleText;
			submitter.TextChanged += HandleText;


			if (user.IsCheater)
			{
				var rect = new Rectangle()
				{
					Fill = new SolidColorBrush(Color.FromRgb(160, 0, 0))
				};
				
				Panel.SetZIndex(rect, -1);

				ConnectedUserGrid.Children.Add(rect);
				Grid.SetColumnSpan(rect, ConnectedUserGrid.ColumnDefinitions.Count);
				Grid.SetRow(rect, user.Index);


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
				var addButton = new Button()
				{
					Content = "Add",
					Margin = new Thickness(5, 0, 10, 0)
				};

				// since we aren't tracking any of our objects outside of this function we create an anonymous function so we can reference the intended objects.
				addButton.Click += (object sender, RoutedEventArgs e) =>
				{
					int.TryParse(threatLevel.Text, out int threat);
					string sub = string.IsNullOrEmpty(submitter.Text) ? Settings["UserNameOverride"] : submitter.Text;

					user.Cheater = new Cheater()
					{
						LastKnownName = user.Name,
						AccountID = user.AccountID,
						CheatList = cheatList.Text,
						ThreatLevel = threat,
						Notes = notes.Text,
						Submitter = sub
					};

					Cheaters.Add(user.Cheater);
				
					OnClientDisconnected(user);
					OnClientConnected(user);
				};

				AddToGrid(addButton, Grid.GetColumn(LblThreatLevel) + 1);
			}
		}

		private async Task GetProfilePictureAsync(User user)
		{
			string profilePicUrl = Steam.DefaultProfilePicture;
			try
			{
				var url = $"http://steamcommunity.com/profiles/{user.SteamID.ConvertToUInt64()}/?xml=1";
				user.Profile = await Steam.GetProfileDataAsync(url);
				profilePicUrl = user.Profile.avatarFull;
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToAsyncString());
			}

			user.ProfilePicture = new Image()
			{
				Source = new BitmapImage(new(profilePicUrl, UriKind.Absolute)),
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
	}
}
