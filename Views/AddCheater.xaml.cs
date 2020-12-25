using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
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
	public partial class AddCheater
	{
		static Settings Settings = Settings.Init();
		static CheaterSet Cheaters = CheaterSet.Init();

		public AddCheater()
		{
			InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			SteamID steam = new();
			string url;
			if (!SteamID.CommunityURLRegex.IsMatch(TxtSteam.Text))
			{
				if (!steam.SetFromString(TxtSteam.Text) && !steam.SetFromSteam3String(TxtSteam.Text))
				{
					return;
				}
				url = $"https://steamcommunity.com/profiles/{steam.ConvertToUInt64()}/?xml=1";
			}
			else
			{
				url = $"{SteamID.CommunityURLRegex.Match(TxtSteam.Text).Value}/?xml=1";
			}

			var data = await Steam.GetProfileDataAsync(url);
			int.TryParse(TxtThreat.Text, out int threat);
			Cheater cheater = new()
			{
				AccountID = Convert.ToUInt64(data.steamID64),
				CheatList = TxtCheats.Text,
				LastKnownName = data.steamID,
				Submitter = Settings["UserNameOverride"],
				ThreatLevel = threat,
				Notes = TxtNotes.Text
			};

			Cheaters.Add(cheater);
		}

		private void TxtSteam_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtSteam.Text == "https://steamcommunity.com/id/kidfearless/")
			{
				TxtSteam.Text = "";
			}
		}

		private void TxtSteam_LostFocus(object sender, RoutedEventArgs e)
		{
			if (TxtSteam.Text == "")
			{
				TxtSteam.Text = "https://steamcommunity.com/id/kidfearless/";
			}
		}

		private void TxtCheats_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtCheats.Text == "Aimbot, Wallhacks, etc.")
			{
				TxtCheats.Text = "";
			}
		}

		private void TxtCheats_LostFocus(object sender, RoutedEventArgs e)
		{
			if (TxtCheats.Text == "")
			{
				TxtCheats.Text = "Aimbot, Wallhacks, etc.";
			}
		}

		private void TxtNotes_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtNotes.Text == "Spawns radio, orders deagle, hides in tunnels")
			{
				TxtNotes.Text = "";
			}
		}

		private void TxtNotes_LostFocus(object sender, RoutedEventArgs e)
		{
			if (TxtNotes.Text == "")
			{
				TxtNotes.Text = "Spawns radio, orders deagle, hides in tunnels";
			}
		}

		private void TxtThreat_GotFocus(object sender, RoutedEventArgs e)
		{
			if (TxtThreat.Text == "1-10")
			{
				TxtThreat.Text = "";
			}
		}

		private void TxtThreat_LostFocus(object sender, RoutedEventArgs e)
		{
			if (TxtThreat.Text == "")
			{
				TxtThreat.Text = "1-10";
			}
		}
	}
}
