﻿namespace LunraGames.SubLight
{
	public enum EncounterTriggers
	{
		Unknown = 0,

		// -- Default Triggers
		/// <summary>
		/// Should only be used for encounters that are triggered in some other way.
		/// </summary>
		/// <remarks>
		/// These will never trigger on their own.
		/// </remarks>
		None = 5,
		/// <summary>
		/// Called every time the game loads.
		/// </summary>
		Load = 10,
		/// <summary>
		/// Called every time the game loads to set basic rules and default
		/// values.
		/// </summary>
		InitializeRules = 20,

		// -- Navigation View Triggers
		/// <summary>
		/// When the player selects a celestial system in the navigation view.
		/// </summary>
		NavigationSelect = 100,
		/// <summary>
		/// When the player selects a celestial system in the navigation view,
		/// but it's out of range.
		/// </summary>
		NavigationSelectOutOfRange = 110,

		// -- Transit Triggers
		/// <summary>
		/// When the player's ark completes a transit.
		/// </summary>
		/// <remarks>
		/// Checking if the ark has been consumed by the void should likely be
		/// done here.
		/// </remarks>
		TransitComplete = 200,

		// -- Resource Triggers
		/// <summary>
		/// When resources are gathered for the ark.
		/// </summary>
		ResourceRequest = 300,
		/// <summary>
		/// When resources are consumed by the ark.
		/// </summary>
		ResourceConsume = 310,

		// -- System Triggers
		/// <summary>
		/// When the player is idle in a system, this is when most resource
		/// related fail scenarios should happen.
		/// </summary>
		SystemIdle = 400
	}
}