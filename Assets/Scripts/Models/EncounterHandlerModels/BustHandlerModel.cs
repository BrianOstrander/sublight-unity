﻿using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class BustHandlerModel : EncounterHandlerModel<BustEncounterLogModel>
	{
		[JsonProperty] BustEdgeModel[] entries;
		[JsonIgnore] public readonly ListenerProperty<BustEdgeModel[]> Entries;
		
		[JsonProperty] bool hasHaltingEvents;
		[JsonIgnore] public readonly ListenerProperty<bool> HasHaltingEvents;
		
		Action haltingDone;
		[JsonIgnore] public readonly ListenerProperty<Action> HaltingDone;

		public BustHandlerModel(BustEncounterLogModel log) : base(log)
		{
			Entries = new ListenerProperty<BustEdgeModel[]>(value => entries = value, () => entries);
			HasHaltingEvents = new ListenerProperty<bool>(value => hasHaltingEvents = value, () => hasHaltingEvents);
			HaltingDone = new ListenerProperty<Action>(value => haltingDone = value, () => haltingDone);
		}
	}
}