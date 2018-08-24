namespace LunraGames.SubLight
{
	public struct SaveStateBlock
	{
		public static SaveStateBlock Savable() { return new SaveStateBlock(true, null); }
		public static SaveStateBlock NotSavable(string reason) { return new SaveStateBlock(false, reason); }

		public readonly bool CanSave;
		public readonly string Reason;

		public SaveStateBlock(bool canSave, string reason)
		{
			CanSave = canSave;
			Reason = string.IsNullOrEmpty(reason) ? "No reason provided" : reason;
		}
	}
}