// - 1 -
using NSTwitterApi;
using NSTwitterApi.Contracts;
using NSTwitterApi.Models.Api;

using var api = new X("auth_token")
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
	Console.WriteLine("Responder name:  " + data.ScreenName);
	Console.WriteLine(data);
}

var apiResult = await api.StartSession();

if (apiResult.ApiStatus != ApiStatusCode.Ok)
{
	Console.WriteLine(apiResult.Data);
	return;
}

Console.WriteLine(api);
Console.ReadLine();