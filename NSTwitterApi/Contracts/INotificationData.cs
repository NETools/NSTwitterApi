using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTwitterApi.Contracts
{
	public interface INotificationData
	{
		public string ResponderId { get; }
		public string ScreenName { get; set; }
	}
}
