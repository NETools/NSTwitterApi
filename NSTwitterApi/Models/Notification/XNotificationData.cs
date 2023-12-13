using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NSTwitterApi.Contracts.Enums;

namespace NSTwitterApi.Models.Notification
{
    internal class XNotificationData
    {
        public string NotificationId;
        public string CursorTop;
        public XNotificationType NotificationType;
    }
}
