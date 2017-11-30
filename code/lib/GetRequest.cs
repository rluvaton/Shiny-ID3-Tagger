//-----------------------------------------------------------------------
// <copyright file="GetRequest.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Executes all API requests. Has a built-in retry handler and a logger</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;	
	using System.Linq;
	using System.Net;
	using System.Net.Http;	
	using System.Threading;
	using System.Threading.Tasks;

	public partial class Form1
	{
		private async Task<string> GetRequest(HttpMessageInvoker client, HttpRequestMessage request, CancellationToken cancelToken)
		{
			const int Timeout = 15;
			const int MaxRetries = 3;
			const int RetryDelay = 2;

			HttpResponseMessage response = new HttpResponseMessage();
			HttpRequestMessage requestBackup = CloneRequest(request);

			string responseString = string.Empty;
			
			for (int i = MaxRetries; i >= 1; i--)
			{
				if (cancelToken.IsCancellationRequested)
				{
					return string.Empty;
				}

				string requestContent = string.Empty;
				request = CloneRequest(requestBackup);

				var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
				timeoutToken.CancelAfter(TimeSpan.FromSeconds(Timeout));

				try
				{
					// Save the request content for later reuse when an error occurs or debuging enabled is
					if (request.Content != null)
					{
						requestContent = request.Content.ReadAsStringAsync().Result;
					}
					
					// If debugging level is 3 (DEBUG) or higher, print out ALL requests
					if (User.Settings["DebugLevel"] >= 3)
					{
						List<string> errorMsg = BuildLogMessage(request, requestContent, null);
						this.PrintLogMessage("error", errorMsg.ToArray());
					}
					
					response = await client.SendAsync(request, timeoutToken.Token);

					// These are common errors ie when a queried track does not exist.Suppress them and return with an empty string					
					if ((request.RequestUri.Host == "api.musicgraph.com" && response.StatusCode == HttpStatusCode.NotFound)
						|| (request.RequestUri.Host == "music.xboxlive.com" && response.StatusCode == HttpStatusCode.NotFound)
						|| (request.RequestUri.Host == "api.lololyrics.com" && response.StatusCode == HttpStatusCode.NotFound)
						|| (request.RequestUri.Host == "api.chartlyrics.com" && response.StatusCode == HttpStatusCode.NotFound)
						|| (request.RequestUri.Host == "coverartarchive.org" && response.StatusCode == HttpStatusCode.NotFound)
						|| (request.RequestUri.Host == "api.chartlyrics.com" && response.StatusCode == HttpStatusCode.InternalServerError)
						|| (request.RequestUri.Host == "accounts.spotify.com" && response.StatusCode == HttpStatusCode.BadGateway))						
					{
						break;
					}

					if (response.IsSuccessStatusCode)
					{
						// Response was successful. Read content from response and return content
						responseString = await response.Content.ReadAsStringAsync();
						break;
					}
					else
					{
						// Response was not successful. But it was also not a common error
						if (!cancelToken.IsCancellationRequested)
						{
							// If debugging is enabled in settings, print out all request properties
							if (User.Settings["DebugLevel"] >= 2)
							{
								List<string> errorMsg = new List<string> { "ERROR:    Response was unsuccessful! Retrying..." };
								errorMsg.AddRange(BuildLogMessage(request, requestContent, response));
								errorMsg.Add("Retry:    " + i + "/" + MaxRetries);
								
								this.PrintLogMessage("error", errorMsg.ToArray());								
							}
						}
					}
					
					// Response was not successful (No status code 200)
					// Response was not in the exception list
					// Continue with loop, but wait some seconds before you try it again to give the server time to recover
					Task wait = Task.Delay(RetryDelay * 1000);
				}
				catch (TaskCanceledException)
				{
					// The request timed out. Server took too long to respond
					// Continue with loop
					if (!cancelToken.IsCancellationRequested)
					{
						// If debugging is enabled in settings, print out all request properties
						if (User.Settings["DebugLevel"] >= 2)
						{
							List<string> errorMsg = new List<string> { "ERROR:    Server took longer than " + Timeout + " seconds to respond! Retrying..." };
							errorMsg.AddRange(BuildLogMessage(request, requestContent, null));
							errorMsg.Add("Retry:    " + i + "/" + MaxRetries);
							
							this.PrintLogMessage("error", errorMsg.ToArray());								
						}
					}					
				}
				catch (Exception error)
				{
					// An unknown application error occured. Cancel request immediatly and don't try again
					if (!cancelToken.IsCancellationRequested)
					{
						// If debugging is enabled in settings, print out all request properties
						if (User.Settings["DebugLevel"] >= 1)
						{
							Exception realerror = error;
							while (realerror.InnerException != null)
								realerror = realerror.InnerException;
							
							string[] errorMsg =
							{
								"ERROR:    An unknown application error occured!",
								"Message:  " + realerror.ToString().TrimEnd('\r', '\n')
							};
							this.PrintLogMessage("error", errorMsg);
						}
					}	

					break;
				}
				
				await Task.Delay(2000);
			}

			response.Dispose();
			requestBackup.Dispose();

			return responseString;
		}
	}
}
