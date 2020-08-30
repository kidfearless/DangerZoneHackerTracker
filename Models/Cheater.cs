using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DangerZoneHackerTracker
{
	[Table("Cheaters")]
	public class Cheater
	{
		[PrimaryKey, AutoIncrement, Column(nameof(Id))]
		public int Id { get; set; }
		[Column(nameof(SteamID2))]

		public string SteamID2 { get; set; }
		[Column(nameof(ThreatLevel))]
		public int ThreatLevel { get; set; }
		[Column(nameof(CheatList))]
		public string CheatList { get; set; }
		[Column(nameof(LastKnownName))]
		public string LastKnownName { get; set; }
	}
}
