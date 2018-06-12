using System;

using UnityEngine;

namespace LunraGames.SpaceFarm
{
#pragma warning disable CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
#pragma warning disable CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
	public struct UniversePosition : IEquatable<UniversePosition>
#pragma warning restore CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
#pragma warning restore CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
	{
		public static UniversePosition Zero { get { return new UniversePosition(Vector3.zero, Vector3.zero); } }

		public UniversePosition(Vector3 sector, Vector3 system)
		{
			Sector = sector;
			System = system;
		}

		public Vector3 Sector;
		public Vector3 System;

		public bool Equals(UniversePosition other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return other.Sector == Sector && other.System == System;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;

			return obj.GetType() == GetType() && Equals((UniversePosition)obj);
		}

		public static bool operator ==(UniversePosition obj1, UniversePosition obj2)
		{
			if (ReferenceEquals(obj1, obj2)) return true;
			if (ReferenceEquals(obj1, null)) return false;
			if (ReferenceEquals(obj2, null)) return false;
			return obj1.Equals(obj2);
		}

		public static bool operator !=(UniversePosition obj1, UniversePosition obj2) { return !(obj1 == obj2); }
	}
}