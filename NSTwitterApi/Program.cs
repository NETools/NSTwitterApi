// - 1 -
using NSTwitterApi;
using NSTwitterApi.Contracts;
using NSTwitterApi.Models.Api;
using NSTwitterApi.Models.Notification;

using var api = new X("", "")
{
	CacheReader = (id) =>
{
	if (File.Exists(id + ".cache0"))
		return File.ReadAllText(id + ".cache0");
	return null;
},
	CacheWriter = (id, csrf) => File.WriteAllText(id + ".cache0", csrf)
};

api.Notification += Api_Notification;

async void Api_Notification(INotificationData data, NotificationHandler handler)
{
	if (data is XTweetNotification mention)
	{
		Console.WriteLine("Responder name:  " + data.ScreenName);
		Console.WriteLine(await api.ReadTweet(mention.ResponseTweetId));
	}

	await handler.MarkAsRead();
	
}

var apiResult = await api.StartSession();

if (apiResult.ApiStatus != ApiStatusCode.Ok)
{
	Console.WriteLine(apiResult.Data);
	return;
}

Console.WriteLine(api);
var indices = api.UserTweetsAndReplies(1000).ToBlockingEnumerable().ToArray();


Console.Write($"A total of {indices.Length} found -- press enter to delete");
Console.ReadLine();

await api.DeleteTweets(1, indices);

Console.ReadLine();