using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTwitterApi.Contracts
{
	public class Enums
	{
		public enum XNotificationType
		{
			Like = 1,
			Response = 2,
			Follow = 4,
			Mentioned = 8
		}
	}
}
