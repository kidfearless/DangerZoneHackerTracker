using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace DangerZoneHackerTracker.Models
{
	class User
	{
		public int Index;
		public string Name;
		public SteamID SteamID;
		public Cheater Cheater;
		public bool Alerted;
		internal bool Diconnected;
		private Grid grid;

		public Grid Grid
		{
			get => grid;
			set => grid = value;
		}
	}

}
