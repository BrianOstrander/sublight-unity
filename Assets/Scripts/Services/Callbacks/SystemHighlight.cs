using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
#pragma warning disable CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
#pragma warning disable CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
	public struct SystemHighlight
#pragma warning restore CS0659 // Overrides Object.Equals(object) but does not override Object.GetHashCode()
#pragma warning restore CS0661 // Defines == or != operator but does not override Ojbect.GetHashCode()
	{
		public enum States
		{
			Unknown = 0,
			Begin = 10,
			Change = 20,
			End = 30
		}

		public static SystemHighlight None { get { return new SystemHighlight(States.End, null); } }

		public readonly States State;
		public readonly SystemModel System;

		public SystemHighlight(States state, SystemModel system)
		{
			State = state;
			System = system;
		}

		public bool Equals(SystemHighlight other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return other.State == State && other.System == System;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;

			return obj.GetType() == GetType() && Equals((SystemHighlight)obj);
		}

		public static bool operator ==(SystemHighlight obj0, SystemHighlight obj1)
		{
			if (ReferenceEquals(obj0, obj1)) return true;
			if (ReferenceEquals(obj0, null)) return false;
			if (ReferenceEquals(obj1, null)) return false;
			return obj0.Equals(obj1);
		}

		public static bool operator !=(SystemHighlight obj0, SystemHighlight obj1) { return !(obj0 == obj1); }
	}
}