using System;

namespace LunraGames.SpaceFarm
{
	public struct EscapeEntry
	{
		public Action Escape;
		public bool? IsShaded;
		public bool? IsObscured;
		public Func<bool> Enabled;

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
