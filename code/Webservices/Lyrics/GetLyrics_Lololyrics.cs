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
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
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
				string artistEnc = WebUtility.UrlEncode(tagNew.Artist);
				string titleEnc = WebUtility.UrlEncode(tagNew.Title);
				
				HttpRequestMessage request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.lololyrics.com/0.5/getLyric?artist=" + artistEnc + "&track=" + titleEnc + "&rawutf8=1");
				
				string content = await this.GetRequest(client, request, cancelToken);
				
				string json = this.ConvertXmlToJson(content);
				JObject data = JsonConvert.DeserializeObject<JObject>(json, this.GetJsonSettings());

				if (data != null && data.SelectToken("result.response") != null)
				{
					string rawLyrics = (string)data.SelectToken("result.response");
				
					//Sanitize lyrics
					rawLyrics = System.Net.WebUtility.HtmlDecode(rawLyrics);							// URL decode lyrics
					rawLyrics = rawLyrics.Trim('\r','n').Trim().Trim('\r','n');							// Remove leading or ending line breaks and white space
					
					if (rawLyrics.Length > 1)
					{
						tagNew.Lyrics = rawLyrics;
						this.PrintLogMessage("search", new[] { "  Lyrics taken from Xiami" });
						Debug.WriteLine("Lololyrics ################################################");
						Debug.WriteLine(tagNew.Lyrics);
					}
				}
				
				request.Dispose();
			}
			
			return tagNew.Lyrics;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content);