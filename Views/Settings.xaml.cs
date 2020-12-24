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
		Settings Settings = Settings.Init();

		public SettingsWindow()
		{
			InitializeComponent();

			LblUpdateRate.Content = $"Update Rate: {Settings["UpdateRate"].DoubleValue:F2}";
			LblVolume.Content = $"Volume: {Settings["Volume"].DoubleValue:F1}";
			TxtUserNameOverride.Text = $"{Settings["UserNameOverride"]}";

			TxtUserNameOverride.TextChanged += UserName_TextChanged;
			SliderUpdateRate.ValueChanged += UpdateRate_ValueChanged;
			SliderVolume.ValueChanged += Volume_ValueChanged;
		}

		private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Settings["Volume"] = e.NewValue;
			LblVolume.Content = $"Volume: {Settings["Volume"].DoubleValue:F2}";
		}
		
		private void UserName_TextChanged(object sender, TextChangedEventArgs e)
		{
			Settings["UserNameOverride"] = TxtUserNameOverride.Text;
		}

		private void UpdateRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Settings["UpdateRate"] = e.NewValue;
			LblUpdateRate.Content = $"Update Rate: {Settings["UpdateRate"].DoubleValue:F1}";
		}

	}
}
