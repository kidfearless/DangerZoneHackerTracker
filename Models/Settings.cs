using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Animation;

namespace DangerZoneHackerTracker
{

	public class Settings : Dictionary<string, string>
	{
		private static readonly string path = InitPath();
		/// <summary>
		/// Called whenever a setting is changed in the application.
		/// </summary>
		/// <param name="setting">Name of the setting that's being changed</param>
		/// <param name="oldValue">Reference to the original settings convar</param>
		/// <param name="newValue">New value casted to a settings convar</param>
		public delegate void SettingChangedCallback(string setting, Dynamic oldValue, Dynamic newValue);
		public event SettingChangedCallback SettingChanged;
		private static Settings _settings;

		private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
		{
			ContractResolver = new SettingsResolver()
		};


		public new Dynamic this[string s]
		{
			get
			{
				if (!this.ContainsKey(s))
				{
					return "";
				}
				var temp = (Dynamic)base[s];
				return temp;
			}
			set
			{
				if(base.ContainsKey(s) && base[s] != value)
				{
					var newValue = value;
					var oldValue = base[s];
					SettingChanged?.Invoke(s, oldValue, newValue);
				}

				base[s] = value;
				Save();
			}
		}



		private Settings(Dictionary<string, string> dictionary) : base(dictionary)
		{
		}

		public Settings()
		{
		}

		public static Settings Init()
		{
			// we've already created one so we want all future instances to just be a reference to the original.
			// that way we don't differing values between instances.
			if(_settings is not null)
			{
				return _settings;
			}
			var reader = new StreamReader(path);
			var jsonString = reader.ReadToEnd();
			reader.Dispose();

			var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
			
			_settings = dict is null? new Settings() : new Settings(dict);

			_settings.AddIfMissing("UpdateRate", 15.0);
			_settings.AddIfMissing("Volume", 1.0);

			return _settings;
		} 

		private static string InitPath() 
		{
			var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
			var folder = Path.Combine(basePath, appName);
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			var path = Path.Combine(folder, "config.json");
			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}
			return path;
		}

		private void Save()
		{
			var jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
			using var writer = new StreamWriter(path);
			writer.Write(jsonString);
			writer.Flush();
		}

		/// <summary>
		/// Adds the key to the dictionary if it doesn't exist.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns>true if the key was added. false if it already exists</returns>
		public bool AddIfMissing(string key, Dynamic value)
		{
			if(this.ContainsKey(key))
			{
				return false;
			}
			else
			{
				this[key] = value;
				return true;
			}
		}
	}
}
