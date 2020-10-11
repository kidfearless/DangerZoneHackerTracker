using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DangerZoneHackerTracker.Models
{
	[XmlRoot("profile")]
	public class SteamUser
	{
		[XmlElement("steamID64")]
		public ulong SteamId64 { get; set; }

		[XmlElement("steamID")]
		public string PersonaName { get; set; }

		[XmlElement("onlineState")]
		public string OnlineState { get; set; }

		[XmlElement("stateMessage")]
		public string StateMessage { get; set; }

		[XmlElement("privacyState")]
		public string PrivacyState { get; set; }

		[XmlElement("visibilityState")]
		public byte VisibilityState { get; set; }

		[XmlElement("avatarIcon")]
		public string AvatarIcon { get; set; }

		[XmlElement("avatarMedium")]
		public string AvatarMedium { get; set; }

		[XmlElement("avatarFull")]
		public string AvatarFull { get; set; }

		[XmlElement("vacBanned")]
		public byte VacBanned { get; set; }

		[XmlElement("tradeBanState")]
		public string TradeBanState { get; set; }

		[XmlElement("isLimitedAccount")]
		public byte IsLimitedAccount { get; set; }
		
		[XmlElement("summary")]
		public string Summary { get; set; }
	}
}
