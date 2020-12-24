using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DangerZoneHackerTracker
{
	// allows us to pull private properties and fields when we handle json files
	public class SettingsResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{

			var list = base.CreateProperties(type, memberSerialization);

			var props = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).ToList();
			var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList();

			foreach (var item in props)
			{
				var prop = base.CreateProperty(item, memberSerialization);
				prop.Writable = true;
				prop.Readable = true;
				list.Add(prop);
			}
			foreach (var item in fields)
			{
				var prop = base.CreateProperty(item, memberSerialization);
				prop.Writable = true;
				prop.Readable = true;
				list.Add(prop);
			}


			return list;
		}
	}
}
