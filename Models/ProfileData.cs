using Newtonsoft.Json;
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

	public class SteamID
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(SteamID obj) => obj.CdataSection;

	}

	public class StateMessage
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }

		public static implicit operator string(StateMessage obj) => obj.CdataSection;
	}

	public class AvatarIcon
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(AvatarIcon obj) => obj.CdataSection;

	}

	public class AvatarMedium
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(AvatarMedium obj) => obj.CdataSection;

	}

	public class AvatarFull
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(AvatarFull obj) => obj.CdataSection;

	}

	public class CustomURL
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(CustomURL obj) => obj.CdataSection;

	}

	public class GameName
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameName obj) => obj.CdataSection;

	}

	public class GameLink
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameLink obj) => obj.CdataSection;

	}

	public class GameIcon
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameIcon obj) => obj.CdataSection;

	}

	public class GameLogo
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameLogo obj) => obj.CdataSection;

	}

	public class GameLogoSmall
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameLogoSmall obj) => obj.CdataSection;

	}

	public class InGameInfo
	{
		public GameName gameName { get; set; }
		public GameLink gameLink { get; set; }
		public GameIcon gameIcon { get; set; }
		public GameLogo gameLogo { get; set; }
		public GameLogoSmall gameLogoSmall { get; set; }
	}

	public class Headline
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(Headline obj) => obj.CdataSection;

	}

	public class Location
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(Location obj) => obj.CdataSection;

	}

	public class Realname
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(Realname obj) => obj.CdataSection;

	}

	public class Summary
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(Summary obj) => obj.CdataSection;

	}

	public class GameName2
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameName2 obj) => obj.CdataSection;

	}

	public class GameLink2
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameLink2 obj) => obj.CdataSection;

	}

	public class GameIcon2
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameIcon2 obj) => obj.CdataSection;

	}

	public class GameLogo2
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameLogo2 obj) => obj.CdataSection;
	}

	public class GameLogoSmall2
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(GameLogoSmall2 obj) => obj.CdataSection;
	}

	public class StatsName
	{
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
		public static implicit operator string(StatsName obj) => obj.CdataSection;
	}

	public class MostPlayedGame
	{
		public GameName2 gameName { get; set; }
		public GameLink2 gameLink { get; set; }
		public GameIcon2 gameIcon { get; set; }
		public GameLogo2 gameLogo { get; set; }
		public GameLogoSmall2 gameLogoSmall { get; set; }
		public string hoursPlayed { get; set; }
		public string hoursOnRecord { get; set; }
		public StatsName statsName { get; set; }
	}

	[JsonArray]
	public class MostPlayedGames
	{
		public List<MostPlayedGame> mostPlayedGame { get; set; }
	}

	public class GroupName
	{
		public static implicit operator string(GroupName obj) => obj.CdataSection;
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
	}

	public class GroupURL
	{
		public static implicit operator string(GroupURL obj) => obj.CdataSection;
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
	}

	public class Headline2
	{
		public static implicit operator string(Headline2 obj) => obj.CdataSection;
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
	}

	public class Summary2
	{
		public static implicit operator string(Summary2 obj) => obj.CdataSection;
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
	}

	public class AvatarIcon2
	{
		public static implicit operator string(AvatarIcon2 obj) => obj.CdataSection;
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
	}

	public class AvatarMedium2
	{
		public static implicit operator string(AvatarMedium2 obj) => obj.CdataSection;
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
	}

	public class AvatarFull2
	{
		public static implicit operator string(AvatarFull2 obj) => obj.CdataSection;
		[JsonProperty("#cdata-section")]
		public string CdataSection { get; set; }
	}

	public class Group
	{
		[JsonProperty("@isPrimary")]
		public string IsPrimary { get; set; }
		public string groupID64 { get; set; }
		public GroupName groupName { get; set; }
		public GroupURL groupURL { get; set; }
		public Headline2 headline { get; set; }
		public Summary2 summary { get; set; }
		public AvatarIcon2 avatarIcon { get; set; }
		public AvatarMedium2 avatarMedium { get; set; }
		public AvatarFull2 avatarFull { get; set; }
		public string memberCount { get; set; }
		public string membersInChat { get; set; }
		public string membersInGame { get; set; }
		public string membersOnline { get; set; }
	}

	[JsonArray]
	public class Groups
	{
		public List<Group> group { get; set; }
	}

	public class Profile
	{
		public string steamID64 { get; set; }
		public SteamID steamID { get; set; }
		public string onlineState { get; set; }
		public StateMessage stateMessage { get; set; }
		public string privacyState { get; set; }
		public string visibilityState { get; set; }
		public AvatarIcon avatarIcon { get; set; }
		public AvatarMedium avatarMedium { get; set; }
		public AvatarFull avatarFull { get; set; }
		public string vacBanned { get; set; }
		public string tradeBanState { get; set; }
		public string isLimitedAccount { get; set; }
		public CustomURL customURL { get; set; }
		public string inGameServerIP { get; set; }
		public InGameInfo inGameInfo { get; set; }
		public string memberSince { get; set; }
		public string steamRating { get; set; }
		public string hoursPlayed2Wk { get; set; }
		public Headline headline { get; set; }
		public Location location { get; set; }
		public Realname realname { get; set; }
		public Summary summary { get; set; }
		//public MostPlayedGames mostPlayedGames { get; set; }
		//public Groups groups { get; set; }
	}

	public class Root
	{
		[JsonProperty("?xml")]
		public Xml Xml { get; set; }
		public Profile profile { get; set; }
	}
}
