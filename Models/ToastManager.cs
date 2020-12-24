using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DangerZoneHackerTracker
{
	class ToastManager
	{
		public static async void ShowToastAsync(string title = "", string message = "", NotificationType type = NotificationType.Error, TimeSpan? duration = null)
		{
			var notificationManager = new NotificationManager();
			await notificationManager.ShowAsync(new NotificationContent()
			{
				Title = title,
				Message = message,
				Type = type
			}, expirationTime: duration);
		}
	}
}
