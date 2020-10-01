using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DangerZoneHackerTracker.Models
{
	class Settings
	{
		[PrimaryKey]
		public int Key { get; set; } = 1;
		public Key? StatusKey { get; set; }

		public void UpdateStatusKey(Key key, DatabaseConnection connection)
		{
			StatusKey = key;
			connection.Update(this, typeof(Settings));
			StatusKeyChanged?.Invoke(this, EventArgs.Empty);
		}
		public double Volume { get; set; } = 1.0;
		public double UpdateRate { get; set; } = 3.0;
		public bool InitialWindowShowed { get; set; }

		public static EventHandler StatusKeyChanged;
	}
}
