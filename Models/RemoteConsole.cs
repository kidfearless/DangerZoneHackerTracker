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
		SystemTimer Timer;
		Settings Settings = Settings.Init();
		StreamReader Reader;
		public delegate void LineReadCallback(string line);
		public event LineReadCallback LineRead;
		public RemoteConsole(string server = "127.0.0.1", int port = 2121) : base(server, port)
		{
			this.Stream = base.GetStream();

			Timer = new SystemTimer(TimeSpan.FromSeconds(Settings["UpdateRate"]).TotalMilliseconds);
			Timer.Elapsed += Timer_Elapsed;
			Timer.Start();

			Settings.SettingChanged += Settings_SettingChanged;
			Reader = new StreamReader(Stream);
			new Thread(StreamReaderThread).Start();
		}

		private void Settings_SettingChanged(string setting, Dynamic oldValue, Dynamic newValue)
		{
			if(setting == "UpdateRate")
			{
				Timer.Interval = TimeSpan.FromSeconds(newValue).TotalMilliseconds;
			}
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				Execute("status");
			}
			catch (IOException)
			{
			}
		}

		private void StreamReaderThread()
		{
			while(Reader is not null)
			{
				try
				{
					foreach (var line in ReadLines())
					{
						LineRead?.Invoke(line);
					}
					Thread.Sleep(10);
				}
				// As far as i can tell there isn't a way to handle this without a try catch.
				catch (Exception)
				{
					return;
				}
			}
		}

		private IEnumerable<string> ReadLines()
		{
			var line = Reader?.ReadLine();
			while(line is not null)
			{
				yield return line;
				line = Reader?.ReadLine();
			}
		}



		/// <summary>
		/// Sends a command to the clients console and executes it. The console itself will not open.
		/// </summary>
		/// <param name="command">The command to send</param>
		public void Execute(string command)
		{
			var bytes = Encoding.Default.GetBytes(command + "\n");
			Stream.Write(bytes);
		}

		public new void Dispose()
		{
			base.Dispose();
			Timer?.Dispose();
			Timer = null;
			Reader?.Dispose();
			Reader = null;
		}
	}
}
