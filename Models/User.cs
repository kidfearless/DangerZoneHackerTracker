using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
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
		public List<FrameworkElement> Elements = new List<FrameworkElement>();
		public Image Image;

		~User()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			foreach (var element in Elements)
			{
				var parent = (Grid)element.Parent;
				parent?.Dispatcher.Invoke(() =>
				{
					parent.Children.Remove(element);
				});
			};
			if(Image != null)
			{
				var parent = (Grid)Image.Parent;
				parent?.Dispatcher.Invoke(() =>
				{
					parent.Children.Remove(Image);
				});
			}
		}
	}

}
