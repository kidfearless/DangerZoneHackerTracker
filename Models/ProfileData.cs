using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DangerZoneHackerTracker.ProfileData
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class Xml
	{
		[JsonProperty("@version")]
		public string Version { get; set; }
		[JsonProperty("@encoding")]
		public string Encoding { get; set; }
		[JsonProperty("@standalone")]
		public string Standalone { get; set; }
	}

	public class CDataString
	{
		public static implicit operator string(CDataString obj) => obj.CdataSection;
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }

		public override string ToString() => CdataSection;
	}

	public class PrivacyMessage
	{
		[JsonProperty("#text")]
		public string[] Text { get; set; }
		public static implicit operator string(PrivacyMessage obj) => string.Join('\n', obj?.Text ?? new string[] { "" });

		public override string ToString() => this;
	}

	public class InGameInfo
	{
		public CDataString gameName { get; set; }
		public CDataString gameLink { get; set; }
		public CDataString gameIcon { get; set; }
		public CDataString gameLogo { get; set; }
		public CDataString gameLogoSmall { get; set; }
	}



	public class MostPlayedGamesContainer
	{
		public IEnumerable<MostPlayedGame> MostPlayedGames
		{
			get
			{
				if (mostPlayedGame is JArray arr)
				{
					var array = arr.ToObject<List<MostPlayedGame>>();
					foreach (MostPlayedGame item in array)
					{
						yield return item;
					}
				}
				else if (mostPlayedGame is JObject obj)
				{
					yield return obj.ToObject<MostPlayedGame>();
				}
				yield break;
			}

		}

		public dynamic mostPlayedGame { get; set; }
	}

	public class MostPlayedGame
	{
		public CDataString gameName { get; set; }
		public CDataString gameLink { get; set; }
		public CDataString gameIcon { get; set; }
		public CDataString gameLogo { get; set; }
		public CDataString gameLogoSmall { get; set; }
		public string hoursPlayed { get; set; }
		public string hoursOnRecord { get; set; }
		public CDataString statsName { get; set; }
	}

	public class Group
	{
		[JsonProperty("@isPrimary")]
		public string IsPrimary { get; set; }
		public string groupID64 { get; set; }
		public CDataString groupName { get; set; }
		public CDataString groupURL { get; set; }
		public CDataString headline { get; set; }
		public CDataString summary { get; set; }
		public CDataString avatarIcon { get; set; }
		public CDataString avatarMedium { get; set; }
		public CDataString avatarFull { get; set; }
		public string memberCount { get; set; }
		public string membersInChat { get; set; }
		public string membersInGame { get; set; }
		public string membersOnline { get; set; }
	}

	[JsonArray]
	public class Groups
	{
		[JsonProperty("group")]

		public List<Group> group { get; set; }
	}

	public class Profile
	{
		public string steamID64 { get; set; }
		public CDataString steamID { get; set; }
		public string onlineState { get; set; }
		public CDataString stateMessage { get; set; }
		public string privacyState { get; set; }
		public string visibilityState { get; set; }
		public CDataString avatarIcon { get; set; }
		public CDataString avatarMedium { get; set; }
		public CDataString avatarFull { get; set; }
		public string vacBanned { get; set; }
		public string tradeBanState { get; set; }
		public string isLimitedAccount { get; set; }
		public CDataString customURL { get; set; }
		public string inGameServerIP { get; set; }
		public CDataString inGameInfo { get; set; }
		public string memberSince { get; set; }
		public string steamRating { get; set; }
		public string hoursPlayed2Wk { get; set; }
		public CDataString headline { get; set; }
		public CDataString location { get; set; }
		public CDataString realname { get; set; }
		public CDataString summary { get; set; }
		public PrivacyMessage privacyMessage { get; set; }

		public MostPlayedGamesContainer mostPlayedGames { get; set; }
		//public Groups groups { get; set; }
	}

	public static class ProfileExtensions
	{
		public static MostPlayedGame GetCSGO([System.Diagnostics.CodeAnalysis.AllowNull] this Profile profile)
		{
			if (profile is null || profile.mostPlayedGames is null)
			{
				return null;
			}

			return profile.mostPlayedGames.MostPlayedGames.FirstOrDefault(t => t.gameName == "Counter-Strike: Global Offensive");
		}
	}

	public class Root
	{
		[JsonProperty("?xml")]
		public Xml Xml { get; set; }
		public Profile profile { get; set; }
	}
}
