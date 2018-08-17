using System;

namespace LunraGames.SubLight
{
	public struct EscapeEntry
	{
		public readonly Action Escape;
		public readonly bool? IsShaded;
		public readonly bool? IsObscured;
		public readonly Func<bool> Enabled;

		public EscapeEntry(
			Action escape, 
			bool? isShaded = null, 
			bool? isObscured = null,
			Func<bool> enabled = null
		)
		{
			Escape = escape;
			IsShaded = isShaded;
			IsObscured = isObscured;
			Enabled = enabled ?? (() => true);
		}
	}
}
