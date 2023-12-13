using System.Text.Json.Serialization;

namespace NSTwitterApi.Models.Json
{
    public class Features
    {
        [JsonPropertyName("c9s_tweet_anatomy_moderator_badge_enabled")]
        public bool C9sTweetAnatomyModeratorBadgeEnabled { get; set; } = true;

        [JsonPropertyName("tweetypie_unmention_optimization_enabled")]
        public bool TweetypieUnmentionOptimizationEnabled { get; set; } = true;

        [JsonPropertyName("responsive_web_edit_tweet_api_enabled")]
        public bool ResponsiveWebEditTweetApiEnabled { get; set; } = true;

        [JsonPropertyName("graphql_is_translatable_rweb_tweet_is_translatable_enabled")]
        public bool GraphqlIsTranslatableRwebTweetIsTranslatableEnabled { get; set; } = true;

        [JsonPropertyName("view_counts_everywhere_api_enabled")]
        public bool ViewCountsEverywhereApiEnabled { get; set; } = true;

        [JsonPropertyName("longform_notetweets_consumption_enabled")]
        public bool LongformNotetweetsConsumptionEnabled { get; set; } = true;

        [JsonPropertyName("responsive_web_twitter_article_tweet_consumption_enabled")]
        public bool ResponsiveWebTwitterArticleTweetConsumptionEnabled { get; set; } = true;

        [JsonPropertyName("tweet_awards_web_tipping_enabled")]
        public bool TweetAwardsWebTippingEnabled { get; set; } = false;

        [JsonPropertyName("responsive_web_home_pinned_timelines_enabled")]
        public bool ResponsiveWebHomePinnedTimelinesEnabled { get; set; } = true;

        [JsonPropertyName("longform_notetweets_rich_text_read_enabled")]
        public bool LongformNotetweetsRichTextReadEnabled { get; set; } = true;

        [JsonPropertyName("longform_notetweets_inline_media_enabled")]
        public bool LongformNotetweetsInlineMediaEnabled { get; set; } = true;

        [JsonPropertyName("responsive_web_graphql_exclude_directive_enabled")]
        public bool ResponsiveWebGraphqlExcludeDirectiveEnabled { get; set; } = true;

        [JsonPropertyName("verified_phone_label_enabled")]
        public bool VerifiedPhoneLabelEnabled { get; set; } = false;

        [JsonPropertyName("freedom_of_speech_not_reach_fetch_enabled")]
        public bool FreedomOfSpeechNotReachFetchEnabled { get; set; } = true;

        [JsonPropertyName("standardized_nudges_misinfo")]
        public bool StandardizedNudgesMisinfo { get; set; } = true;

        [JsonPropertyName("tweet_with_visibility_results_prefer_gql_limited_actions_policy_enabled")]
        public bool TweetWithVisibilityResultsPreferGqlLimitedActionsPolicyEnabled { get; set; } = true;

        [JsonPropertyName("responsive_web_media_download_video_enabled")]
        public bool ResponsiveWebMediaDownloadVideoEnabled { get; set; } = false;

        [JsonPropertyName("responsive_web_graphql_skip_user_profile_image_extensions_enabled")]
        public bool ResponsiveWebGraphqlSkipUserProfileImageExtensionsEnabled { get; set; } = false;

        [JsonPropertyName("responsive_web_graphql_timeline_navigation_enabled")]
        public bool ResponsiveWebGraphqlTimelineNavigationEnabled { get; set; } = true;

        [JsonPropertyName("responsive_web_enhance_cards_enabled")]
        public bool ResponsiveWebEnhanceCardsEnabled { get; set; } = false;
    }

}