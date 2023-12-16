using NSTwitterApi.Contracts;
using NSTwitterApi.Models.Api;
using NSTwitterApi.Models.Json;
using NSTwitterApi.Models.Json.URL;
using NSTwitterApi.Models.Notification;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Timers;
using static NSTwitterApi.Contracts.Enums;
using Timer = System.Timers.Timer;

namespace NSTwitterApi
{
    /// <summary>
    /// - 1 -
    /// <br></br>
    /// BiidhnIllah programmed by Enes Hergül
    /// <br></br>
    /// Uses auth_token to trick twitter into thinking user is interacting with the website.
    /// <br></br>
    /// Currently, this class provides basic method like obtaining or sending tweets, deleting them or uploading media.
    /// <br></br>
    /// ToDo: Poll status.
    /// </summary>
    public class X : IDisposable
	{
		private const string BearerToken = "AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA";
		private const string BaseUrl = "https://twitter.com";

		private HttpClient _client;
		private CookieContainer _cookieContainer;

		private bool _loggedIn;

		private Timer _notificationTimer;

		/// <summary>
		/// The main logger.
		/// </summary>
		public Action<string> Logger { get; set; } = new Action<string>((log) => Console.WriteLine($"[*] {log}"));

		/// <summary>
		/// Pointer to a function returning the cached csrf token associated with auth_token
		/// </summary>
		public Func<string, string?> CacheReader { get; set; } = (id) => null;

		/// <summary>
		/// Pointer to a method writing the csrf token associated with auth_token.
		/// <br></br>
		/// First argument is the auth_token, second argument the generated csrf token.
		/// </summary>
		public Action<string, string?> CacheWriter { get; set; } = (id, ct0) => { };

		/// <summary>
		/// The auth_token cookie token.
		/// </summary>
		public string AuthToken { get; private set; }
		/// <summary>
		/// The ct0 cookie token.
		/// </summary>
		public string CSRFToken { get; private set; }

		/// <summary>
		/// Your twitter screen name (@exmaple)
		/// </summary>
		public string ScreenName { get; private set; }

		/// <summary>
		/// The rest id associated with your profile
		/// </summary>
		public string RestId { get; private set; }

		public bool NotificationEnabled
		{
			get => _notificationTimer.Enabled;
			set => _notificationTimer.Enabled = value;
		}

		public event Action<INotificationData, NotificationHandler> Notification;

		/// <summary>
		/// Initializes a new X-API-Client.
		/// </summary>
		/// <param name="auth_token">The value for auth_token can be found in your browser's cookie container.<br>The user must be logged in.</br></param>
		public X(string auth_token, bool enableNotification = true, int notificationTimerInSeconds = 5)
		{
			AuthToken = auth_token;

			_notificationTimer = new Timer(TimeSpan.FromSeconds(notificationTimerInSeconds));
			_notificationTimer.Elapsed += HandleNotification;

			NotificationEnabled = enableNotification;
		}

		private async void HandleNotification(object? sender, ElapsedEventArgs e)
		{
			if (!NotificationEnabled)
				return;
			if (!_loggedIn)
				return;

			Logger?.Invoke("Handle notification.");

			var unreadCount = await UnreadCount();
			if (unreadCount == 0)
				return;

			var api = "/i/api/2/notifications/all.json?include_profile_interstitial_type=1&include_blocking=1&include_blocked_by=1&include_followed_by=1&include_want_retweets=1&include_mute_edge=1&include_can_dm=1&include_can_media_tag=1&include_ext_has_nft_avatar=1&include_ext_is_blue_verified=1&include_ext_verified_type=1&include_ext_profile_image_shape=1&skip_status=1&cards_platform=Web-12&include_cards=1&include_ext_alt_text=true&include_ext_limited_action_results=true&include_quote_count=true&include_reply_count=1&tweet_mode=extended&include_ext_views=true&include_entities=true&include_user_entities=true&include_ext_media_color=true&include_ext_media_availability=true&include_ext_sensitive_media_warning=true&include_ext_trusted_friends_metadata=true&send_error_codes=true&simple_quoted_tweet=true&count=20&requestContext=launch&ext=mediaStats%2ChighlightedLabel%2ChasNftAvatar%2CvoiceInfo%2CbirdwatchPivot%2CsuperFollowMetadata%2CunmentionInfo%2CeditControl";

			SetTransactionId();

			var response = await _client.GetAsync(BaseUrl + api);
			var stream = await response.Content.ReadAsStreamAsync();
			var root = JsonNode.Parse(stream);

			var notifications = CollectNotificationIds(root, XNotificationType.Like | XNotificationType.Response | XNotificationType.Follow | XNotificationType.Mentioned);
			await foreach (var notification in notifications)
			{
				var notificationType = notification.NotificationType;
				INotificationData tweetNotification = default;
				switch (notificationType)
				{
					case XNotificationType.Response:
						var tweetInQuestion = root["globalObjects"]["tweets"][notification.NotificationId];
						tweetNotification = new XTweetNotification()
						{
							ResponderId = tweetInQuestion["user_id_str"].GetValue<string>(),
							ResponseTweetId = tweetInQuestion["id_str"].GetValue<string>(),
							RespondedToTweetId = tweetInQuestion["in_reply_to_status_id_str"].GetValue<string>(),
						};
						break;
					case XNotificationType.Like:
						var likedTweetInQuestion = root["globalObjects"]["notifications"][notification.NotificationId];
						tweetNotification = new XLikeNotification()
						{
							ResponderId = likedTweetInQuestion["template"]["aggregateUserActionsV1"]["fromUsers"][0]["user"]["id"].GetValue<string>(),
							LikedTweetId = likedTweetInQuestion["template"]["aggregateUserActionsV1"]["targetObjects"][0]["tweet"]["id"].GetValue<string>()
						};
						break;
					case XNotificationType.Follow:
						var followInQuestion = root["globalObjects"]["notifications"][notification.NotificationId];
						tweetNotification = new XFollowNotification()
						{
							ResponderId = followInQuestion["template"]["aggregateUserActionsV1"]["fromUsers"][0]["user"]["id"].GetValue<string>()
						};
						break;
					case XNotificationType.Mentioned:
						var mentionedInQuestion = root["globalObjects"]["tweets"][notification.NotificationId];
						tweetNotification = new XMentionedNotification()
						{
							ResponderId = mentionedInQuestion["user_id_str"].GetValue<string>(),
							ResponseTweetId = mentionedInQuestion["id_str"].GetValue<string>()
						};
						break;
				}

				tweetNotification.ScreenName = await ResolveRestId(tweetNotification.ResponderId);

				var notificationHandler = new NotificationHandler(this, notification.CursorTop);
				Notification?.Invoke(tweetNotification, notificationHandler);
			}
		}

		internal async Task<bool> UpdateCursor(string cursorId)
		{
			using var urlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>() { { "cursor", cursorId } });
			var response = await _client.PostAsync("https://twitter.com/i/api/2/notifications/all/last_seen_cursor.json", urlEncodedContent);
			var content = await response.Content.ReadAsStringAsync();
			var root = JsonNode.Parse(content);
			return root["cursor"].GetValue<string>() == cursorId;
		}

		private async IAsyncEnumerable<XNotificationData> CollectNotificationIds(JsonNode? root, XNotificationType notificationType)
		{
			var notificationIds = Toolkit.ResolveNotificationTypes(notificationType);
			var instructions = root["timeline"]["instructions"].AsArray();
			
			JsonArray entries = default;
			long markSortIndex = 0;

			foreach (var instruction in instructions)
			{
				var addEntries = instruction["addEntries"];
				if (addEntries != null)
				{
					entries = addEntries["entries"] as JsonArray;
					continue;
				}

				var markEntriesUnreadGreaterThanSortIndex = instruction["markEntriesUnreadGreaterThanSortIndex"];
				if (markEntriesUnreadGreaterThanSortIndex != null)
				{
					markSortIndex = long.Parse(markEntriesUnreadGreaterThanSortIndex["sortIndex"].GetValue<string>());
				}
			}

			var cursor = entries[0]["content"]["operation"]["cursor"]["value"].GetValue<string>();

			foreach (var entry in entries)
			{
				var entryId = entry["entryId"].GetValue<string>();
				if (!entryId.Contains("notification"))
					continue;

				var sortIndex = long.Parse(entry["sortIndex"].GetValue<string>());
				if (markSortIndex >= sortIndex)
					break;

				var element = entry["content"]["item"]["clientEventInfo"]["element"].GetValue<string>();
				if (!notificationIds.Contains(element))
				{
					continue;
				}

				var elementNotificationType = Toolkit.NotificationStringMapping[element];
				string elementNotificationId = "";
				if (elementNotificationType == XNotificationType.Response || elementNotificationType == XNotificationType.Mentioned)
					elementNotificationId = entry["content"]["item"]["content"]["tweet"]["id"].GetValue<string>();
				else if (elementNotificationType == XNotificationType.Like || elementNotificationType == XNotificationType.Follow)
				{
					elementNotificationId = entry["content"]["item"]["content"]["notification"]["id"].GetValue<string>();
				}

				var notificationData = new XNotificationData()
				{
					NotificationId = elementNotificationId,
					NotificationType = elementNotificationType,
					CursorTop = cursor
				};

				yield return notificationData;

			}


		}

		private async Task<int> UnreadCount()
		{
			string api = "/i/api/2/badge_count/badge_count.json?supports_ntab_urt=1";

			SetTransactionId();

			var response = await _client.GetAsync(BaseUrl + api);
			var stream = await response.Content.ReadAsStreamAsync();
			var root = JsonNode.Parse(stream);

			return root["total_unread_count"].GetValue<int>();
		}

		/// <summary>
		/// Deletes all specified tweet id's
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="indices"></param>
		/// <returns>Amount of tweets deleted.</returns>
		public async Task<int> DeleteTweets(int delay, params string[] indices)
		{
			CheckValidity();
			string api = "/i/api/graphql/VaenaVgh5q5ih7kvyVjgtg/DeleteTweet";
			int count = 0;
			foreach (var index in indices)
			{
				SetTransactionId();
				var content = new StringContent("{\"variables\":{\"tweet_id\":\"" + index + "\",\"dark_request\":false},\"queryId\":\"VaenaVgh5q5ih7kvyVjgtg\"}", Encoding.UTF8, "application/json");
				var response = await _client.PostAsync(BaseUrl + api, content);
				if (response.StatusCode == HttpStatusCode.OK)
					count++;

				await Task.Delay(delay);
				Logger($"Deleted {index}");
			}
			return count;
		}

		/// <summary>
		/// Unlikes all specified tweet id's
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="indices"></param>
		/// <returns>Amount of tweets unliked.</returns>
		public async Task<int> UnlikeTweets(int delay, params string[] indices)
		{
			CheckValidity();
			string api = "/i/api/graphql/ZYKSe-w7KEslx3JhSIk5LA/UnfavoriteTweet";
			int count = 0;
			foreach (var index in indices)
			{
				SetTransactionId();
				var content = new StringContent("{\"variables\":{\"tweet_id\":\"" + index + "\"},\"queryId\":\"ZYKSe-w7KEslx3JhSIk5LA\"}", Encoding.UTF8, "application/json");
				var response = await _client.PostAsync(BaseUrl + api, content);
				if (response.StatusCode == HttpStatusCode.OK)
					count++;

				await Task.Delay(delay);
				Logger($"Unliked {index}");
			}
			return count;
		}

		/// <summary>
		/// Get's specified amount of user likes 
		/// </summary>
		/// <param name="count">Amount of tweets to obtain</param>
		/// <returns><see cref="IAsyncEnumerable{T}"/> of tweets (text, id)</returns>
		public IAsyncEnumerable<string?> UserLikes(int count)
		{
			return RetrieveTweets(count, RestId, false, "BZpa3joi5mv2Rp14jC7y3A/Likes");
		}

		/// <summary>
		/// Get's specified amount of tweets 
		/// </summary>
		/// <param name="count">Amount of tweets to obtain</param>
		/// <returns><see cref="IAsyncEnumerable{T}"/> of tweets (text, id)</returns>
		public IAsyncEnumerable<string?> UserTweetsAndReplies(int count)
		{
			return RetrieveTweets(count, RestId, true, "YlkSUg0mRBx7-EkxCvc-bw/UserTweetsAndReplies");
		}

		/// <summary>
		/// Get's specified amount of tweets 
		/// </summary>
		/// <param name="count">Amount of tweets to obtain</param>
		/// <returns><see cref="IAsyncEnumerable{T}"/> of tweets (text, id)</returns>
		public IAsyncEnumerable<string?> UserTweetsAndReplies(string restId, int count)
		{
			return RetrieveTweets(count, restId, true, "YlkSUg0mRBx7-EkxCvc-bw/UserTweetsAndReplies");
		}

		private async IAsyncEnumerable<string?> RetrieveTweets(int count, string restId, bool compareRestIds, string apiCall)
		{
			CheckValidity();
			int totalTweets = 0;
			string cursor = null;

			var variables = new UrlVariables()
			{
				UserId = restId
			};
			var features = new UrlFeatures();

			while (totalTweets < count)
			{
				Logger($"Retrieving tweets under cursor: {cursor}");

				SetTransactionId();

				variables.Cursor = cursor;
				variables.Count = Math.Min(count, 20);
				var jsonResponse = await GetTweets(apiCall, variables, features);

				if (!await EvalResponse(jsonResponse))
					break;

				JsonNode? baseNode = null;
				try
				{
					Logger($"Parsing json response.");
					baseNode = JsonNode.Parse(jsonResponse.Json);
				}
				catch
				{
					Logger(jsonResponse.Json);
					await Task.Delay(1000);
					continue;
				}

				var errorNode = CheckForErrors(baseNode);
				if (errorNode.ApiStatus == ApiStatusCode.Failed)
				{
					Logger($"[ERROR] {errorNode.Data}");
					yield break;
				}

				var root = baseNode["data"]["user"]["result"]["timeline_v2"]["timeline"]["instructions"];
				if (root is JsonArray instructions)
				{
					if (instructions.Count == 0)
						yield break;

					foreach (var instruction in instructions)
					{
						if (instruction["type"].GetValue<string>() != "TimelineAddEntries")
							continue;
						var entries = instruction["entries"] as JsonArray;
						if (entries == null)
							continue;

						if (entries.Count == 2)
							yield break;

						foreach (var entry in entries)
						{
							var contentType = entry["content"]["entryType"].GetValue<string>();
							if (contentType == "TimelineTimelineItem")
							{
								var result = ExtractTweetId(entry["content"]["itemContent"], restId, compareRestIds);
								if (result != null)
								{
									totalTweets++;
									yield return result;
								}
							}
							else if (contentType == "TimelineTimelineModule")
							{
								var items = entry["content"]["items"] as JsonArray;
								foreach (var item in items)
								{
									if (item["item"]["itemContent"]["itemType"].GetValue<string>() == "TimelineTweet")
									{
										var result = ExtractTweetId(item["item"]["itemContent"], restId, compareRestIds);
										if (result != null)
										{
											totalTweets++;
											yield return result;
										}
									}

								}
							}
							else if (contentType == "TimelineTimelineCursor" && entry["entryId"].GetValue<string>().Contains("cursor-bottom"))
							{
								cursor = entry["content"]["value"].GetValue<string>();
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Uploads a media file to twitter.
		/// </summary>
		/// <param name="media">The media as byte array</param>
		/// <param name="mediaType">Media type</param>
		/// <param name="mediaCategory">Media category</param>
		/// <param name="additionalQuery">Additional queries like video duration
		/// <br></br>
		/// Key-value notation has to be used, following parameters must be seperated with &amp;</param>
		/// <returns><see cref="APIResponse"/> contains media id that may be used when creating a tweet.</returns>
		public async Task<APIResponse> UploadMedia(byte[] media, string mediaType, string mediaCategory, string additionalQuery)
		{
			CheckValidity();
			var apiResponse = new APIResponse();
			var initResult = await InitUpload(media.Length, mediaType, mediaCategory, additionalQuery);
			if (initResult.ApiStatus == ApiStatusCode.Ok)
			{
				var mediaId = initResult.Data;
				int segment = 0;
				var slices = Toolkit.SliceArray(media, 999_999);
				foreach (var slice in slices)
				{
					var appendResult = await AppendMedia(mediaId, slice, segment);
					if (appendResult.ApiStatus != ApiStatusCode.Ok)
					{
						apiResponse.ApiStatus = ApiStatusCode.Failed;
						apiResponse.HttpStatusCode = appendResult.HttpStatusCode;
						apiResponse.Data = $"Failed to append segment {segment}";
						return apiResponse;
					}
					segment++;
				}

				var finalizeResult = await FinalizeMedia(mediaId);
				if (finalizeResult.ApiStatus == ApiStatusCode.Ok)
				{
					apiResponse.ApiStatus = ApiStatusCode.Ok;
					apiResponse.Data = finalizeResult.Data;
				}
				else
				{
					apiResponse.ApiStatus = ApiStatusCode.Failed;
					apiResponse.Data = "Failed to finalize upload.";
				}

			}
			else
			{
				apiResponse.ApiStatus = ApiStatusCode.Failed;
				apiResponse.Data = "Failed to init media.";
			}

			return apiResponse;
		}

		/// <summary>
		/// Creates a tweet
		/// </summary>
		/// <param name="message">The tweet to send</param>
		/// <param name="mediaIds">Additional media id's to append</param>
		/// <param name="isNsfw">Indicates whether media is sensitive</param>
		/// <returns><see cref="APIResponse"/></returns>
		public async Task<APIResponse> CreateTweet(string message, string[] mediaIds = null, bool isNsfw = false)
		{
			CheckValidity();
			SetTransactionId();

			var api = "/i/api/graphql/5V_dkq1jfalfiFOEZ4g47A/CreateTweet";
		
			var requestObject = new Root()
			{
				QueryId = "5V_dkq1jfalfiFOEZ4g47A",
				Features = new Features(),
				Variables = new Variables()
				{
					DarkRequest = false,
					TweetText = message,
					SemanticAnnotationIds = new List<object>(),
					Media = new Media()
					{
						PossiblySensitive = isNsfw,
						MediaEntities = new List<MediaEntity>()
					}
				}
			};

			foreach (var mediaId in mediaIds)
			{
				var statusApiResponse = await StatusMedia(mediaId);
				while(statusApiResponse.ApiStatus == ApiStatusCode.NotDone)
				{
					var secondsToWait = int.Parse(statusApiResponse.Data);
					await Task.Delay(secondsToWait * 1000);
					Logger($"Wait {secondsToWait} seconds.");
					statusApiResponse = await StatusMedia(mediaId);
				}

				requestObject.Variables.Media.MediaEntities.Add(new MediaEntity()
				{
					MediaId = mediaId,
					TaggedUsers = new List<object>()
				});
			}

			var json = JsonSerializer.Serialize(requestObject);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await _client.PostAsync(BaseUrl + api, content);
			var responseText = await content.ReadAsStringAsync();

			var apiResult = new APIResponse();
			if(response.StatusCode == HttpStatusCode.OK)
			{
				apiResult.HttpStatusCode = HttpStatusCode.OK;
				apiResult.Data = responseText;
				apiResult.ApiStatus = ApiStatusCode.Ok;
			}
			else
			{
				apiResult.HttpStatusCode = response.StatusCode;
				apiResult.Data = "Message sent failed";
				apiResult.ApiStatus = ApiStatusCode.Failed;
			}

			return apiResult;
		}

		/// <summary>
		/// Uses the auth_token to enter account and generates csrf tokens to emulate browser interaction with twitter.
		/// </summary>
		/// <returns><see cref="APIResponse"/> indicates whether the call has worked</returns>
		public async Task<APIResponse> StartSession()
		{
			var apiResponse = new APIResponse();

			var csrfToken = CacheReader(AuthToken);
			if(csrfToken != null)
			{
				Logger("Reading csrf from cache.");
				CSRFToken = csrfToken;
			}


			if (CSRFToken == null)
			{
				// Step 1: Initialize client header
				Logger("InitializeAPI");
				InitializeAsAPI();

				// Step 2: Send GET-Request to twitter, obtain ct0 from cookie-container
				var response = await _client.GetAsync(BaseUrl);
				var responseCookies = _cookieContainer.GetCookies(new Uri(BaseUrl)).Cast<Cookie>().ToList();

				var csrf = responseCookies.Find(p => p.Name.Equals("ct0"));

				apiResponse.HttpStatusCode = response.StatusCode;

				if (csrf == null)
				{
					apiResponse.Data = "auth_token is not known.";
					apiResponse.ApiStatus = ApiStatusCode.AuthFailed;

					return apiResponse;
				}

				// Step 3: Re-initialize with ct0

				CSRFToken = csrf.Value;
				apiResponse.ApiStatus = ApiStatusCode.Ok;
				apiResponse.Data = "CSRF generated.";

				Logger("CSRF generated.");

				CacheWriter(AuthToken, CSRFToken);

				response.Dispose();
			}

			InitializeAsWebBrowser();
			ScreenName = await FindScreenName();

			InitializeAsAPI();
			var restId = await GetRestId(ScreenName);
			if(restId.ApiStatus != ApiStatusCode.Ok)
			{
				apiResponse.ApiStatus = ApiStatusCode.Failed;
				apiResponse.Data = "Could not retrieve rest_id.";
				_loggedIn = false;
				return apiResponse;
			}

			RestId = restId.Data!;
			
			apiResponse.HttpStatusCode = HttpStatusCode.OK;
			_loggedIn = true;
			HandleNotification(this, null);
			return apiResponse;
		}

		/// <summary>
		/// Resolves the rest id to the corresponding screen name
		/// </summary>
		/// <param name="restId"></param>
		/// <returns>Null if user is found otherwise null</returns>
		public async Task<string> ResolveRestId(string restId)
		{
			string api = $"https://api.twitter.com/1.1/users/show.json?user_id={restId}";
			var response = await _client.GetAsync(api);
			var content = await response.Content.ReadAsStringAsync();
			var root = JsonNode.Parse(content);
			return root["screen_name"].GetValue<string>();
		}


		/// <summary>
		/// Resolves the screename to the corresponding rest id
		/// </summary>
		/// <param name="screenName"></param>
		/// <returns>Null if user is found otherwise null</returns>
		public async Task<string?> ResolveScreenName(string screenName)
		{
			var apiResult = await GetRestId(screenName);
			if (apiResult.ApiStatus != ApiStatusCode.Ok)
				return null;
			return apiResult.Data;
		}

		private async Task<APIResponse> GetRestId(string screenName)
		{
			string api = $"/i/api/graphql/G3KGOASz96M-Qu0nwmGXNg/UserByScreenName?variables=%7B%22screen_name%22%3A%22{screenName}%22%2C%22withSafetyModeUserFields%22%3Atrue%7D&features=%7B%22hidden_profile_likes_enabled%22%3Atrue%2C%22hidden_profile_subscriptions_enabled%22%3Atrue%2C%22responsive_web_graphql_exclude_directive_enabled%22%3Atrue%2C%22verified_phone_label_enabled%22%3Afalse%2C%22subscriptions_verification_info_is_identity_verified_enabled%22%3Atrue%2C%22subscriptions_verification_info_verified_since_enabled%22%3Atrue%2C%22highlights_tweets_tab_ui_enabled%22%3Atrue%2C%22creator_subscriptions_tweet_preview_api_enabled%22%3Atrue%2C%22responsive_web_graphql_skip_user_profile_image_extensions_enabled%22%3Afalse%2C%22responsive_web_graphql_timeline_navigation_enabled%22%3Atrue%7D&fieldToggles=%7B%22withAuxiliaryUserLabels%22%3Afalse%7D";

			SetTransactionId();

			var apiResponse = new APIResponse();

			var response = await _client.GetAsync(BaseUrl + api);
			var stream = await response.Content.ReadAsStreamAsync();
			var root = JsonNode.Parse(stream);

			try
			{
				var errorNode = CheckForErrors(root);
				if (errorNode.ApiStatus == ApiStatusCode.Failed)
				{
					apiResponse.ApiStatus = ApiStatusCode.Failed;
					apiResponse.Data = errorNode.Data;
					Logger($"[ERROR] {errorNode.Data}");
					return apiResponse;
				}

				var dataUser = root["data"]["user"];

				if(dataUser == null)
				{
					apiResponse.ApiStatus = ApiStatusCode.Failed;
					apiResponse.Data = "User not found.";
					return apiResponse;
				}

				var typeName = dataUser["result"]["__typename"];

				if (typeName.GetValue<string>() == "UserUnavailable")
				{
					apiResponse.ApiStatus = ApiStatusCode.Failed;
					apiResponse.Data = "User banned.";
					return apiResponse;
				}

				apiResponse.ApiStatus = ApiStatusCode.Ok;
				apiResponse.Data = root["data"]["user"]["result"]["rest_id"].GetValue<string>();


				return apiResponse;
			}
			finally
			{
				response.Dispose();
				stream.Dispose();
			}
		}

		private async Task<string> FindScreenName()
		{
			var response = await _client.GetAsync(BaseUrl);
			var content = await response.Content.ReadAsStringAsync();

			Regex regEx = new Regex("\"screen_name\":\"([^\\\"]+)\"");
			var match = regEx.Match(content);

			response.Dispose();

			return match.Groups[1].Value;
		}
		private async Task<APIResponse> InitUpload(int length, string mediaType, string mediaCategory, string additionalQuery)
		{
			SetTransactionId();
			Logger($"Init upload for media: {length} | {mediaType} | {mediaCategory} | {additionalQuery}");

			string api = $"/i/media/upload.json?command=INIT&total_bytes={length}&media_type={mediaType}&media_category={mediaCategory}&{additionalQuery}";

			Logger("Send media initialization post request.");
			var response = await _client.PostAsync(BaseUrl + api, null);

			Logger("Request initialization stream.");
			var utf8Stream = await response.Content.ReadAsStreamAsync();

			Logger("Parse media initialization stream.");
			var node = JsonNode.Parse(utf8Stream);

			var apiResponse = new APIResponse();
			apiResponse.HttpStatusCode = response.StatusCode;
			if (response.StatusCode == HttpStatusCode.Accepted)
			{
				Logger("Media initialization done.");
				apiResponse.ApiStatus = ApiStatusCode.Ok;
				apiResponse.Data = node["media_id_string"].GetValue<string>();
			}
			else
			{
				Logger("Media initialization failed.");
				apiResponse.ApiStatus = ApiStatusCode.Failed;
			}

			response.Dispose();
			utf8Stream.Dispose();

			return apiResponse;
		}
		private async Task<APIResponse> AppendMedia(string mediaId, byte[] buffer, int segment)
		{
			SetTransactionId();
			Logger($"Append media: {buffer.Length} | {mediaId} | {segment}");
			string api = $"/i/media/upload.json?command=APPEND&media_id={mediaId}&segment_index={segment}";
			using (var content = new MultipartFormDataContent($"----WebKitFormBoundary{Toolkit.GenerateKey(16)}"))
			{
				content.Add(new StreamContent(new MemoryStream(buffer)), "media", "blob");

				Logger("Send media append post request.");
				var response = await _client.PostAsync(BaseUrl + api, content);

				var apiResult = new APIResponse();
				apiResult.HttpStatusCode = response.StatusCode;

				if (response.StatusCode == HttpStatusCode.NoContent)
				{
					Logger("Append media done.");
					apiResult.ApiStatus = ApiStatusCode.Ok;
				}
				else
				{
					Logger("Append media failed.");
					apiResult.ApiStatus = ApiStatusCode.Failed;
				}

				response.Dispose();

				return apiResult;
			}
		}
		private async Task<APIResponse> FinalizeMedia(string mediaId)
		{
			SetTransactionId();
			Logger($"Finalize media upload: {mediaId}");
			var api = $"/i/media/upload.json?command=FINALIZE&media_id={mediaId}";

			Logger("Send finalize post request.");
			var response = await _client.PostAsync(BaseUrl + api, null);

			var apiResponse = new APIResponse();
			apiResponse.HttpStatusCode = response.StatusCode;
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
			{
				Logger("Finalize media done.");

				apiResponse.ApiStatus = ApiStatusCode.Ok;
				var content = await response.Content.ReadAsStreamAsync();
				var node = JsonNode.Parse(content);
				apiResponse.Data = node["media_id_string"].GetValue<string>();
				content.Dispose();
			}
			else
			{
				Logger("Finalize media failed.");
				apiResponse.ApiStatus = ApiStatusCode.Failed;
			}

			response.Dispose();


			return apiResponse;
		}
		private async Task<APIResponse> StatusMedia(string mediaId)
		{
			SetTransactionId();
			Logger($"Begin status media: {mediaId}");
			string api = $"/i/media/upload.json?command=STATUS&media_id={mediaId}";

			Logger("Send status get request.");
			var response = await _client.GetAsync(BaseUrl + api);

			var apiResponse = new APIResponse();
			apiResponse.HttpStatusCode = response.StatusCode;
			if (response.StatusCode == HttpStatusCode.OK)
			{
				apiResponse.ApiStatus = ApiStatusCode.Ok;
				var contentStream = await response.Content.ReadAsStreamAsync();
				var node = JsonNode.Parse(contentStream);
				var status = node["processing_info"]["state"].GetValue<string>();
				if (status == "succeeded")
				{
					Logger("Media upload status done.");
					apiResponse.ApiStatus = ApiStatusCode.Ok;
				}
				else if (status == "in_progress")
				{
					Logger("Media upload status in progress.");

					var checkAfterSecond = node["processing_info"]["check_after_secs"].GetValue<int>();
					apiResponse.ApiStatus = ApiStatusCode.NotDone;
					apiResponse.Data = checkAfterSecond.ToString();
				}

				contentStream.Dispose();
			}

			response.Dispose();

			return apiResponse;
		}

		private void InitializeAsWebBrowser()
		{
			if (_client != null)
			{
				_client.Dispose();
				Logger("Client disposed.");
			}
			_client = new HttpClient();
			Logger("New Webbrowser initialized");

			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
			_client.DefaultRequestHeaders.Add("Cookie", $"auth_token={AuthToken}; ct0={CSRFToken}");

			Logger("Webbrowser headers set.");
		}
		private void InitializeAsAPI()
		{
			if (_client != null)
			{
				_client.Dispose();
				Logger("Client disposed.");
			}
			ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

			_cookieContainer = new CookieContainer();
			var clientHandler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.All };
			clientHandler.CookieContainer = _cookieContainer;

			_client = new HttpClient(clientHandler);

			Logger("New api client initialized");

			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

			_client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
			_client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
			_client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

			_client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("de-DE"));
			_client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("de"));
			_client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("q", 0.9));

			_client.DefaultRequestHeaders.Connection.Add("Keep-Alive");

			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

			_client.DefaultRequestHeaders.Add("Origin", "https://twitter.com");

			_client.DefaultRequestHeaders.Referrer = new Uri("https://twitter.com/compose/tweet");

			_client.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\"Chromium\";v=\"118\", \"Brave\";v=\"118\", \"Not=A?Brand\";v=\"99\"");

			_client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?0");

			_client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"Windows\"");

			_client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");

			_client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");

			_client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");

			_client.DefaultRequestHeaders.Add("Sec-Gpc", "1");

			_client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");

			_client.DefaultRequestHeaders.Add("X-Client-Uuid", Guid.NewGuid().ToString());

			SetTransactionId();

			_client.DefaultRequestHeaders.Add("Cookie", $"auth_token={AuthToken}; ct0={CSRFToken}");
			_client.DefaultRequestHeaders.Add("X-Csrf-Token", CSRFToken);


			_client.DefaultRequestHeaders.Add("X-Twitter-Active-User", "yes");

			_client.DefaultRequestHeaders.Add("X-Twitter-Auth-Type", "OAuth2Session");

			_client.DefaultRequestHeaders.Add("X-Twitter-Client-Language", "en");

			Logger("Headers set.");
		}
		private async Task<JsonResponse> GetTweets(string apiCall, UrlVariables variables, UrlFeatures features)
		{

			var variableUrlEncoded = variables.EncodeToUrl();
			var featureUrlEncoded = features.EncodeToUrl();

			string api = $"/i/api/graphql/{apiCall}?variables={variableUrlEncoded}&features={featureUrlEncoded}";

			Logger("Making GET-Request to server.");
			using var response = await _client.GetAsync(BaseUrl + api);

			Logger("Processing json response.");
			var jsonResponse = new JsonResponse(response);
			await jsonResponse.Read();

			return jsonResponse;
		}

		private void CheckValidity()
		{
			if (!_loggedIn)
				throw new Exception("You are not logged in.");
		}
		private void SetTransactionId()
		{
			_client.DefaultRequestHeaders.Remove("X-Client-Transaction-Id");
			_client.DefaultRequestHeaders.Add("X-Client-Transaction-Id", Toolkit.GenerateKey(94));
			Logger("Transaction id set.");
		}

		private static string? ExtractTweetId(JsonNode node, string restId, bool compareRestIds)
		{
			var resultNode = node["tweet_results"]["result"];
			string? userRestId;
			if (resultNode["__typename"].GetValue<string>() == "TweetWithVisibilityResults")
			{
				userRestId = resultNode["tweet"]["legacy"]["user_id_str"].GetValue<string>();
				resultNode = resultNode["tweet"];
			}
			else
			{
				userRestId = resultNode["legacy"]["user_id_str"].GetValue<string>();
			}

			if (!compareRestIds && userRestId != restId)
				return null;

			var tweetRestId = resultNode["rest_id"].GetValue<string>();
			return tweetRestId;
		}
		private async Task TooManyRequests(JsonResponse response)
		{
			var epoch = long.Parse(response.ResponseHeader["x-rate-limit-reset"].First());
			var time = DateTimeOffset.FromUnixTimeSeconds(epoch);
			Logger($"The maximum count of requests have been exceeded -- the blockage will be lifted {time}.");
			var waitingPeriodInMilliseconds = (epoch - DateTimeOffset.UtcNow.ToUnixTimeSeconds()) * 1000;
			Logger($"Thread is waiting {(waitingPeriodInMilliseconds / 1000) / 60} minutes.");
			await Toolkit.DelayLong(waitingPeriodInMilliseconds);
		}
		private static APIResponse CheckForErrors(JsonNode node)
		{
			var result = new APIResponse();
			result.ApiStatus = ApiStatusCode.Ok;

			if (node.AsObject().ContainsKey("errors"))
			{
				result.ApiStatus = ApiStatusCode.Failed;
				result.Data = node["errors"][0]["message"].GetValue<string>();
			}

			return result;
		}
		private async Task<bool> EvalResponse(JsonResponse response)
		{
			var rateLimit = response.ResponseHeader["x-rate-limit-remaining"].ToList()[0];
			Logger($"Remaining requests: {rateLimit}");

			switch (response.StatusCode)
			{
				case HttpStatusCode.TooManyRequests:
					await TooManyRequests(response);
					return true;
			}

			return true;
		}

		public override string ToString()
		{
			StringBuilder debugBuilder = new StringBuilder();
			if (!_loggedIn)
				debugBuilder.AppendLine("LogIn failed!");
			else
				debugBuilder.AppendLine($"Logged in as {ScreenName} ({RestId})");
			return debugBuilder.ToString();
		}

		public void Dispose()
		{
			_client.Dispose();
			_notificationTimer.Dispose();
		}
	}
}
