using DangerZoneHackerTracker.Models;
using System;
using System.Collections.Generic;
using System.Text;
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
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class SettingsWindow
	{
		readonly DatabaseConnection Connection;
		readonly Settings Settings;
		public SettingsWindow()
		{
			InitializeComponent();
			Connection = new DatabaseConnection();
			Settings = Connection.Table<Settings>().First();
			TxtStatusUpdate.Text = Settings.UpdateRate.ToString();
			LblVolume.Content = $"Volume: {Settings.Volume}";
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
	}
}
