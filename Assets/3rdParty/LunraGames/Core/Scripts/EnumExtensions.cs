using System;
using System.Linq;

namespace LunraGames
{
	public static class EnumExtensions
	{
		public static T[] GetValues<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
		}
	}
}
