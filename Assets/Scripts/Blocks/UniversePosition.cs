using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
#pragma warning disable CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
#pragma warning disable CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
	public struct UniversePosition : IEquatable<UniversePosition>
#pragma warning restore CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
#pragma warning restore CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
	{
		const float UnityToUniverseScalar = 0.02f;
		const float UniverseToUnityScalar = 50f;
		const float UniverseToLightYearScalar = 25f;

		/// <summary>
		/// The current sector offset of all unity units.
		/// </summary>
		static UniversePosition CurrentOffset = Zero;

		/// <summary>
		/// Updates the sector offset upon request.
		/// </summary>
		/// <remarks>
		/// Should be added as a listener in the initialize state.
		/// </remarks>
		/// <param name="request">Request.</param>
		public static void OnUniversePositionRequest(UniversePositionRequest request)
		{
			switch(request.State)
			{
				case UniversePositionRequest.States.Request:
					CurrentOffset = request.Position;
					App.Callbacks.UniversePositionRequest(request.Duplicate(UniversePositionRequest.States.Complete));
					break;
			}
		}

		public static float ToUniverseDistance(float unityDistance) { return unityDistance * UnityToUniverseScalar; }

		public static float ToUnityDistance(float universeDistance) { return universeDistance * UniverseToUnityScalar; }

		public static float ToLightYearDistance(float universeDistance) { return universeDistance * UniverseToLightYearScalar; }

		public static UniversePosition ToUniverse(Vector3 unityPosition)
		{
			return new UniversePosition(CurrentOffset.Sector, unityPosition * UnityToUniverseScalar);
		}

		public static Vector3 ToUnity(UniversePosition universePosition)
		{
			return ((universePosition.Sector - CurrentOffset.Sector) + universePosition.System) * UniverseToUnityScalar;
		}

		/// <summary>
		/// Calculates the distance in universe units between two points.
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="universePosition0">Universe position0.</param>
		/// <param name="universePosition1">Universe position1.</param>
		public static float Distance(UniversePosition universePosition0, UniversePosition universePosition1)
		{
			if (universePosition0.Sector == universePosition1.Sector)
			{
				return Vector3.Distance(universePosition0.System, universePosition1.System);
			}
			var adjusted0 = universePosition0.Sector + universePosition0.System;
			var adjusted1 = universePosition1.Sector + universePosition1.System;
			return Vector3.Distance(adjusted0, adjusted1);
		}

		/// <summary>
		/// Calculates the travel time between two points, when supplied with a speed in universe units per day.
		/// </summary>
		/// <returns>The time.</returns>
		/// <param name="universePosition0">Universe position0.</param>
		/// <param name="universePosition1">Universe position1.</param>
		/// <param name="speed">Speed in universe units per day.</param>
		public static DayTime TravelTime(UniversePosition universePosition0, UniversePosition universePosition1, float speed)
		{
			return new DayTime(Distance(universePosition0, universePosition1) / speed);
		}

		public static UniversePosition Zero { get { return new UniversePosition(Vector3.zero, Vector3.zero); } }

		public UniversePosition(Vector3 system)
		{
			Adjust(Vector3.zero, system, out Sector, out System);
		}

		public UniversePosition(Vector3 sector, Vector3 system)
		{
			Adjust(sector, system, out Sector, out System);
		}

		public UniversePosition(float sectorX, float sectorY, float sectorZ, float systemX, float systemY, float systemZ)
		{
			Adjust(new Vector3(sectorX, sectorY, sectorZ), new Vector3(systemX, systemY, systemZ), out Sector, out System);
		}

		[JsonProperty] public readonly Vector3 Sector;
		[JsonProperty] public readonly Vector3 System;

		[JsonIgnore]
		public Vector3 Normalized { get { return ToUnity(this + CurrentOffset).normalized; } }
		[JsonIgnore]
		public UniversePosition SectorZero { get { return new UniversePosition(Vector3.zero, System); } }
		[JsonIgnore]
		public UniversePosition SystemZero { get { return new UniversePosition(Sector, Vector3.zero); } }

		public UniversePosition NewSector(Vector3 sector) { return new UniversePosition(sector, System); }
		public UniversePosition NewSystem(Vector3 system) { return new UniversePosition(Sector, system); }

		public bool SectorEquals(UniversePosition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return Mathf.Approximately(Sector.x, other.Sector.x) &&
						Mathf.Approximately(Sector.y, other.Sector.y) &&
						Mathf.Approximately(Sector.z, other.Sector.z);
		}

		public bool Equals(UniversePosition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return Mathf.Approximately(Sector.x, other.Sector.x) &&
						Mathf.Approximately(Sector.y, other.Sector.y) &&
						Mathf.Approximately(Sector.z, other.Sector.z) &&
						Mathf.Approximately(System.x, other.System.x) &&
						Mathf.Approximately(System.y, other.System.y) &&
						Mathf.Approximately(System.z, other.System.z);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;

			return obj.GetType() == GetType() && Equals((UniversePosition)obj);
		}

		public static bool operator ==(UniversePosition obj0, UniversePosition obj1)
		{
			if (ReferenceEquals(obj0, obj1)) return true;
			if (ReferenceEquals(obj0, null)) return false;
			if (ReferenceEquals(obj1, null)) return false;
			return obj0.Equals(obj1);
		}

		public static bool operator !=(UniversePosition obj0, UniversePosition obj1) { return !(obj0 == obj1); }

		public static UniversePosition operator +(UniversePosition obj0, UniversePosition obj1)
		{
			var sector = obj0.Sector + obj1.Sector;
			var system = obj0.System + obj1.System;
			return new UniversePosition(sector, system);
		}

		public static UniversePosition operator -(UniversePosition obj0, UniversePosition obj1)
		{
			var sector = obj0.Sector - obj1.Sector;
			var system = obj0.System - obj1.System;
			return new UniversePosition(sector, system);
		}

		static void Adjust(Vector3 sector, Vector3 system, out Vector3 adjustedSector, out Vector3 adjustedSystem)
		{
			var sectorRemainder = new Vector3(sector.x % 1f, sector.y % 1f, sector.z % 1f);
			system += sectorRemainder;
			var systemRemainder = new Vector3(system.x % 1f, system.y % 1f, system.z % 1f);
			sector += system - systemRemainder;
			sector -= sectorRemainder;
			system = systemRemainder;

			if (system.x < 0f) 
			{
				system = system.NewX(system.x + Mathf.Floor(Mathf.Abs(system.x)));
				sector = sector.NewX(sector.x - 1f);
				system = system.NewX(system.x + 1f);
			}
			if (system.y < 0f)
			{
				system = system.NewY(system.y + Mathf.Floor(Mathf.Abs(system.y)));
				sector = sector.NewY(sector.y - 1f);
				system = system.NewY(system.y + 1f);
			}
			if (system.z < 0f)
			{
				system = system.NewZ(system.z + Mathf.Floor(Mathf.Abs(system.z)));
				sector = sector.NewZ(sector.z - 1f);
				system = system.NewZ(system.z + 1f);
			}

			system = new Vector3(system.x % 1f, system.y % 1f, system.z % 1f);

			adjustedSector = sector;
			adjustedSystem = system;
		}

		public override string ToString()
		{
			return "[ " + Sector.x + ", " + Sector.y + ", " + Sector.z + " ] ( " + System.x + ", " + System.y + ", " + System.z + " )";
		}
	}
}