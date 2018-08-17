using System;
using UnityEngine;

namespace LunraGames.SubLight 
{
	public class Heartbeat 
	{
		public Action<float> Update = delta => {};
		public Action<float> LateUpdate = delta => {};

		public void TriggerUpdate(float delta) 
		{
			Update(delta);
		}

		public void TriggerLateUpdate(float delta)
		{
			LateUpdate(delta);
		}

		public void Wait(Action done, float seconds)
		{
			if (done == null) throw new ArgumentNullException("done");
			if (seconds < 0f) throw new ArgumentOutOfRangeException("seconds", "Cannot be less than zero.");
			var endtime = DateTime.Now.AddSeconds(seconds);
			Wait(done, () => endtime < DateTime.Now);
		}

		public void Wait(Action done, Func<bool> condition)
		{
			if (done == null) throw new ArgumentNullException("done");
			if (condition == null) throw new ArgumentNullException("condition");

			Wait(status => done(), condition);
		}

		/// <summary>
		/// Waits for the specified condition to be true and returns a cancel action.
		/// </summary>
		/// <param name="done">Done.</param>
		/// <param name="condition">Condition.</param>
		public Action Wait(Action<RequestStatus> done, Func<bool> condition)
		{
			if (done == null) throw new ArgumentNullException("done");
			if (condition == null) throw new ArgumentNullException("condition");

			var status = RequestStatus.Unknown;
			Action onCancel = () => status = RequestStatus.Cancel;

			Action<float> waiter = null;
			waiter = delta =>
			{
				try
				{
					if (status == RequestStatus.Unknown)
					{
						if (condition()) status = RequestStatus.Success;
						else return;
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					return;
				}

				Update -= waiter; // This may give you a warning, it's safe to ignore since this lambda is local

				try
				{
					done(status);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			};

			Update += waiter;

			return onCancel;
		}
	}
}

