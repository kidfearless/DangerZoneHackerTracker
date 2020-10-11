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
		List<string> ReturnLines;


		public BulkAddWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			// Get each line in the text box and put it in a list
			var lines = TxtBox.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			ReturnLines = new List<string>(lines);
			
			// Wipe it early so that it looks like we're faster than we actually are.
			TxtBox.Text = "";

			// for every line in the text box we run it through the ReadLine function
			var taskList = new List<Task>();
			foreach (var line in lines)
			{
				var task = Task.Run(() => ReadLine(line));
				taskList.Add(task);
			}

			// We create a new Task for when all of the tasks in our task list are completed.
			Task.WhenAll(taskList).Wait();

			// recreate the list with any values that couldn't be parsed.
			TxtBox.Text = string.Join('\n', ReturnLines);
			if(TxtBox.Text.Length != 0 && !string.IsNullOrWhiteSpace(TxtBox.Text))
			{
				TxtBlockWarning.Visibility = Visibility.Visible;
			}
		}

		private async Task<int> ReadLine(string line)
		{
			var tuple = await Steam.TryGetProfileDataAsync(line);
			if(tuple.Result)
			{
				string cheats = "<Bulk Added User>";
				int threat = -1;
				var accountID = tuple.ProfileData.SteamID.AccountID;
				var foundCheater = new DatabaseConnection().Table<Cheater>().Any(cheat => cheat.AccountID == accountID);

				if (!foundCheater)
				{
					new DatabaseConnection().Insert(new Cheater()
					{
						AccountID = accountID,
						ThreatLevel = threat,
						CheatList = cheats,
						LastKnownName = tuple.ProfileData.PersonaName
					});
				}

				this.ReturnLines.Remove(line);
			}
			return 0;
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

		private void TxtBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (TxtBox.Text == "https://steamcommunity.com/id/kidfearless/")
			{
				TxtBox.Text = "";
			}
		}

		private void TxtBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (TxtBox.Text == "")
			{
				TxtBox.Text = "https://steamcommunity.com/id/kidfearless/";
			}
		}

		private void TxtBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!this.IsInitialized)
			{
				return;
			}
			if (TxtBox.Text == "")
			{
				TxtBlockWarning.Visibility = Visibility.Hidden;
				BtnAdd.IsEnabled = false;
			}
			else
			{
				BtnAdd.IsEnabled = true;
			}
		}

		private void BiaWindow_MouseDown(object sender, MouseButtonEventArgs e)
		{
		}
	}
}
