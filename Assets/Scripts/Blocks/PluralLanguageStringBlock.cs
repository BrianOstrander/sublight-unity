using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct PluralLanguageStringBlock
	{
		public LanguageStringModel Singular;
		public LanguageStringModel Plural;

		public LanguageStringModel Get(float value) { return Mathf.Approximately(value, 1f) ? Singular : Plural; }

		public PluralLanguageStringBlock(LanguageStringModel singular, LanguageStringModel plural = null)
		{
			Singular = singular;
			Plural = plural ?? singular;
		}
	}
}