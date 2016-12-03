//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Baidu.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves SYNCED lyrics from baidu.com</summary>
// http://apistore.baidu.com/apiworks/servicedetail/1020.html
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<string> GetLyrics_Baidu(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			if (tagNew.Artist != null && tagNew.Title != null)
			{
				HttpRequestMessage request = new HttpRequestMessage();
				request.Headers.Add("apikey", User.Accounts["BaApiKey"]);
				request.RequestUri = new Uri("http://apis.baidu.com/geekery/music/query?s=" + tagNew.Artist + " - " + tagNew.Title + "&size=1");
				
				string content = await this.GetRequest(client, request, cancelToken);
				
				if (content.StartsWith("{", StringComparison.Ordinal) && content.EndsWith("}", StringComparison.Ordinal))
				{
					JObject data = JsonConvert.DeserializeObject<JObject>(content, this.GetJsonSettings());

					if (data != null && data.SelectToken("data.data") != null)
					{
						string name = (string)data.SelectToken("data.data[0].filename");
						string hash = (string)data.SelectToken("data.data[0].hash");
						string time = (string)data.SelectToken("data.data[0].duration");
						
						request = new HttpRequestMessage();
						request.Headers.Add("apikey", User.Accounts["BaApiKey"]);
						request.RequestUri = new Uri("http://apis.baidu.com/geekery/music/krc?name=" + name + "&hash=" + hash + "&time=" + time);
						
						string content2 = await this.GetRequest(client, request, cancelToken);
	
						if (content2.StartsWith("{", StringComparison.Ordinal) && content2.EndsWith("}", StringComparison.Ordinal))
						{						
							JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());
						
							if (data2 != null && data2.SelectToken("data.content") != null)
							{
								string response = (string)data2.SelectToken("data.content");						
								
								const string RegExPattern = @"\[\d\d:\d\d\.\d\d]";
								response = Regex.Replace(response, RegExPattern, string.Empty);	
								
								if (!string.IsNullOrWhiteSpace(response) && response.Trim() != "听音乐，找酷狗")
								{
									tagNew.Lyrics = response;
									this.Log("search", new[] { "  Lyrics taken from Baidu" });
								}	
							}
						}
					}
				}
				
				request.Dispose();
			}

			return tagNew.Lyrics;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content);