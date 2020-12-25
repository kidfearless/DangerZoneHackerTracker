using DangerZoneHackerTracker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DangerZoneHackerTracker
{
	class Steam
	{
		public const string DefaultProfilePicture = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg";
		public static async Task<ProfileData.Profile> GetProfileDataAsync(string url)
		{
			if(url == "http://steamcommunity.com/profiles/76561198798849865/?xml=1")
			{
				System.Diagnostics.Debug.Write("");
			}
			var xmlText = await WebReader.ReadToEndAsync(url);

			// Parse xml into object
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlText);

			// parse xml object as json text
			string json = JsonConvert.SerializeXmlNode(doc);

			var temp = JsonConvert.DeserializeObject<ProfileData.Root>(json);



			return temp.profile;
		}

		public static ProfileData.Profile GetProfileData(string url)
		{
			var xmlText = WebReader.ReadToEnd(url);

			// Parse xml into object
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlText);

			// parse xml object as json text
			string json = JsonConvert.SerializeXmlNode(doc);
			var temp = JsonConvert.DeserializeObject<ProfileData.Root>(json);

			return temp.profile;
		}
	}
}
