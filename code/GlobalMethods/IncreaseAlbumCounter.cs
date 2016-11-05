//-----------------------------------------------------------------------
// <copyright file="IncreaseAlbumCounter.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if the current WEB API has returned an album which is also the majority album from all other APIs</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	
	public partial class Form1
	{
		private static string IncreaseAlbumCounter(object service, object webserviceAlbum, string majorityAlbum)
		{
			if (!Runtime.AlbumHits.ContainsKey(service.ToString()))
			{
				Runtime.AlbumHits.Add(service.ToString(), 0);
			}
			
			if (webserviceAlbum != null && majorityAlbum != null)
			{
				if (Strip(webserviceAlbum.ToString().ToLower(Runtime.CultEng)) == Strip(majorityAlbum.ToLower(Runtime.CultEng)))
				{
					Runtime.AlbumHits[service.ToString()] += 1;
				}
			}
			
			string result = Runtime.AlbumHits[service.ToString()].ToString(Runtime.CultEng);
			
			return result;
		}
	}
}