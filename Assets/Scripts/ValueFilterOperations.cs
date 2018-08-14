namespace LunraGames.SubLight
{
	public enum EncounterInteractionFilterOperations
	{
		Unknown = 0,
		NeverSeen = 10,
		Seen = 20,
		NotCompleted = 30,
		Completed = 40
	}

	public enum StringFilterOperations
	{
		Unknown = 0,
		Equals = 10,
		IsNullOrEmpty = 20,
		IsNull = 30
	}

	/* I don't think we need this... it's the only thing you can do with bools...
	public enum BooleanValueFilterOperations
	{
		Unknown = 0,
		Equals = 10
	}
	*/


}