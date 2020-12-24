using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DangerZoneHackerTracker
{
	public class Cheater
	{
		public ulong AccountID; 

		public int ThreatLevel;

		[DefaultValue("")]
		public string CheatList;

		[DefaultValue("")]
		public string LastKnownName;

		[DefaultValue("")]
		public string Notes;

		[DefaultValue("")]
		public string Submitter;
	}
}
