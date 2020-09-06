using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace DangerZoneHackerTracker.Models
{
	class User : IDisposable
	{
		public int Index;
		public string Name;
		public SteamID SteamID;
		public Cheater Cheater;
		public bool Alerted;
		private Grid grid;

		public Grid Grid
		{
			get => grid;
			set => grid = value;
		}

		public void Dispose()
		{
			if(this.Grid != null)
			{
				var parent = (StackPanel)this.Grid.Parent;
				if(parent != null && parent.Children.Contains(this.Grid))
				{
					parent.Children.Remove(this.Grid);
				}
			}
		}
	}

}
