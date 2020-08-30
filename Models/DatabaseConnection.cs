using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DangerZoneHackerTracker.Models
{
	class DatabaseConnection : SQLiteConnection
	{
		public DatabaseConnection() : base(Constants.DatabasePath, Constants.Flags)
		{

		}
	}
}
