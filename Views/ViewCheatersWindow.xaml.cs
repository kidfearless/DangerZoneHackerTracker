using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

using Biaui;
using Biaui.Controls;

namespace DangerZoneHackerTracker.Views
{
	/// <summary>
	/// Interaction logic for ViewCheatersWindow.xaml
	/// </summary>
	public partial class ViewCheatersWindow
	{
		CheaterSet Cheaters = CheaterSet.Init();
		Settings Settings = Settings.Init();

		private Func<Cheater, object> Selector = t => t.AccountID;
		private Thickness CheckBoxThickness;
		Style TableRowLabelStyle;

		IEnumerable<Cheater> SortedCheaters => Cheaters.OrderBy(Selector);

		public ViewCheatersWindow()
		{
			InitializeComponent();
			CheckBoxThickness = new Thickness(16, 7, 0, 0);
			TableRowLabelStyle = this.FindResource("TableRowLabelStyle") as Style;

			foreach (int i in Enumerable.Range(0, Cheaters.Count))
			{
				GridCheaters.RowDefinitions.Add(new());			
			}
			UpdateCheaterGrid();
		}

		void UpdateCounterLabels()
		{
			int total = Cheaters.Count;
			int cheaterCount = Cheaters.Where(t => t.ThreatLevel != 0).Count();
			LblTotalCount.Content = $"Total: {total}";
			LblCheaterCount.Content = $"Cheaters: {cheaterCount}";
			LblUserCount.Content = $"Notes: {total - cheaterCount}";

		}

		private void UpdateCheaterGrid()
		{
			List<UIElement> savedItems = new();

			foreach (UIElement item in this.GridCheaters.Children)
			{
				if (Grid.GetRow(item) == 0)
				{
					savedItems.Add(item);
				}
			}

			this.GridCheaters.Children.Clear();
			foreach (UIElement item in savedItems)
			{
				this.GridCheaters.Children.Add(item);
			}

			var cheaters = SortedCheaters.ToArray();
			for (int i = 0; i < cheaters.Length; i++)
			//for (int i = 0; i < 50; i++)
			{
				AddGrid(cheaters[i], i + 1);
			}

			UpdateCounterLabels();
		}

		private void AddGrid(Cheater cheater, int row)
		{
			void AddToGrid(UIElement element, int column)
			{
				this.GridCheaters.Children.Add(element);
				Grid.SetColumn(element, column);
				Grid.SetRow(element, row);
			}

			TextBox TextBoxCreate(string value) => new()
			{
				Text = value,
				Margin = new Thickness(5, 0, 5, 0),
			};



			/* <Label Grid.Column="0">WhiteList</Label>
									<Label Grid.Column="1">Name</Label>
									<Label Grid.Column="2">Steam ID</Label>
									<Label Grid.Column="3">Submitter</Label>
									<Label Grid.Column="4">Cheat List</Label>
									<Label Grid.Column="5">Notes</Label>
									<Label Grid.Column="6">ThreatLevel</Label>
									<Label Grid.Column="7"></Label>*/

			BiaCheckBox wlCheckBox = new()
			{
				BoxBorderColor = new ByteColor((byte)(255 * .9), 200, 200, 200),
				Margin = CheckBoxThickness,
			};



			Label lblName = new()
			{
				Content = cheater.LastKnownName,
				Style = TableRowLabelStyle
			};
			Label lblSteamID = new()
			{
				Content = new SteamID(cheater.AccountID).Render(false).Replace("_", "__"),
				Style = TableRowLabelStyle
			};

			TextBox txtSubmitter = TextBoxCreate(cheater.Submitter);
			TextBox txtCheatList = TextBoxCreate(cheater.CheatList);
			TextBox txtNotes = TextBoxCreate(cheater.Notes);
			TextBox txtThreat = TextBoxCreate(cheater.ThreatLevel.ToString());
			Button btnRemove = new();
			btnRemove.Content = "Remove";

			AddToGrid(wlCheckBox, 0);
			AddToGrid(lblName, 1);
			AddToGrid(lblSteamID, 2);
			AddToGrid(txtSubmitter, 3);
			AddToGrid(txtCheatList, 4);
			AddToGrid(txtNotes, 5);
			AddToGrid(txtThreat, 6);
			AddToGrid(btnRemove, 7);

			btnRemove.Click += (object sender, RoutedEventArgs args) =>
			{
				Cheaters.Remove(cheater);
				GridCheaters.Children.Remove(wlCheckBox);
				GridCheaters.Children.Remove(lblName);
				GridCheaters.Children.Remove(lblSteamID);
				GridCheaters.Children.Remove(txtSubmitter);
				GridCheaters.Children.Remove(txtCheatList);
				GridCheaters.Children.Remove(txtNotes);
				GridCheaters.Children.Remove(txtThreat);
				GridCheaters.Children.Remove(btnRemove);
				UpdateCounterLabels();
			};

			lblSteamID.MouseRightButtonDown += (object sender, MouseButtonEventArgs e) =>
			{
				Clipboard.SetText($"{cheater.LastKnownName}, {cheater.CheatList}, {cheater.ThreatLevel}, {cheater.Notes} " +
						 $"\nhttp://steamcommunity.com/profiles/{cheater.AccountID}");


				lblSteamID.Content = "*Copied To Clipboard*";
				var timer = new System.Timers.Timer(1000);
				timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => Dispatcher.Invoke(() =>
				{
					lblSteamID.Content = new SteamID(cheater.AccountID).Render(false).Replace("_", "__");

					timer.Dispose();
				});
				timer.Start();
			};

			lblName.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) =>
			{
				try
				{
					Process.Start(new ProcessStartInfo($"http://steamcommunity.com/profiles/{cheater.AccountID}") { UseShellExecute = true });
				}
				catch { }
			};

			void HandleText(object sender, TextChangedEventArgs e)
			{
				cheater.CheatList = txtCheatList.Text;
				int.TryParse(txtThreat.Text, out int threat2);
				txtThreat.Text = threat2.ToString();
				cheater.ThreatLevel = threat2;
				cheater.Submitter = string.IsNullOrEmpty(txtSubmitter.Text) ? Settings["UserNameOverride"] : txtSubmitter.Text;

				cheater.Notes = txtNotes.Text;
				Cheaters.Save();
			}


			txtSubmitter.TextChanged += HandleText;
			txtCheatList.TextChanged += HandleText;
			txtNotes.TextChanged += HandleText;
			txtThreat.TextChanged += HandleText;

		}


		TextBox TextBoxCreate(string value) => new()
		{
			Text = value,
			Margin = new Thickness(5, 0, 5, 0),
		};
	}
}
