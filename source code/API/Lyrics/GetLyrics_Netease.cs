﻿//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Netease.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from Netease</summary>
// https://github.com/JounQin/netease-muisc-api/blob/master/api/lyric.js
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetLyrics_Netease(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Netease";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			if (tagNew.Artist != null && tagNew.Title != null)
			{
				using (HttpRequestMessage searchRequest = new HttpRequestMessage())
				{
					searchRequest.Method = HttpMethod.Post;
					searchRequest.RequestUri = new Uri("http://music.163.com/api/search/get/");
					searchRequest.Headers.Add("referer", "http://music.163.com");
					searchRequest.Headers.Add("Cookie", "appver=2.0.2");
					searchRequest.Content = new FormUrlEncodedContent(new[]
						{
							new KeyValuePair<string, string>("s", WebUtility.UrlEncode(tagNew.Artist + " - " + tagNew.Title)),
							new KeyValuePair<string, string>("type", "1")
						});

					string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
					JObject searchData = JsonConvert.DeserializeObject<JObject>(searchContent, this.GetJsonSettings());

					if (searchData != null)
					{
						// Check if any returned song artist and title match search parameters
						JToken song = (from track in searchData.SelectTokens("result.songs[*]")
									   where track.SelectToken("artists[0].name").ToString().ToLowerInvariant() == tagNew.Artist.ToLowerInvariant()
									   where track.SelectToken("name").ToString().ToLowerInvariant() == tagNew.Title.ToLowerInvariant()
									   select track).FirstOrDefault();

						if (song != null && song.SelectToken("id") != null)
						{
							string songid = (string)song.SelectToken("id");

							using (HttpRequestMessage lyricsRequest = new HttpRequestMessage())
							{
								lyricsRequest.Headers.Add("referer", "http://music.163.com");
								lyricsRequest.Headers.Add("Cookie", "appver=2.0.2");
								lyricsRequest.RequestUri = new Uri("http://music.163.com/api/song/lyric/?id=" + songid + "&lv=-1&kv=-1&tv=-1");

								string lyricsContent = await this.GetResponse(client, lyricsRequest, cancelToken);
								JObject lyricsData = JsonConvert.DeserializeObject<JObject>(lyricsContent, this.GetJsonSettings());

								if (lyricsData != null && lyricsData.SelectToken("lrc.lyric") != null)
								{
									string rawLyrics = (string)lyricsData.SelectToken("lrc.lyric");

									// Sanitize lyrics
									rawLyrics = Regex.Replace(rawLyrics, @"[\r\n]\[x-trans\].*", string.Empty);                 // Remove [x-trans] lines (Chinese translation)
									rawLyrics = Regex.Replace(rawLyrics, @"\[\d{2}:\d{2}(\.\d{2})?\]([\r\n])?", string.Empty);  // Remove timestamps like [01:01:123] or [01:01]
									rawLyrics = Regex.Replace(rawLyrics, @".*?[\u4E00-\u9FFF]+.*?[\r\n]", string.Empty);        // Remove lines where Chinese characters are. Most of time they are credits like [by: XYZ]
									rawLyrics = Regex.Replace(rawLyrics, @"\[.*?\]", string.Empty);                             // Remove square brackets [by: XYZ] credits
									rawLyrics = Regex.Replace(rawLyrics, @"<\d+>", string.Empty);                               // Remove angle brackets <123>. No idea for what they are. Example track is "ABBA - Gimme Gimme Gimme"
									rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));                 // Remove leading or ending white space per line
									rawLyrics = rawLyrics.Trim();                                                               // Remove leading or ending line breaks and white space

									if (rawLyrics.Length > 1)
									{
										o.Lyrics = rawLyrics;
									}
								}
							}
						}
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

// System.IO.File.WriteAllText (@"D:\response.json", content);