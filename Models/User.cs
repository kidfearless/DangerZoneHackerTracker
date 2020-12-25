using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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
	}
}
