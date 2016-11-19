//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Lololyrics.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from lololyrics.com</summary>
// http://api.lololyrics.com/
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<string> GetLyrics_Lololyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			if (tagNew.Artist != null && tagNew.Title != null)
			{
				HttpRequestMessage request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.lololyrics.com/0.5/getLyric?artist=" + tagNew.Artist + "&track=" + tagNew.Title);
				
				string content = await this.GetRequest(client, request, cancelToken);
				
				string json = this.ConvertXmlToJson(content);
				JObject data = JsonConvert.DeserializeObject<JObject>(json, this.GetJsonSettings());

				if (data != null && data.SelectToken("result.response") != null)
				{
					string response = (string)data.SelectToken("result.response");
					if (!string.IsNullOrWhiteSpace(response))
					{
						response = System.Net.WebUtility.HtmlDecode(response);
						
						tagNew.Lyrics = response;
						this.Log("search", new[] { "  Lyrics taken from Lololyrics" });	
					}
				}
				
				request.Dispose();
			}
			
			return tagNew.Lyrics;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content);