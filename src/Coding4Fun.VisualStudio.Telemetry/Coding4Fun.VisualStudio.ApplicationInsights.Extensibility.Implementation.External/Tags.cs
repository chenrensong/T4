using System;
using System.Collections.Generic;
using System.Globalization;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Base class for tags backed context.
	/// </summary>
	internal static class Tags
	{
		internal static bool? GetTagBoolValueOrNull(this IDictionary<string, string> tags, string tagKey)
		{
			string tagValueOrNull = tags.GetTagValueOrNull(tagKey);
			if (string.IsNullOrEmpty(tagValueOrNull))
			{
				return null;
			}
			return bool.Parse(tagValueOrNull);
		}

		internal static int? GetTagIntValueOrNull(this IDictionary<string, string> tags, string tagKey)
		{
			string tagValueOrNull = tags.GetTagValueOrNull(tagKey);
			if (string.IsNullOrEmpty(tagValueOrNull))
			{
				return null;
			}
			return int.Parse(tagValueOrNull, CultureInfo.InvariantCulture);
		}

		internal static DateTimeOffset? GetTagDateTimeOffsetValueOrNull(this IDictionary<string, string> tags, string tagKey)
		{
			string tagValueOrNull = tags.GetTagValueOrNull(tagKey);
			if (string.IsNullOrEmpty(tagValueOrNull))
			{
				return null;
			}
			return DateTimeOffset.Parse(tagValueOrNull, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		}

		internal static void SetStringValueOrRemove(this IDictionary<string, string> tags, string tagKey, string tagValue)
		{
			tags.SetTagValueOrRemove(tagKey, tagValue);
		}

		internal static void SetDateTimeOffsetValueOrRemove(this IDictionary<string, string> tags, string tagKey, DateTimeOffset? tagValue)
		{
			if (!tagValue.HasValue)
			{
				tags.SetTagValueOrRemove(tagKey, tagValue);
				return;
			}
			string tagValue2 = tagValue.Value.ToString("o", CultureInfo.InvariantCulture);
			tags.SetTagValueOrRemove(tagKey, tagValue2);
		}

		internal static void SetTagValueOrRemove<T>(this IDictionary<string, string> tags, string tagKey, T tagValue)
		{
			tags.SetTagValueOrRemove(tagKey, Convert.ToString(tagValue, CultureInfo.InvariantCulture));
		}

		internal static void InitializeTagValue<T>(this IDictionary<string, string> tags, string tagKey, T tagValue)
		{
			if (!tags.ContainsKey(tagKey))
			{
				tags.SetTagValueOrRemove(tagKey, tagValue);
			}
		}

		internal static void InitializeTagDateTimeOffsetValue(this IDictionary<string, string> tags, string tagKey, DateTimeOffset? tagValue)
		{
			if (!tags.ContainsKey(tagKey))
			{
				tags.SetDateTimeOffsetValueOrRemove(tagKey, tagValue);
			}
		}

		internal static string GetTagValueOrNull(this IDictionary<string, string> tags, string tagKey)
		{
			if (tags.TryGetValue(tagKey, out string value))
			{
				return value;
			}
			return null;
		}

		private static void SetTagValueOrRemove(this IDictionary<string, string> tags, string tagKey, string tagValue)
		{
			if (string.IsNullOrEmpty(tagValue))
			{
				tags.Remove(tagKey);
			}
			else
			{
				tags[tagKey] = tagValue;
			}
		}
	}
}
