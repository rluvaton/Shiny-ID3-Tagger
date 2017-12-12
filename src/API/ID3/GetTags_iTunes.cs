﻿//-----------------------------------------------------------------------
// <copyright file="GetTags_Itunes.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from iTunes API for current track</summary>
// https://www.apple.com/itunes/affiliates/resources/documentation/itunes-store-web-service-search-api.html
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_Itunes(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "iTunes";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://itunes.apple.com/search?term=" + searchTermEnc + "&media=music&limit=1");

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = this.DeserializeJson(searchContent);

				if (searchData != null && searchData.SelectToken("results") != null)
				{
					o.Artist = (string)searchData.SelectToken("results[0].artistName");
					o.Title = (string)searchData.SelectToken("results[0].trackName");
					o.Album = (string)searchData.SelectToken("results[0].collectionName");
					o.Date = (string)searchData.SelectToken("results[0].releaseDate");
					o.Genre = (string)searchData.SelectToken("results[0].primaryGenreName");
					o.DiscCount = (string)searchData.SelectToken("results[0].discCount");
					o.DiscNumber = (string)searchData.SelectToken("results[0].discNumber");
					o.TrackCount = (string)searchData.SelectToken("results[0].trackCount");
					o.TrackNumber = (string)searchData.SelectToken("results[0].trackNumber");
					o.Cover = (string)searchData.SelectToken("results[0].artworkUrl100");
					if (o.Cover != null)
					{
						o.Cover = o.Cover.Replace("100x100", "600x600");
					}
				}
			}

			// ###########################################################################
			sw.Stop();
			o.Duration = string.Format("{0:s\\,f}", sw.Elapsed);

			return o;
		}
	}
}
