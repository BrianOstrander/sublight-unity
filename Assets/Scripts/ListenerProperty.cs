using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
	public class ListenerProperty<T>
	{
		Action<T> set;
		Func<T> get;

		public Action<T> Changed = value => { };

		public T Value
		{
			get { return get(); }
			set
			{
				if (EqualityComparer<T>.Default.Equals(get(), value)) return;
				set(value);
				Changed(value);
			}
		}

		public ListenerProperty(Action<T> set, Func<T> get, params Action<T>[] listeners)
		{
			this.set = set;
			this.get = get;

			foreach (var listener in listeners) Changed += listener;
		}

		/// <summary>
		/// Converts the ModelProperty to the associated type.
		/// </summary>
		/// <remarks>
		/// Only one way casting is supported so that the callbacks bound to the Changed action aren't lost.
		/// </remarks>
		/// <returns>The implicit.</returns>
		/// <param name="p">P.</param>
		public static implicit operator T(ListenerProperty<T> p)
		{
			return p.Value;
		}
	}
}