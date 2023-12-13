using NSTwitterApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTwitterApi.Models.Notification
{
	public class XLikeNotification : INotificationData
	{
		public string ResponderId { get; internal set; }
		public string ScreenName { get; set; }
		public string LikedTweetId { get; internal set; }
	}
}
