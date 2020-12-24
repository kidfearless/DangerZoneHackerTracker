using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DangerZoneHackerTracker
{
	public static class WebReader
	{
		public static async Task<string> ReadToEndAsync(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			using var response = await request.GetResponseAsync() as HttpWebResponse;

			if (response.StatusCode != HttpStatusCode.OK)
			{
				return null;
			}

			Stream receiveStream = response.GetResponseStream();
			using StreamReader readStream = new StreamReader(receiveStream, detectEncodingFromByteOrderMarks: true);


			string line = await readStream.ReadToEndAsync();
			return line;
		}

		public static string ReadToEnd(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			using var response = request.GetResponse() as HttpWebResponse;

			if (response.StatusCode != HttpStatusCode.OK)
			{
				return null;
			}

			Stream receiveStream = response.GetResponseStream();
			using StreamReader readStream = new StreamReader(receiveStream, detectEncodingFromByteOrderMarks: true);


			string line = readStream.ReadToEnd();
			return line;
		}

		public static IEnumerable<string> ReadLines(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			using var response = request.GetResponse() as HttpWebResponse;

			if (response.StatusCode != HttpStatusCode.OK)
			{
				yield break;
			}

			Stream receiveStream = response.GetResponseStream();
			using StreamReader readStream = new StreamReader(receiveStream, detectEncodingFromByteOrderMarks: true);


			var line = readStream.ReadLine();
			while(line is not null)
			{
				yield return line;
				line = readStream.ReadLine();
			}
		}

		public static IEnumerable<string> ReadAllLines(string url)
		{
			return ReadToEnd(url).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		}

	}
}
