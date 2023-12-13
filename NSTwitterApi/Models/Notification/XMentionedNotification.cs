using NSTwitterApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTwitterApi.Models.Notification
{
	internal class XMentionedNotification : INotificationData
	{
		public string ResponderId { get; internal set; }
		public string ScreenName { get; set; }
		public string ResponseTweetId { get; internal set; }
	}
}
