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
	public partial class FirstTimeSetupWindow
	{
		public FirstTimeSetupWindow()
		{
			InitializeComponent();
		}

		public bool IsTrackingStatusKey { get; private set; }

		private void BtnStatus_Click(object sender, RoutedEventArgs e)
		{
			IsTrackingStatusKey = true;
			BtnStatus.Content = "<...>";
		}

		private void BiaWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (IsTrackingStatusKey)
			{
				var key = e.SystemKey == Key.None ? e.Key : e.SystemKey;
				if (key == Key.Escape)
				{
					BtnStatus.Content = "<N/A>";

					App.Current.StatusKey = Key.None;
				}
				else
				{
					BtnStatus.Content = $"<{key}>";
					App.Current.StatusKey = key;
					using var db = new DatabaseConnection();
					var setting = db.Table<Settings>().First();
					setting.UpdateStatusKey(key, db);
				}

				IsTrackingStatusKey = false;
			}
		}

		private void BtnClose_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
