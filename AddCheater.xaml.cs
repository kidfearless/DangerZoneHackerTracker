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
using Windows.AI.MachineLearning;

namespace DangerZoneHackerTracker
{

	/// <summary>
	/// Interaction logic for BulkAddWindow.xaml
	/// </summary>
	public partial class AddCheater
	{

		public AddCheater()
		{
			InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			using var db = new DatabaseConnection();
			var tuple = await Steam.TryGetProfileDataAsync(TxtSteam.Text);
			if(tuple.Result)
			{
				var cheats = string.IsNullOrWhiteSpace(TxtCheats.Text) ? "<N/A>" : TxtCheats.Text;
				if(!int.TryParse(TxtNotes.Text, out int threat))
				{
					threat = -1;
				}

				var cheater = db.Table<Cheater>().FirstOrDefault(c => c.AccountID == tuple.ProfileData.SteamID.AccountID);
				if (cheater == null)
				{
					db.Insert(new Cheater()
					{
						AccountID = tuple.ProfileData.SteamID.AccountID,
						CheatList = cheats,
						Notes = TxtNotes.Text,
						LastKnownName = tuple.ProfileData.PersonaName,
						ThreatLevel = threat
					}, typeof(Cheater));
				}
				else
				{
					cheater.CheatList = cheats;
					cheater.ThreatLevel = threat;
					cheater.Notes = TxtNotes.Text;
					cheater.LastKnownName = tuple.ProfileData.PersonaName;
					db.Update(cheater, typeof(Cheater));
				}
			}
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
