using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DangerZoneHackerTracker
{
	class CheaterComparer : IEqualityComparer<Cheater>
	{
		public bool Equals(Cheater x, Cheater y) => x.AccountID == y.AccountID;
		public int GetHashCode([DisallowNull] Cheater obj) => obj.AccountID.GetHashCode();
	}

	public class CheaterSet : HashSet<Cheater>
	{
		private static readonly string path = InitPath();
		private static CheaterSet _cheaters;


		private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
		{
			ContractResolver = new SettingsResolver()
		};


		public CheaterSet() : base(new CheaterComparer())
		{
		}

		// C# doesn't treat static classes like objects but rather like namespaces with variables inside them.
		// So to get static class objects we create a single instance of the object and return it in this function.
		public static CheaterSet Init()
		{
			// we've already created one so we want all future instances to just be a reference to the original.
			// that way we don't differing values between instances.
			if (_cheaters is not null)
			{
				return _cheaters;
			}
			using var reader = new StreamReader(path);
			var jsonString = reader.ReadToEnd();

			_cheaters = JsonConvert.DeserializeObject<CheaterSet>(jsonString, JsonSettings) ?? new CheaterSet();

			return _cheaters;
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

			var path = Path.Combine(folder, "cheaters.json");
			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}
			return path;
		}

		// Originally I used a sqlite database to store this data, but that made it prone to errors
		// if I needed to modify the table in any way. So now we'll store it in memory and keep a copy
		// syncronized with the file. If this game ever gets bad enough where we have thousands of cheaters in our
		// list then I'll need to move back to something more efficient.
		public void Save()
		{
			var jsonString = JsonConvert.SerializeObject(this, Formatting.Indented, JsonSettings);
			using var writer = new StreamWriter(path);
			writer.Write(jsonString);
			writer.Flush();
		}

		public new bool Add(Cheater item)
		{
			var result = base.Add(item);
			Save();
			return result;
		}

		public new void ExceptWith(IEnumerable<Cheater> other)
		{
			base.ExceptWith(other);
			
			Save();
		}

		public new void IntersectWith(IEnumerable<Cheater> other)
		{
			base.IntersectWith(other);
			Save();
		}

		public new void SymmetricExceptWith(IEnumerable<Cheater> other)
		{
			base.SymmetricExceptWith(other);
			Save();
		}

		public new void UnionWith(IEnumerable<Cheater> other)
		{
			base.UnionWith(other);
			Save();
		}

		public new void Clear()
		{
			base.Clear();
			Save();
		}

		public new bool Remove(Cheater item)
		{
			var result = base.Remove(item);
			Save();
			return result;
		}
	}
}
