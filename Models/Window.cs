using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

		[DllImport("coredll.dll", SetLastError = true)]
		private static extern int GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

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

		public static string GetProcessLocation(string name)
		{
			var process = Process.GetProcessesByName(name).FirstOrDefault();
			return process?.MainModule.FileName;
		}

		public static Process GetActiveProcess()
		{
			IntPtr handle = GetForegroundWindow();
			uint pID;

			GetWindowThreadProcessId(handle, out pID);

			return Process.GetProcessById((Int32)pID);
		}

	}
}
