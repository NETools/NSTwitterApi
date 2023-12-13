using NSTwitterApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTwitterApi
{
	public class NotificationHandler
	{
		private X _x;
		private string _cursorTop;

		internal NotificationHandler(X x, string cursorTop)
		{
			_x = x;
			_cursorTop = cursorTop;
		}

		public Task<bool> MarkAsRead()
		{
			return _x.UpdateCursor(_cursorTop);
		}
	}
}
