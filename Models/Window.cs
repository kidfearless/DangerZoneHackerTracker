using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DangerZoneHackerTracker.Models
{
	public static class Window
	{
		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		public static string GetActiveWindowTitle()
		{
			const int nChars = 256;
			StringBuilder Buff = new StringBuilder(nChars);
			IntPtr handle = GetForegroundWindow();

			if (GetWindowText(handle, Buff, nChars) > 0)
			{
				return Buff.ToString();
			}
			return String.Empty;
		}
	}
}
