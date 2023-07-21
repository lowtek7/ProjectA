using System;

namespace Core.Utility
{
	/// <summary>
	/// Unix timestamp converter
	/// Inspired by "How can I convert a Unix timestamp to DateTime and vice versa?"
	/// https://stackoverflow.com/a/250400
	/// </summary>
	public static class UnixTimeConverter
	{
		public static DateTime UnixTimeToDateTime(uint unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
			return dateTime;
		}

		public static uint ToUnixTime(this DateTime dateTime)
		{
			return (uint)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
		}
	}
}
