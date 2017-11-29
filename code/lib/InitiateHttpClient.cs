//-----------------------------------------------------------------------
// <copyright file="InitiateHttpClient.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Create a single HTTP client which is used later for all Web API requests</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.NetworkInformation;

	public partial class Form1
	{
		private static HttpClient InitiateHttpClient()
		{
			HttpClientHandler handler = new HttpClientHandler();
			handler.UseCookies = false;

			Ping ping = new Ping();

			try
			{
				PingReply reply = ping.Send(User.Settings["Proxy"].Split(':')[0], 100);

				if (reply.Status == IPStatus.Success)
				{
					handler.Proxy = new WebProxy(User.Settings["Proxy"], false);
					handler.UseProxy = true;
				}
			}
			catch (ArgumentException)
			{
				// user entered an invalid "proxy:port" string in settings.json e.g. "0.0.0.0:0000"
			}
			catch (NullReferenceException)
			{
				// User closed the window while ping was still running
			}

			HttpClient client = new HttpClient(handler);
			client.MaxResponseContentBufferSize = 256000000;

			ServicePointManager.DefaultConnectionLimit = 10;		// Not sure if it's needed since this limit applies to connection per remote host (per API), not per client
			
			ping.Dispose();
			return client;
		}
	}
}
