using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Devices.Portable;

namespace DangerZoneHackerTracker.Models
{
	class Steam
	{
		public struct ProfileData
		{
			public string Avatar;
			public SteamID SteamID;
			public string PersonaName;
			public string Summary;
			public bool VACBanned;
			public string TradeBanState;
		}

		public static bool TryGetProfileData(string steamID, out ProfileData profileData)
		{
			profileData = new ProfileData();
			string url;
			var steam = new SteamID(0);
			if (!SteamID.CommunityURLRegex.IsMatch(steamID))
			{
				if (!steam.SetFromString(steamID) && !steam.SetFromSteam3String(steamID))
				{
					return false;
				}
				url = $"https://steamcommunity.com/profiles/{steam.ConvertToUInt64()}/?xml=1";
			}
			else
			{
				url = $"{SteamID.CommunityURLRegex.Match(steamID).Value}/?xml=1";
			}

			try
			{
				profileData = GetProfileData(url);
				return true;
			}
			catch
			{
			}
			return false;
		}

		// Can't have out/ref parameters in async functions. So we return a tuple with the result and profile data.
		public static async Task<(bool Result, ProfileData ProfileData)> TryGetProfileDataAsync(string steamID)
		{
			(bool Result, ProfileData ProfileData) returnValue = (false, new ProfileData());

			string url;
			var steam = new SteamID(0);
			if (!SteamID.CommunityURLRegex.IsMatch(steamID))
			{
				if (!steam.SetFromString(steamID) && !steam.SetFromSteam3String(steamID))
				{
					return returnValue;
				}
				url = $"https://steamcommunity.com/profiles/{steam.ConvertToUInt64()}/?xml=1";
			}
			else
			{
				url = $"{SteamID.CommunityURLRegex.Match(steamID).Value}/?xml=1";
			}

			try
			{
				returnValue.ProfileData = await GetProfileDataAsync(url);
				returnValue.Result = true;
				return returnValue;
			}
			catch
			{
			}
			return returnValue;
		}

		private static async Task<ProfileData> GetProfileDataAsync(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			var response = (HttpWebResponse)await request.GetResponseAsync();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				return InitializeProfileData(response);
			}

			throw new Exception($"Could not find profile data from the given url: {url}");
		}
		private static ProfileData GetProfileData(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			var response = (HttpWebResponse)request.GetResponse();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				return InitializeProfileData(response);
			}

			throw new Exception($"Could not find profile data from the given url: {url}");
		}

		private static ProfileData InitializeProfileData(HttpWebResponse response)
		{
			using Stream receiveStream = response.GetResponseStream();
			using StreamReader readStream = new StreamReader(receiveStream);

			string line = readStream.ReadToEnd();
			// returns a dynamic object with properties that follow the xml tree.

			var serializer = new XmlSerializer(typeof(SteamUser));
			SteamUser profile = (SteamUser)serializer.Deserialize(new StringReader(line));
			ProfileData data = new ProfileData
			{
				PersonaName = profile.PersonaName,
				SteamID = new SteamID(profile.SteamId64),
				Summary = profile.Summary ?? "",
				TradeBanState = profile.TradeBanState,
				VACBanned = profile.VacBanned != 0,
				Avatar = profile.AvatarFull
			};

			return data;
		}
	}
}
