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
	}
}
