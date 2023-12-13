using Newtonsoft.Json;
using NSTwitterApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NSTwitterApi.Models.Json.URL
{
	internal class UrlFeatures : IUrlEncoded
	{
		[JsonProperty(PropertyName = "responsive_web_graphql_exclude_directive_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool ResponsiveWebGraphqlExcludeDirectiveEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "verified_phone_label_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool VerifiedPhoneLabelEnabled { get; set; }

		[JsonProperty(PropertyName = "responsive_web_home_pinned_timelines_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool ResponsiveWebHomePinnedTimelinesEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "creator_subscriptions_tweet_preview_api_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool CreatorSubscriptionsTweetPreviewApiEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "responsive_web_graphql_timeline_navigation_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool ResponsiveWebGraphqlTimelineNavigationEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "responsive_web_graphql_skip_user_profile_image_extensions_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool ResponsiveWebGraphqlSkipUserProfileImageExtensionsEnabled { get; set; }

		[JsonProperty(PropertyName = "c9s_tweet_anatomy_moderator_badge_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool C9sTweetAnatomyModeratorBadgeEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "tweetypie_unmention_optimization_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool TweetypieUnmentionOptimizationEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "responsive_web_edit_tweet_api_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool ResponsiveWebEditTweetApiEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "graphql_is_translatable_rweb_tweet_is_translatable_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool GraphqlIsTranslatableRwebTweetIsTranslatableEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "view_counts_everywhere_api_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool ViewCountsEverywhereApiEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "longform_notetweets_consumption_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool LongformNotetweetsConsumptionEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "responsive_web_twitter_article_tweet_consumption_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool ResponsiveWebTwitterArticleTweetConsumptionEnabled { get; set; }

		[JsonProperty(PropertyName = "tweet_awards_web_tipping_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool TweetAwardsWebTippingEnabled { get; set; }

		[JsonProperty(PropertyName = "freedom_of_speech_not_reach_fetch_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool FreedomOfSpeechNotReachFetchEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "standardized_nudges_misinfo", NullValueHandling = NullValueHandling.Ignore)]
		public bool StandardizedNudgesMisinfo { get; set; } = true;

		[JsonProperty(PropertyName = "tweet_with_visibility_results_prefer_gql_limited_actions_policy_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool TweetWithVisibilityResultsPreferGqlLimitedActionsPolicyEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "longform_notetweets_rich_text_read_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool LongformNotetweetsRichTextReadEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "longform_notetweets_inline_media_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool LongformNotetweetsInlineMediaEnabled { get; set; } = true;

		[JsonProperty(PropertyName = "responsive_web_media_download_video_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool ResponsiveWebMediaDownloadVideoEnabled { get; set; }

		[JsonProperty(PropertyName = "responsive_web_enhance_cards_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool ResponsiveWebEnhanceCardsEnabled { get; set; }
	}
}
