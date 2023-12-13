using Newtonsoft.Json;
using NSTwitterApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTwitterApi.Models.Json.URL
{
	internal class UrlVariables : IUrlEncoded
	{
		[JsonProperty(PropertyName = "userId", NullValueHandling = NullValueHandling.Ignore)]
		public string? UserId { get; set; }

		[JsonProperty(PropertyName = "count", NullValueHandling = NullValueHandling.Ignore)]
		public int Count { get; set; } = 32;

		[JsonProperty(PropertyName = "cursor", NullValueHandling = NullValueHandling.Ignore)]
		public string? Cursor { get; set; }

		[JsonProperty(PropertyName = "includePromotedContent", NullValueHandling = NullValueHandling.Ignore)]
		public bool IncludePromotedContent { get; set; }

		[JsonProperty(PropertyName = "withClientEventToken", NullValueHandling = NullValueHandling.Ignore)]
		public bool WithClientEventToken { get; set; }

		[JsonProperty(PropertyName = "withBirdwatchNotes", NullValueHandling = NullValueHandling.Ignore)]
		public bool WithBirdwatchNotes { get; set; }

		[JsonProperty(PropertyName = "withVoice", NullValueHandling = NullValueHandling.Ignore)]
		public bool WithVoice { get; set; } = true;

		[JsonProperty(PropertyName = "withV2Timeline", NullValueHandling = NullValueHandling.Ignore)]
		public bool WithV2Timeline { get; set; } = true;
	}
}
