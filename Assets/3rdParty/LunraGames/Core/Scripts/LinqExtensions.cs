using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LunraGames
{
	public static class LinqExtensions
	{
		public static IEnumerable<string> FriendlyMatch(this IEnumerable<string> entries, string search)
		{
			return FriendlyMatch(entries, search, s => s);
		}

		public static IEnumerable<T> FriendlyMatch<T>(this IEnumerable<T> entries, string search, Func<T, string> keySelector)
		{
			if (string.IsNullOrEmpty(search)) return entries;
			var pattern = string.Empty;
			foreach (var character in search) pattern += character+".*";
			var regex = new Regex(pattern, RegexOptions.IgnoreCase);
			return entries.Where(e => regex.IsMatch(keySelector(e)));
		}

		public static T FirstOrFallback<T>(this IEnumerable<T> entries, Func<T, bool> predicate, T fallback = default(T))
		{
			return entries.DefaultIfEmpty(fallback).FirstOrDefault(predicate);
		}

		public static T FirstOrFallback<T>(this IEnumerable<T> entries, T fallback = default(T))
		{
			return entries.DefaultIfEmpty(fallback).FirstOrDefault();
		}

		public static T Random<T>(this IEnumerable<T> entries, T fallback = default(T))
		{
			if (entries == null || entries.Count() == 0) return fallback;
			return entries.ElementAt(UnityEngine.Random.Range(0, entries.Count()));
		}
	}
}