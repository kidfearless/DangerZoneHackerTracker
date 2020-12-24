using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DangerZoneHackerTracker
{
	class Steam
	{
		public static async Task<dynamic> GetProfileDataAsync(string url)
		{
			var xmlText = await WebReader.ReadToEndAsync(url);

			// Parse xml into object
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlText);

			// parse xml object as json text
			string json = JsonConvert.SerializeXmlNode(doc);
			// parse the text as a dynamic object
			return JsonConvert.DeserializeObject<ExpandoObject>(json);
		}

		public static  dynamic GetProfileData(string url)
		{
			var xmlText = WebReader.ReadToEnd(url);

			// Parse xml into object
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlText);

			// parse xml object as json text
			string json = JsonConvert.SerializeXmlNode(doc);
			// parse the text as a dynamic object
			return JsonConvert.DeserializeObject<ExpandoObject>(json);
		}
	}
}
