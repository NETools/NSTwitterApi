using Newtonsoft.Json;
using NSTwitterApi.Contracts;
using System.Text;
using static NSTwitterApi.Contracts.Enums;

namespace NSTwitterApi
{
	internal static class Toolkit
	{
		private static char[] Alphabet = Enumerable
												.Range((int)'A', 26)
												.Select(p => (char)p)
												.Concat(Enumerable
												.Range((int)'a', 26)
												.Select(p => (char)p)
												.Concat(Enumerable
												.Range((int)'0', 10)
												.Select(p => (char)p)))
												.Concat(Enumerable
												.Range('/', 1)
												.Select(p => (char)p))
												.Concat(Enumerable
												.Range('+', 1)
												.Select(p => (char)p))
												.ToArray();

		private static Dictionary<char, string> UrlEncodings = new Dictionary<char, string>()
		{
			{ '"', "%22" },
			{ ',', "%2C" },
			{ '{', "%7B" },
			{ '}', "%7D" },
			{ ':', "%3A" }
		};

		public static Dictionary<XNotificationType, string[]> NotificationTypeMapping = new Dictionary<XNotificationType, string[]>()
		{
			{ XNotificationType.Like, ["users_liked_your_tweet", "user_liked_multiple_tweets"] },
			{ XNotificationType.Response, ["user_replied_to_your_tweet"] },
			{ XNotificationType.Follow, ["follow_from_recommended_user"] },
			{ XNotificationType.Mentioned, ["user_mentioned_you"] },

		};

		public static Dictionary<string, XNotificationType> NotificationStringMapping = new Dictionary<string, XNotificationType>()
		{
			{ "users_liked_your_tweet", XNotificationType.Like },
			{ "user_liked_multiple_tweets", XNotificationType.Like },
			{ "user_replied_to_your_tweet", XNotificationType.Response },
			{ "follow_from_recommended_user", XNotificationType.Follow },
			{ "user_mentioned_you", XNotificationType.Mentioned }
		};

		public static HashSet<string> ResolveNotificationTypes(XNotificationType notificationType)
		{
			HashSet<string> ids = new HashSet<string>();
			var encoded = (int)notificationType;
			var bitLength = Math.ILogB(encoded) + 1;
			for (int i = 0; i < bitLength; i++)
			{
				var bit = (encoded >> i) & 1;
				if (bit == 1)
				{
					var mapping = NotificationTypeMapping[(XNotificationType)(1 << i)];
					foreach (var id in mapping)
					{
						ids.Add(id);
					}
				}
			}
			return ids;
		}

		public static string GenerateKey(int length)
		{
			var transactionId = new StringBuilder();

			Enumerable.Range(0, 256).OrderBy(p => Guid.NewGuid()).Take(length).ToList().ForEach((index) =>
			{
				transactionId.Append(Alphabet[index % Alphabet.Length]);
			});

			return transactionId.ToString();
		}

		public static IEnumerable<byte[]> SliceArray(byte[] array, int bytesPerSlice)
		{
			var currentLength = 0;
			while(currentLength < array.Length)
			{
				int len = Math.Min(bytesPerSlice, array.Length - currentLength);
				var buffer = new byte[len];
				Array.Copy(array, currentLength, buffer, 0, len);
				yield return buffer;
				currentLength += bytesPerSlice;
			}
		}

		public static async Task DelayLong(long milliseconds)
		{
			while(milliseconds > 0)
			{
				int millisecondsPart = milliseconds > int.MaxValue ? int.MaxValue : (int)milliseconds;
				await Task.Delay(millisecondsPart);
				milliseconds -= millisecondsPart;
			}
		}

		public static string EncodeToUrl<T>(this T data) where T : IUrlEncoded
		{
			var json = JsonConvert.SerializeObject(data);
			StringBuilder urlBuilder = new StringBuilder();
			for (int i = 0; i < json.Length; i++)
			{
				var c = json[i];
				if (UrlEncodings.ContainsKey(c))
					urlBuilder.Append(UrlEncodings[c]);
				else urlBuilder.Append(c);
			}

			return urlBuilder.ToString();
		}

		
	}
}
