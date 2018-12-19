using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

		[SerializeField]
		Transform velocityOptionsAnchor;
		[SerializeField]
		float velocityOptionsRadius;
		[SerializeField]
		float velocityOptionsBeginAngleOffset;
		[SerializeField]
		float velocityOptionsSpacing;

		[SerializeField]
		Transform velocityOptionsRoot;
		[SerializeField]
		GridVelocityOptionLeaf velocityOptionPrefab;
		[Header("Option Toggle Enabled")]
		[SerializeField]
		XButtonStyleBlock optionToggleEnabledStyle;
		[Header("Option Toggle Disabled")]
		[SerializeField]
		XButtonStyleBlock optionToggleDisabledStyle;

		[Header("Test")]
		[SerializeField]
		int previewCount;
		[SerializeField]
		float previewRadius;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		GridVelocityOptionLeaf[] options;

		public void SetVelocities(TransitVelocity velocities, int multiplier)
		{
			ClearVelocities();
			var velocityCount = velocities.MultiplierVelocities.Length;
			options = new GridVelocityOptionLeaf[velocityCount];

			for (var i = 0; i < velocityCount; i++)
			{
				var velocityIndex = (velocityCount - 1) - i;
				var position = Vector3.zero;
				var normal = Vector3.zero;
				GetOrientation(velocityIndex, out position, out normal);

				var instance = velocityOptionsRoot.gameObject.InstantiateChild(velocityOptionPrefab, setActive: true);

				instance.transform.position = position;
				instance.transform.forward = normal;

				SetOptionIndex(instance, velocityIndex, multiplier);

				options[velocityIndex] = instance;
			}
		}

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

		void ClearVelocities()
		{
			velocityLabel.text = string.Empty;
			velocityOptionsRoot.ClearChildren<GridVelocityOptionLeaf>();
			options = null;
		}

		void GetOrientation(int index, out Vector3 position, out Vector3 normal)
		{
			var beginOffset = Mathf.Deg2Rad * (90f - velocityOptionsBeginAngleOffset);
			var totalAngle = beginOffset - (index * velocityOptionsSpacing * Mathf.Deg2Rad);

			var result = new Vector3(Mathf.Cos(totalAngle), Mathf.Sin(totalAngle), 0f);
			result = result.NewX(result.x * -1f);

			result = velocityOptionsAnchor.rotation * result;

			normal = result;
			position = velocityOptionsAnchor.position + (result * velocityOptionsRadius);
		}

		void SetOptionIndices(int index)
		{
			for (var i = 0; i < options.Length; i++)
			{
				SetOptionIndex(options[i], i, index);
			}
		}

		void SetOptionIndex(GridVelocityOptionLeaf leaf, int currentIndex, int targetIndex)
		{
			var toggleEnabled = currentIndex <= targetIndex;
			Debug.Log("Toggle " + currentIndex + " is " + (toggleEnabled ? "enabled" : "disabled"));
			leaf.Toggle.LocalStyle = toggleEnabled ? optionToggleEnabledStyle : optionToggleDisabledStyle;
			leaf.Toggle.gameObject.SetActive(false);
			leaf.Toggle.gameObject.SetActive(true);
		}

		public override void Reset()
		{
			base.Reset();

			velocityOptionPrefab.gameObject.SetActive(false);

			ClearVelocities();
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

		void OnDrawGizmos()
		{
#if UNITY_EDITOR
			Handles.color = Color.green;
			Handles.DrawWireDisc(velocityOptionsAnchor.position, velocityOptionsAnchor.forward, velocityOptionsRadius);

			for (var i = 0; i < previewCount; i++)
			{
				var position = Vector3.zero;
				var normal = Vector3.zero;
				GetOrientation(i, out position, out normal);
				Handles.DrawWireDisc(position, normal, previewRadius);
				if (i == 0) Handles.DrawWireDisc(position, normal, previewRadius * 0.5f);
			}
#endif
		}
	}

	public interface IGridVelocityView : IView
	{
		void SetVelocities(TransitVelocity velocities, int multiplier);
		int Multiplier { set; }
		string VelocityUnit { set; }
		string ResourceUnit { set; }
	}
}