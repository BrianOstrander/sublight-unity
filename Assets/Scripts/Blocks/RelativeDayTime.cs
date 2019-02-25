using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight
{
	[Serializable]
#pragma warning disable CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
#pragma warning disable CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
	public struct RelativeDayTime : IEquatable<RelativeDayTime>
#pragma warning restore CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
#pragma warning restore CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
	{
		public static RelativeDayTime Zero { get { return new RelativeDayTime(DayTime.Zero, DayTime.Zero); } }

		[JsonProperty] public readonly DayTime ShipTime;
		[JsonProperty] public readonly DayTime GalacticTime;

		public RelativeDayTime(
			DayTime shipTime,
			DayTime galacticTime
		)
		{
			ShipTime = shipTime;
			GalacticTime = galacticTime;
		}

		public static RelativeDayTime operator +(RelativeDayTime obj0, RelativeDayTime obj1)
		{
			return new RelativeDayTime(
				obj0.ShipTime + obj1.ShipTime,
				obj0.GalacticTime + obj1.GalacticTime
			);
		}

		public static RelativeDayTime operator -(RelativeDayTime obj0, RelativeDayTime obj1)
		{
			return new RelativeDayTime(
				obj0.ShipTime - obj1.ShipTime,
				obj0.GalacticTime - obj1.GalacticTime
			);
		}

		public static RelativeDayTime operator *(RelativeDayTime obj, float value)
		{
			return new RelativeDayTime(
				obj.ShipTime * value,
				obj.GalacticTime * value
			);
		}

		public static RelativeDayTime operator *(float value, RelativeDayTime obj)
		{
			return obj * value;
		}

		public bool Equals(RelativeDayTime other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return ShipTime == other.ShipTime && GalacticTime == other.GalacticTime;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;

			return obj.GetType() == GetType() && Equals((RelativeDayTime)obj);
		}

		public static bool operator ==(RelativeDayTime obj0, RelativeDayTime obj1)
		{
			if (ReferenceEquals(obj0, obj1)) return true;
			if (ReferenceEquals(obj0, null)) return false;
			if (ReferenceEquals(obj1, null)) return false;
			return obj0.Equals(obj1);
		}

		public static bool operator !=(RelativeDayTime obj0, RelativeDayTime obj1) { return !(obj0 == obj1); }

		public override string ToString()
		{
			return "Ship\n\t" + ShipTime.ToString() + "\nGalactic\n\t" + GalacticTime.ToString();
		}
	}
}