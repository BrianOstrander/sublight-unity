using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class GridVelocityView : View, IGridVelocityView
	{
		enum SizeTransitions
		{
			Unknown = 0,
			Maximizing = 10,
			Maximized = 20,
			WaitingToMinimize = 30,
			Minimizing = 40,
			Minimized = 50
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float exitDelayDuration;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		SizeTransitions sizeTransition;
		float? exitDelayRemaining;

		public override void Reset()
		{
			base.Reset();
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);


			//exitDelayRemaining = Mathf.Max(0f, exitDelayRemaining.Value - delta);

			//if (Mathf.Approximately(0f, exitDelayRemaining.Value)) exitDelayRemaining = null;

		}

		#region Events
		public void OnEnter()
		{
			Debug.Log("entered");
			//switch (sizeTransition)
			//{
			//	case SizeTransitions.Maximizing:
			//	case SizeTransitions.Maximized:
			//		// These could happen if you hover over a button while it's maximized or whatever.
			//		break;
			//	case SizeTransitions.WaitingToMinimize:
			//		sizeTransition = SizeTransitions.Maximized;
			//		break;
			//	case SizeTransitions.Minimizing:
			//	case SizeTransitions.Minimized:
			//		sizeTransition = SizeTransitions.Maximizing;
			//		break;
			//	default:
			//		Debug.LogError("Unrecognized transition: " + sizeTransition);
			//		break;
			//}
		}

		public void OnExit()
		{
			Debug.Log("exited");
			//switch (sizeTransition)
			//{
			//	case SizeTransitions.Maximizing:
			//	case SizeTransitions.Maximized:
			//		sizeTransition = SizeTransitions.WaitingToMinimize;
			//		break;
			//	case SizeTransitions.WaitingToMinimize:
			//	case SizeTransitions.Minimizing:
			//	case SizeTransitions.Minimized:
			//		break;
			//	default:
			//		Debug.LogError("Unrecognized transition: " + sizeTransition);
			//		break;
			//}
		}

		public void OnClick()
		{
			Debug.Log("clicked area");
		}

		public void OnClickButton()
		{
			Debug.Log("clicked button");
		}
		#endregion
	}

	public interface IGridVelocityView : IView
	{

	}
}