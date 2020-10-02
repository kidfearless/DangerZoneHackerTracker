using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DangerZoneHackerTracker.Models
{
	[XmlRoot("memberList")]
	public class SteamGroup
	{
		[XmlElement("groupID64")]
		public ulong GroupID;
		[XmlElement("groupDetails")]
		public GroupDetails groupDetails;
		[XmlElement("memberCount")]
		public int MemberCount;
		[XmlElement("totalPages")]
		public int TotalPages;
		[XmlElement("currentPage")]
		public int CurrentPage;
		[XmlElement("startingMember")]
		public int StartingMember;
		[XmlElement("members")]
		public Members Members;

		public static void Run()
		{
			var text = File.ReadAllText(@"D:\pe\Downloads\memberslistxml.xml");
			var stream = File.Open(@"D:\pe\Downloads\memberslistxml.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new StreamReader(stream);
			XmlSerializer serializer = new XmlSerializer(typeof(SteamGroup));
			var result = (SteamGroup)serializer.Deserialize(reader);
		}
	}
	public struct GroupDetails
	{
		[XmlElement("groupName")]
		public string GroupName;
		[XmlElement("groupURL")]
		public string GroupUrl;
		[XmlElement("headline")]
		public string Headline;
		[XmlElement("summary")]
		public string Summary;
		[XmlElement("avatarIcon")]
		public string AvatarIcon;
		[XmlElement("avatarMedium")]
		public string AvatarMedium;
		[XmlElement("avatarFull")]
		public string AvatarFull;
		[XmlElement("memberCount")]
		public int MemberCount;
		[XmlElement("membersInChat")]
		public int MembersInChat;
		[XmlElement("membersInGame")]
		public int MembersInGame;
		[XmlElement("membersOnline")]
		public int MembersOnline;
	};
	public class Members
	{
		[XmlElement("steamID64")]
		public List<string> SteamID64;
	}
}
