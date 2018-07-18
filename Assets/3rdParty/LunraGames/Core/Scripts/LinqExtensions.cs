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

		public static T LastOrFallback<T>(this IEnumerable<T> entries, Func<T, bool> predicate, T fallback = default(T))
		{
			return entries.DefaultIfEmpty(fallback).LastOrDefault(predicate);
		}

		public static T LastOrFallback<T>(this IEnumerable<T> entries, T fallback = default(T))
		{
			return entries.DefaultIfEmpty(fallback).LastOrDefault();
		}

		public static T Random<T>(this IEnumerable<T> entries, T fallback = default(T))
		{
			if (entries == null || entries.Count() == 0) return fallback;
			return entries.ElementAt(UnityEngine.Random.Range(0, entries.Count()));
		}

		public static IEnumerable<T> Append<T>(this IEnumerable<T> entries, T element)
		{
			if (entries == null) throw new ArgumentNullException("entries");
			return ConcatIterator(entries, element, false);
		}

		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> entries, T element)
		{
			if (entries == null) throw new ArgumentNullException("entries");
			return ConcatIterator(entries, element, true);
		}

		static IEnumerable<T> ConcatIterator<T>(IEnumerable<T> entries, T element, bool start)
		{
			if (start) yield return element;
			foreach (var entry in entries) yield return entry;
			if (!start) yield return element;
		}

		public static bool ContainsOrIsEmpty<T>(this IEnumerable<T> entries, T element)
		{
			if (entries.Count() == 0) return true;
			return entries.Contains(element);
		}
	}
}