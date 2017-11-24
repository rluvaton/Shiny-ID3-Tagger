//-----------------------------------------------------------------------
// <copyright file="GetTags_QQ.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from qq.com API for current track</summary>
// https://github.com/LIU9293/musicAPI/blob/master/src/qq.js
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
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
		private async Task<Id3> GetTags_QQ(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "QQ";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);
			
			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("https://c.y.qq.com/soso/fcgi-bin/search_cp?w=" + searchTermEnc + "&format=json&p=0&n=100&aggr=1&lossless=1&cr=1");
			
			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());			
			Debug.WriteLine(data1);
			if (data1 != null && data1.SelectToken("data.song.list[0]") != null)
			{
				JToken album = data1.SelectToken("data.song.list[0]");
				
				// TODO only 24 album hits where gracenote has 73 in Pop folder
				foreach (JToken grpAlbum in data1.SelectToken("data.song.list[0].grp"))
				{
					if ((long)grpAlbum["pubtime"] > 0 && (long)grpAlbum["pubtime"] <= (long)album["pubtime"])
					{
						album = grpAlbum;
					}
				}
				
				o.Title = (string)album["songname"];
				
				// ###########################################################################				
				request = new HttpRequestMessage();
				request.RequestUri = new Uri("https://c.y.qq.com/v8/fcg-bin/fcg_v8_album_info_cp.fcg?format=json&albumid=" + album["albumid"]);
				
				string content2 = await this.GetRequest(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());
				
				o.Artist = (string)data2.SelectToken("data.singername");
				o.Album = (string)data2.SelectToken("data.name");
				o.Date = (string)data2.SelectToken("data.aDate");
				
				if (data2.SelectToken("data.genre") != null)
				{
					string genre = (string)data2.SelectToken("data.genre");
					o.Genre = Regex.Replace(genre, " [\u4e00-\u9fa5]+$", string.Empty, RegexOptions.IgnoreCase);
				}
					
				o.TrackNumber = null;
				o.TrackCount = (string)data2.SelectToken("data.total");
				o.DiscNumber = null;
				o.DiscCount = null;
				o.Cover = "https://y.gtimg.cn/music/photo_new/T002R500x500M000" + (string)data2.SelectToken("data.mid") + ".jpg";
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