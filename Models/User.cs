using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace DangerZoneHackerTracker
{
	class UserComparer : IEqualityComparer<User>, IComparer<User>
	{
		// index sorting
		public int Compare(User x, User y) => x.Index.CompareTo(y.Index);

		// uniqueness identifier
		public bool Equals(User x, User y) => x.AccountID == y.AccountID;
		public int GetHashCode([DisallowNull] User obj) => obj.AccountID.GetHashCode();
	}

	public class User : IComparable
	{
		public delegate void ProfileRetreivedCallback(User user, ProfileData.Profile profile);
		public event ProfileRetreivedCallback ProfileRetreived;

		public int Index { get; set; }

		[DefaultValue("")]
		public string Name { get; set; }


		public ulong AccountID => SteamID.ConvertToUInt64();

		public SteamID SteamID { get; set; }


		public Cheater? Cheater { get; set; }
		public bool IsCheater => Cheater != null;

		public ProfileData.Profile Profile { get; set; }

		public Image ProfilePicture { get; set; }

		public int CompareTo(object obj)
		{
			if(obj is User other)
			{
				return this.Index.CompareTo(other.Index);
			}
			throw new ArgumentException($"object {obj}[{obj.GetType()} is not comparable to User]");
		}

		public sealed class UIData
		{
			internal TextBox CheatList;
			internal TextBox ThreatLevel;
			internal TextBox Submitter;
			internal TextBox Notes;
			internal Label Name;
			internal Label SteamID;
			internal Rectangle BackgroundRect;
			internal Button Button;
		}

		public UIData UI = new();

		public async void GetProfileData()
		{
			var url = $"http://steamcommunity.com/profiles/{AccountID}/?xml=1";
			try
			{
				this.Profile = await Steam.GetProfileDataAsync(url);
				this.ProfileRetreived?.Invoke(this, this.Profile);
			}
			// sometimes it just fails for no reason. so we just try again.
			catch
			{
				try
				{
					this.Profile = await Steam.GetProfileDataAsync(url);
					this.ProfileRetreived?.Invoke(this, this.Profile);
				}
				catch
				{

				}
			}
		}
	}
}
