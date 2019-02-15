namespace LunraGames.SubLight
{
	public enum EncounterTriggers
	{
		Unknown = 0,

		// -- Default Triggers
		Load = 10,

		// -- Navigation View Triggers
		NavigationSelect = 100,

		// -- Transit Triggers
		TransitComplete = 200,

		// -- Resource Triggers
		ResourceRequest = 300,

		// -- System Triggers
		SystemIdle = 400
	}
}