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
			Minimizing = 30,
			Minimized = 40
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float sizeTransitionDuration;
		[SerializeField]
		MeshRenderer velocityMesh;
		[SerializeField]
		TextMeshProUGUI velocityLabel;
		[SerializeField]
		TextMeshProUGUI velocityUnitLabel;
		[SerializeField]
		TextMeshProUGUI multiplierLabel;
		[SerializeField]
		TextMeshProUGUI multiplierResourceLabel;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public float Velocity { set { velocityLabel.text = value.ToString("0.00"); } }

		public int Multiplier
		{
			set
			{
				multiplierLabel.text = value.ToString("N0");
				// TODO: Other logic....
			}
		}

		public string VelocityUnit { set { velocityUnitLabel.text = value ?? string.Empty; } }

		public string ResourceUnit { set { multiplierResourceLabel.text = value ?? string.Empty; } }

		float SizeTransitionScalar { get { return 1f / sizeTransitionDuration; } }

		SizeTransitions sizeTransition;
		float sizeTransitionProgress; // 0 is minimized and 1 is maximized

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			var transitionDelta = 0f;

			switch (sizeTransition)
			{
				case SizeTransitions.Maximized:
				case SizeTransitions.Minimized:
					return;
				case SizeTransitions.Maximizing:
					transitionDelta = delta * SizeTransitionScalar;
					break;
				case SizeTransitions.Minimizing:
					transitionDelta = -(delta * SizeTransitionScalar);
					break;
				default:
					Debug.LogError("Unrecognized transition: " + sizeTransition);
					break;
			}

			SetTransition(sizeTransitionProgress = Mathf.Clamp01(sizeTransitionProgress + transitionDelta));

			if (Mathf.Approximately(sizeTransition == SizeTransitions.Maximizing ? 1f : 0f, sizeTransitionProgress))
			{
				switch (sizeTransition)
				{
					case SizeTransitions.Maximizing:
						sizeTransition = SizeTransitions.Maximized;
						break;
					case SizeTransitions.Minimizing:
						sizeTransition = SizeTransitions.Minimized;
						break;
				}
			}
		}

		void SetTransition(float transition)
		{
			velocityMesh.material.SetFloat(ShaderConstants.HoloWidgetGridVelocitySelection.Maximized, 1f - transition);
		}

		public override void Reset()
		{
			base.Reset();

			Velocity = 0f;
			Multiplier = 0;
			VelocityUnit = string.Empty;
			ResourceUnit = string.Empty;

			SetTransition(0f);
			sizeTransition = SizeTransitions.Minimized;
			sizeTransitionProgress = 0f;
		}

		#region Events
		public void OnEnterTransitionArea()
		{
			switch (sizeTransition)
			{
				case SizeTransitions.Maximizing:
				case SizeTransitions.Maximized:
					break;
				case SizeTransitions.Minimizing:
				case SizeTransitions.Minimized:
					sizeTransition = SizeTransitions.Maximizing;
					break;
				default:
					Debug.LogError("Unrecognized transition: " + sizeTransition);
					break;
			}
		}

		public void OnExitTransitionArea()
		{
			switch (sizeTransition)
			{
				case SizeTransitions.Maximizing:
				case SizeTransitions.Maximized:
					sizeTransition = SizeTransitions.Minimizing;
					break;
				case SizeTransitions.Minimizing:
				case SizeTransitions.Minimized:
					break;
				default:
					Debug.LogError("Unrecognized transition: " + sizeTransition);
					break;
			}
		}
		#endregion
	}

	public interface IGridVelocityView : IView
	{
		float Velocity { set; }
		int Multiplier { set; }
		string VelocityUnit { set; }
		string ResourceUnit { set; }
	}
}