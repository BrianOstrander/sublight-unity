﻿using System;

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
		public enum Formats
		{
			Unknown = 0,
			None = 10,
			Coordinate = 20
		}

		static string FormatPrefix(Formats format)
		{
			return Enum.GetName(typeof(Formats), format).ToLower() + "_";
		}

		public const float UniverseToLightYearScalar = 500f;
		public const float LightYearToUniverseScalar = 0.002f;

		public static float ToLightYearDistance(float universeDistance) { return universeDistance * UniverseToLightYearScalar; }
		public static float ToUniverseDistance(float lightYearDistance) { return lightYearDistance * LightYearToUniverseScalar; }

		public static Vector3 ToLightYearDistance(Vector3 universeDistance) { return universeDistance * UniverseToLightYearScalar; }
		public static Vector3 ToUniverseDistance(Vector3 lightYearDistance) { return lightYearDistance * LightYearToUniverseScalar; }

		public static UniversePosition Lerp(
			float progress,
			UniversePosition begin,
			UniversePosition end
		)
		{
			var delta = end - begin;
			return begin + (delta * progress);
		}

		public static UniversePosition Lerp(
			Vector3 progress,
			UniversePosition begin,
			UniversePosition end
		)
		{
			var delta = (end - begin).Lossy;
			delta.Scale(progress);
			return begin + new UniversePosition(delta, Vector3.zero);
		}

		/// <summary>
		/// Gets a normalized Vector3 from the provided UniversePositions.
		/// Useful for getting a normal value of where something is within an
		/// area.
		/// </summary>
		/// <returns>The sector.</returns>
		/// <param name="delta">Delta.</param>
		/// <param name="area">Area.</param>
		public static Vector3 NormalizedSector(
			UniversePosition delta,
			UniversePosition area
		)
		{
			var sectorDelta = delta.Sector;
			var sectorArea = area.Sector;
			return new Vector3(sectorDelta.x / sectorArea.x, sectorDelta.y / sectorArea.y, sectorDelta.z / sectorArea.z);
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
				return Vector3.Distance(universePosition0.Local, universePosition1.Local);
			}
			var adjusted0 = universePosition0.Sector + universePosition0.Local;
			var adjusted1 = universePosition1.Sector + universePosition1.Local;
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

		public static void Expand(
			string value,
			Action none = null,
			Action<UniversePosition, int> coordinate = null
		)
		{
			if (string.IsNullOrEmpty(value))
			{
				if (none != null) none();
			}
			else if (value.StartsWith(FormatPrefix(Formats.Coordinate))) ExpandCoordinate(value, none, coordinate);
			else
			{
				Debug.LogError("Unrecognized format for value: " + value);
				if (none != null) none();
			}
		}

		static void ExpandCoordinate(
			string value,
			Action none,
			Action<UniversePosition, int> coordinate
		)
		{
			try
			{
				value = value.Remove(0, FormatPrefix(Formats.Coordinate).Length);

				var elements = value.Split('_');
				if (elements.Length != 4) throw new Exception("Unable to expand \"" + value + "\", 4 elements required");

				var position = new UniversePosition(
					new Vector3Int(
						int.Parse(elements[0]),
						int.Parse(elements[1]),
						int.Parse(elements[2])
					)
				);
				var index = int.Parse(elements[3]);

				if (coordinate != null) coordinate(position, index);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				Debug.LogError("Unable to expand and parse \"" + value + "\", returning false");
				if (none != null) none();
			}
		}

		public static string Shrink(UniversePosition position, int index = -1)
		{
			var sector = position.SectorInteger;
			return FormatPrefix(Formats.Coordinate) + sector.x + "_" + sector.y + "_" + sector.z + "_" + index;
		}

		public static UniversePosition Zero { get { return new UniversePosition(Vector3.zero, Vector3.zero); } }
		public static UniversePosition One { get { return new UniversePosition(Vector3.one, Vector3.zero); } }

		public UniversePosition(Vector3Int sector)
		{
			Adjust(new Vector3(sector.x, sector.y, sector.z), Vector3.zero, out Sector, out Local);
		}

		public UniversePosition(Vector3 local)
		{
			Adjust(Vector3.zero, local, out Sector, out Local);
		}

		public UniversePosition(Vector3 sector, Vector3 local)
		{
			Adjust(sector, local, out Sector, out Local);
		}

		public UniversePosition(Vector3Int sector, Vector3 local)
		{
			Adjust(new Vector3(sector.x, sector.y, sector.z), local, out Sector, out Local);
		}

		public UniversePosition(float sectorX, float sectorY, float sectorZ, float localX, float localY, float localZ)
		{
			Adjust(new Vector3(sectorX, sectorY, sectorZ), new Vector3(localX, localY, localZ), out Sector, out Local);
		}
		/// <summary>
		/// A value with no upper or lower limit. Any decimal amount gets converted into a Local amount.
		/// </summary>
		[JsonProperty] public readonly Vector3 Sector;
		/// <summary>
		/// The value, from 0 to 1, within a Sector unit.
		/// </summary>
		[JsonProperty] public readonly Vector3 Local;

		[JsonIgnore]
		public UniversePosition SectorZero { get { return new UniversePosition(Vector3.zero, Local); } }
		[JsonIgnore]
		public UniversePosition LocalZero { get { return new UniversePosition(Sector, Vector3.zero); } }
		[JsonIgnore]
		public Vector3Int SectorInteger { get { return new Vector3Int((int)Sector.x, (int)Sector.y, (int)Sector.z); } }
		[JsonIgnore]
		public Vector3 Lossy { get { return Sector + Local; } }

		public UniversePosition NewSector(Vector3 sector) { return new UniversePosition(sector, Local); }
		public UniversePosition NewLocal(Vector3 local) { return new UniversePosition(Sector, local); }

		public bool SectorEquals(UniversePosition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return SectorEquals(other.SectorInteger);
		}

		public bool SectorEquals(Vector3Int other)
		{
			return SectorInteger == other;
		}

		public bool Equals(UniversePosition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return Mathf.Approximately(Sector.x, other.Sector.x) &&
						Mathf.Approximately(Sector.y, other.Sector.y) &&
						Mathf.Approximately(Sector.z, other.Sector.z) &&
						Mathf.Approximately(Local.x, other.Local.x) &&
						Mathf.Approximately(Local.y, other.Local.y) &&
						Mathf.Approximately(Local.z, other.Local.z);
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
			var local = obj0.Local + obj1.Local;
			return new UniversePosition(sector, local);
		}

		public static UniversePosition operator -(UniversePosition obj0, UniversePosition obj1)
		{
			var sector = obj0.Sector.ApproximateSubtract(obj1.Sector);
			var local = obj0.Local.ApproximateSubtract(obj1.Local);
			return new UniversePosition(sector, local);
		}

		public static UniversePosition operator *(float value, UniversePosition obj)
		{
			return new UniversePosition(obj.Sector * value, obj.Local * value);
		}

		public static UniversePosition operator *(UniversePosition obj, float value)
		{
			return value * obj;
		}

		static void Adjust(Vector3 sector, Vector3 local, out Vector3 adjustedSector, out Vector3 adjustedLocal)
		{
			var sectorRemainder = new Vector3(sector.x % 1f, sector.y % 1f, sector.z % 1f);
			local += sectorRemainder;
			var localRemainder = new Vector3(local.x % 1f, local.y % 1f, local.z % 1f);
			sector += local - localRemainder;
			sector -= sectorRemainder;
			local = localRemainder;

			if (local.x < 0f) 
			{
				local = local.NewX(local.x + Mathf.Floor(Mathf.Abs(local.x)));
				sector = sector.NewX(sector.x - 1f);
				local = local.NewX(local.x + 1f);
			}
			if (local.y < 0f)
			{
				local = local.NewY(local.y + Mathf.Floor(Mathf.Abs(local.y)));
				sector = sector.NewY(sector.y - 1f);
				local = local.NewY(local.y + 1f);
			}
			if (local.z < 0f)
			{
				local = local.NewZ(local.z + Mathf.Floor(Mathf.Abs(local.z)));
				sector = sector.NewZ(sector.z - 1f);
				local = local.NewZ(local.z + 1f);
			}

			local = new Vector3(local.x % 1f, local.y % 1f, local.z % 1f);

			adjustedSector = sector;
			adjustedLocal = local;
		}

		public override string ToString()
		{
			return "[ " + Sector.x + ", " + Sector.y + ", " + Sector.z + " ] ( " + Local.x + ", " + Local.y + ", " + Local.z + " )";
		}

		public string ToByteString()
		{
			return "Sector:\n" + Sector.ToBytesString() + "\nLocal:\n" + Local.ToBytesString();
		}
	}
}