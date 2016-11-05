//-----------------------------------------------------------------------
// <copyright file="GetTags_LastFm.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Last.fm API for current track</summary>
// http://www.last.fm/api/rest
// http://www.last.fm/api/show/track.getInfo
// http://www.dennisdel.com/?p=24
// http://stackoverflow.com/questions/21956358/can-anyone-provide-a-code-example-for-accessing-this-last-fm-api
// http://geekswithblogs.net/THines01/archive/2010/12/03/responsestatusline.aspx
// limit=1 not available for track.getInfo or album.getInfo method
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
		private async Task<Id3> GetTags_LastFm(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Last.fm";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			string artistEnc = WebUtility.UrlEncode(artist);
			string titleEnc = WebUtility.UrlEncode(title);			
			
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://ws.audioscrobbler.com/2.0/");
			request.Headers.ExpectContinue = false;
			request.Content = new StringContent("api_key=" + User.Accounts["LaApiKey"] +
				"&format=json&method=track.getInfo&artist=" + artistEnc + "&track=" + titleEnc + "&autocorrect=1");

			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("track") != null)
			{
				o.Artist = (string)data1.SelectToken("track.artist.name");
				o.Title = (string)data1.SelectToken("track.name");
				o.Album = (string)data1.SelectToken("track.album.title");
				o.Genre = (string)data1.SelectToken("track.toptags.tag[0].name");
				o.TrackNumber = (string)data1.SelectToken("track.album.@attr.position");

				// ###########################################################################
				string albumid = (string)data1.SelectToken("track.album.mbid");

				request = new HttpRequestMessage(HttpMethod.Post, "https://ws.audioscrobbler.com/2.0/");
				request.Headers.ExpectContinue = false;
				request.Content = new StringContent("api_key=" + User.Accounts["LaApiKey"] +
					"&format=json&method=album.getInfo&mbid=" + albumid);

				string content2 = await this.GetRequest(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null && data2.SelectToken("album") != null)
				{
					// documentation says "releasedate" property exists. But I have never seen it in reality
					o.Date = (string)data2.SelectToken("album.releasedate");		
					o.DiscCount = null;
					o.DiscNumber = null;
					o.TrackCount = (string)data2.SelectToken("album.tracks.track[-1:].@attr.rank");					
					o.Cover = (string)data2.SelectToken("album.image[-1:].#text");
					if (o.Cover != null)
					{
						o.Cover = o.Cover.Replace("http://img2-ak.lst.fm/i/u/arQ", "http://img2-ak.lst.fm/i/u");
					}
				}
			}

			// ###########################################################################
			sw.Stop();
			o.Duration = string.Format("{0:s\\,f}", sw.Elapsed);

			request.Dispose();
			return o;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content2);