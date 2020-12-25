using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using ThreadTimer = System.Threading.Timer;
using SystemTimer = System.Timers.Timer;

namespace DangerZoneHackerTracker
{
	public class RemoteConsole : TcpClient
	{
		NetworkStream Stream;
		StreamReader Reader;
		SystemTimer Timer;
		Settings Settings = Settings.Init();
		public delegate void LineReadCallback(string line);
		public event LineReadCallback LineRead;
		public RemoteConsole(string server = "127.0.0.1", int port = 2121) : base(server, port)
		{
			this.Stream = base.GetStream();
			Reader = new StreamReader(Stream);

			Timer = new SystemTimer(TimeSpan.FromSeconds(Settings["UpdateRate"]).TotalMilliseconds);
			Timer.Elapsed += Timer_Elapsed;
			Timer.Start();

			Settings.SettingChanged += Settings_SettingChanged;
			new Thread(StreamReaderThread).Start();
		}

		private void Settings_SettingChanged(string setting, Dynamic oldValue, Dynamic newValue)
		{
			if(setting == "UpdateRate")
			{
				Timer.Interval = TimeSpan.FromSeconds(newValue).TotalMilliseconds;
			}
		}

#pragma warning disable CS0168 // Variable is declared but never used
		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				Execute("status");
			}
			catch (Exception exc)
			{
			}
		}
#pragma warning restore CS0168 // Variable is declared but never used

		private void StreamReaderThread()
		{
			while (Stream != null)
			{
				try
				{
					var lines = ReadLines();
					foreach (var line in lines)
					{
						System.Diagnostics.Debug.WriteLine(line);
						LineRead?.Invoke(line);
					}
					Thread.Sleep(10);
				}
				// As far as i can tell there isn't a way to handle the socket exception without a try catch.
				// it might just be that valve fucked it up on their end.
				catch (Exception)
				{
					return;
				}
			}
		}

		private string[] ReadAllLines()
		{
			List<string> list = new();
			while (Stream != null && Stream.DataAvailable && Stream.CanRead)
			{
				var line = Reader.ReadLine();
				if(line is not null)
				{
					list.Add(line);
				}
				else
				{
					break;
				}
			}

			return list.ToArray();
		}

		private IEnumerable<string> ReadLines()
		{
			while (Stream != null && Stream.DataAvailable && Stream.CanRead)
			{
				var line = Reader.ReadLine();
				if (line is not null)
				{
					yield return line;
				}
				else
				{
					yield break;
				}
			}
		}



		/// <summary>
		/// Sends a command to the clients console and executes it. The console itself will not open.
		/// </summary>
		/// <param name="command">The command to send</param>
		public void Execute(string command)
		{
			var bytes = Encoding.Default.GetBytes(command + "\n");
			Stream?.Write(bytes);
		}

		public new void Dispose()
		{
			base.Dispose();
			Timer?.Dispose();
			Timer = null;
			Stream?.Dispose();
			Stream = null;
			Reader?.Dispose();
			Reader = null;
		}
	}
}
