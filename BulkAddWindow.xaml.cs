using DangerZoneHackerTracker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DangerZoneHackerTracker
{

	/// <summary>
	/// Interaction logic for BulkAddWindow.xaml
	/// </summary>
	public partial class BulkAddWindow
	{
		private struct SteamUser
		{
			public string UserName;
			public SteamID SteamID;
		}

		private struct ProfileData
		{
			public string URL;
			public string SteamID;
			public string PersonaName;
			public string Summary;
		}

		private readonly Regex ProfileDataRegex;
		List<string> returnLines;
		List<SteamUser> users;


		public BulkAddWindow()
		{
			InitializeComponent();
			ProfileDataRegex = new Regex(@"g_rgProfileData = ({.+});");
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			var lines = TxtBox.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			TxtBox.Text = "";
			using var db = new DatabaseConnection();
			returnLines = new List<string>(lines);
			users = new List<SteamUser>();
			var taskList = new List<Task>();
			foreach (var line in lines)
			{
				var task = Task.Run(async () => await ReadLine(line));
				taskList.Add(task);
			}
			var taskCompleted = Task.WhenAll(taskList);
			taskCompleted.Wait();
			this.TxtBox.Dispatcher.Invoke(() =>
			{
				TxtBox.Text = string.Join('\n', returnLines);
			});
		}

		private async Task<int> ReadLine(string line)
		{
			try
			{
				SteamUser user = new SteamUser()
				{
					UserName = "<The Suspect>"
				};
				string cheats = "<Bulk Added User>";
				int threat = -1;

				var match = SteamID.CommunityURLRegex.Match(line);
				if (match.Success)
				{
					try
					{
						var profileData = await GetProfileDataAsync(match.Value);
						user.SteamID = new SteamID(Convert.ToUInt64(profileData.SteamID));
						user.UserName = profileData.PersonaName;
						returnLines.Remove(line);
						users.Add(user);
					}
					catch
					{
						if (ulong.TryParse(match.Groups[1].Value, out ulong steamid))
						{
							user.SteamID = new SteamID(steamid);
							returnLines.Remove(line);

							users.Add(user);
						}
						else
						{
							return 1;
							//continue;
						}
					}
				}
				else
				{
					user.SteamID = new SteamID(line);
					if (user.SteamID.AccountID != 0)
					{
						returnLines.Remove(line);
						users.Add(user);
					}
					else
					{
						return 1;
						//continue;
					}
				}

				var foundCheater = new DatabaseConnection().Table<Cheater>().Any(cheat => cheat.AccountID == user.SteamID.AccountID);

				if (!foundCheater)
				{
					new DatabaseConnection().Insert(new Cheater()
					{
						AccountID = user.SteamID.AccountID,
						ThreatLevel = threat,
						CheatList = cheats,
						LastKnownName = user.UserName
					});
				}
			}
			catch (Exception exc)
			{
			}
			return 0;
		}

		private async Task<ProfileData> GetProfileDataAsync(string url)
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
					var match = ProfileDataRegex.Match(line);
					if (match.Success)
					{
						response.Close();
						readStream.Close();

						return JsonConvert.DeserializeObject<ProfileData>(match.Groups[1].Value);
					}
				}

				response.Close();
				readStream.Close();
			}

			throw new Exception($"Could not find profile data from the given url: {url}");
		}

		private void TxtBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if(TxtBox.Text == "https://steamcommunity.com/id/kidfearless/")
			{
				TxtBox.Text = "";
			}
		}

		private void TxtBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if(TxtBox.Text == "")
			{
				TxtBox.Text = "https://steamcommunity.com/id/kidfearless/";
			}
		}
	}
}
