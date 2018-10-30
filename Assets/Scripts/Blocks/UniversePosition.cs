using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
#pragma warning disable CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
#pragma warning disable CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
	public struct UniversePosition : IEquatable<UniversePosition>
#pragma warning restore CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
#pragma warning restore CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
	{
		const float UniverseToLightYearScalar = 50f;
		const float LightYearToUniverseScalar = 0.02f;

		public static float ToLightYearDistance(float universeDistance) { return universeDistance * UniverseToLightYearScalar; }
		public static float ToUniverseDistance(float lightYearDistance) { return lightYearDistance * LightYearToUniverseScalar; }

		public static Vector3 ToLightYearDistance(Vector3 universeDistance) { return universeDistance * UniverseToLightYearScalar; }
		public static Vector3 ToUniverseDistance(Vector3 lightYearDistance) { return lightYearDistance * LightYearToUniverseScalar; }

		/// <summary>
		/// Calculates the distance in universe units between two points.
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="universePosition0">Universe position0.</param>
		/// <param name="universePosition1">Universe position1.</param>
		public static float Distance(UniversePosition universePosition0, UniversePosition universePosition1)
		{
			if (universePosition0.Local == universePosition1.Local)
			{
				return Vector3.Distance(universePosition0.Sector, universePosition1.Sector);
			}
			var adjusted0 = universePosition0.Local + universePosition0.Sector;
			var adjusted1 = universePosition1.Local + universePosition1.Sector;
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

		public UniversePosition(Vector3 sector)
		{
			Adjust(Vector3.zero, sector, out Local, out Sector);
		}

		public UniversePosition(Vector3 local, Vector3 sector)
		{
			Adjust(local, sector, out Local, out Sector);
		}

		public UniversePosition(float localX, float localY, float localZ, float sectorX, float sectorY, float sectorZ)
		{
			Adjust(new Vector3(localX, localY, localZ), new Vector3(sectorX, sectorY, sectorZ), out Local, out Sector);
		}
		/// <summary>
		/// The value, from 0 to 1, within a Sector unit.
		/// </summary>
		[JsonProperty] public readonly Vector3 Local;
		/// <summary>
		/// A value with no upper or lower limit. Any decimal amount gets converted into a Local amount.
		/// </summary>
		[JsonProperty] public readonly Vector3 Sector;

		[JsonIgnore]
		public UniversePosition LocalZero { get { return new UniversePosition(Vector3.zero, Sector); } }
		[JsonIgnore]
		public UniversePosition SectorZero { get { return new UniversePosition(Local, Vector3.zero); } }

		public UniversePosition NewLocal(Vector3 local) { return new UniversePosition(local, Sector); }
		public UniversePosition NewSector(Vector3 sector) { return new UniversePosition(Local, sector); }

		public bool LocalEquals(UniversePosition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return Mathf.Approximately(Local.x, other.Local.x) &&
						Mathf.Approximately(Local.y, other.Local.y) &&
						Mathf.Approximately(Local.z, other.Local.z);
		}

		public bool Equals(UniversePosition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return Mathf.Approximately(Local.x, other.Local.x) &&
						Mathf.Approximately(Local.y, other.Local.y) &&
						Mathf.Approximately(Local.z, other.Local.z) &&
						Mathf.Approximately(Sector.x, other.Sector.x) &&
						Mathf.Approximately(Sector.y, other.Sector.y) &&
						Mathf.Approximately(Sector.z, other.Sector.z);
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
			var local = obj0.Local + obj1.Local;
			var sector = obj0.Sector + obj1.Sector;
			return new UniversePosition(local, sector);
		}

		public static UniversePosition operator -(UniversePosition obj0, UniversePosition obj1)
		{
			var local = obj0.Local - obj1.Local;
			var sector = obj0.Sector - obj1.Sector;
			return new UniversePosition(local, sector);
		}

		static void Adjust(Vector3 local, Vector3 sector, out Vector3 adjustedLocal, out Vector3 adjustedSector)
		{
			var localRemainder = new Vector3(local.x % 1f, local.y % 1f, local.z % 1f);
			sector += localRemainder;
			var sectorRemainder = new Vector3(sector.x % 1f, sector.y % 1f, sector.z % 1f);
			local += sector - sectorRemainder;
			local -= localRemainder;
			sector = sectorRemainder;

			if (sector.x < 0f) 
			{
				sector = sector.NewX(sector.x + Mathf.Floor(Mathf.Abs(sector.x)));
				local = local.NewX(local.x - 1f);
				sector = sector.NewX(sector.x + 1f);
			}
			if (sector.y < 0f)
			{
				sector = sector.NewY(sector.y + Mathf.Floor(Mathf.Abs(sector.y)));
				local = local.NewY(local.y - 1f);
				sector = sector.NewY(sector.y + 1f);
			}
			if (sector.z < 0f)
			{
				sector = sector.NewZ(sector.z + Mathf.Floor(Mathf.Abs(sector.z)));
				local = local.NewZ(local.z - 1f);
				sector = sector.NewZ(sector.z + 1f);
			}

			sector = new Vector3(sector.x % 1f, sector.y % 1f, sector.z % 1f);

			adjustedLocal = local;
			adjustedSector = sector;
		}

		public override string ToString()
		{
			return "[ " + Local.x + ", " + Local.y + ", " + Local.z + " ] ( " + Sector.x + ", " + Sector.y + ", " + Sector.z + " )";
		}
	}
}