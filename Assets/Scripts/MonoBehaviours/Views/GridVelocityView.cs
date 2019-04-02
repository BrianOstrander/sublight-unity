/*
using System;
using System.Linq;
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
		CanvasGroup group;
		[SerializeField]
		CanvasGroup labelGroup;
		[SerializeField]
		TextMeshProUGUI velocityLabel;
		[SerializeField]
		TextMeshProUGUI velocityUnitLabel;
		[SerializeField]
		TextMeshProUGUI multiplierLabel;
		[SerializeField]
		TextMeshProUGUI multiplierResourceLabel;

		[SerializeField]
		Color velocityColorCurrentNormal;
		[SerializeField]
		Color velocityColorCurrentDisabled;

		[SerializeField]
		CanvasGroup resourceWarningGroup;
		[SerializeField]
		float resourceWarningRevealDuration;
		[SerializeField]
		TextMeshProUGUI resourceWarningLabel;
		[SerializeField]
		ParticleSystem resourceWarningParticles;
		[SerializeField]
		Vector3 resourceWarningRotation;

		[SerializeField]
		CanvasGroup velocityOptionsGroup;
		[SerializeField]
		AnimationCurve velocityOptionsTransitionOpacity;
		[SerializeField]
		Transform velocityOptionsAnchor;
		[SerializeField]
		float velocityOptionsRadius;
		[SerializeField]
		float velocityOptionsBeginAngleOffset;
		[SerializeField]
		float velocityOptionsSpacing;

		[SerializeField]
		float velocityDisabledLabelOpacity;
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

		VelocityProfileState? lastVelocity;
		GridVelocityOptionLeaf[] options;
		long frameOptionEntered;
		long frameOptionExited;

		bool showingResourceWarning;

		public void SetVelocities(VelocityProfileState velocity)
		{
			if (lastVelocity.HasValue && lastVelocity.Value.Profile.Approximately(velocity.Profile) && lastVelocity.Value.PropellantUsageLimit == velocity.PropellantUsageLimit)
			{
				if (lastVelocity.Value.PropellantUsage != velocity.PropellantUsage) SetOptionIndices(velocity.PropellantUsage);
				
				lastVelocity = velocity;
				return;
			}

			lastVelocity = velocity;

			ClearVelocities();
			var velocityCount = velocity.Profile.Count;
			options = new GridVelocityOptionLeaf[velocityCount];

			for (var i = 0; i < velocityCount; i++)
			{
				var currentIndex = i;
				var propellantUsageEntry = i + 1;
				var isEnabled = propellantUsageEntry <= velocity.PropellantUsageLimit;
				var orientationIndex = (velocityCount - 1) - currentIndex;
				var position = Vector3.zero;
				var normal = Vector3.zero;
				GetOrientation(orientationIndex, out position, out normal);

				var instance = velocityOptionsRoot.gameObject.InstantiateChild(velocityOptionPrefab, setActive: true);

				instance.transform.position = position;
				instance.transform.forward = -normal;

				instance.Button.OnEnter.AddListener(() => OnEnterOption(instance, propellantUsageEntry));
				instance.Button.OnExit.AddListener(() => OnExitOption(instance));

				if (isEnabled) instance.Button.OnClick.AddListener(() => OnClickEnabledOption(instance, propellantUsageEntry));
				else instance.Button.OnClick.AddListener(() => OnClickDisabledOption(instance, propellantUsageEntry));

				instance.EnabledArea.SetActive(isEnabled);
				instance.DisabledArea.SetActive(!isEnabled);

				options[currentIndex] = instance;
			}

			velocityMesh.material.SetFloat(ShaderConstants.HoloWidgetGridVelocitySelection.AnchorMinimum, velocity.Profile.Minimum.RelativisticLightYears);
			velocityMesh.material.SetFloat(ShaderConstants.HoloWidgetGridVelocitySelection.AnchorMaximum, velocity.Profile.Maximum.RelativisticLightYears);

			SetOptionIndices(velocity.PropellantUsage);
		}

		public Action<int> MultiplierSelection { set; private get; }
		public string VelocityUnit { set { velocityUnitLabel.text = value ?? string.Empty; } }
		public string ResourceUnit { set { multiplierResourceLabel.text = value ?? string.Empty; } }
		public string ResourceWarning { set { resourceWarningLabel.text = value ?? string.Empty; } }

		float SizeTransitionScalar { get { return 1f / sizeTransitionDuration; } }

		SizeTransitions sizeTransition;
		float sizeTransitionProgress; // 0 is minimized and 1 is maximized
		int? multiplierBeforePreview;

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			var revealDelta = delta * (1f / resourceWarningRevealDuration);
			resourceWarningGroup.alpha = Mathf.Clamp01(resourceWarningGroup.alpha + (showingResourceWarning ? revealDelta : -revealDelta));

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
			velocityOptionsGroup.alpha = velocityOptionsTransitionOpacity.Evaluate(transition);
		}

		void ClearVelocities()
		{
			velocityLabel.text = string.Empty;
			multiplierLabel.text = string.Empty;
			velocityOptionsRoot.ClearChildren<GridVelocityOptionLeaf>();
			options = null;
			velocityMesh.material.SetFloat(ShaderConstants.HoloWidgetGridVelocitySelection.AnchorMinimum, 0f);
			velocityMesh.material.SetFloat(ShaderConstants.HoloWidgetGridVelocitySelection.AnchorCurrent, 0f);
			velocityMesh.material.SetFloat(ShaderConstants.HoloWidgetGridVelocitySelection.AnchorMaximum, 0f);
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

		void SetOptionIndices(int propellantUsage)
		{
			multiplierLabel.text = propellantUsage.ToString("N0");
			velocityLabel.text = lastVelocity.Value.GetVelocityByUsage(propellantUsage).RelativisticLightYears.ToString("0.00");

			var isEnabled = propellantUsage <= lastVelocity.Value.PropellantUsageLimit;
			showingResourceWarning = !isEnabled;
			labelGroup.alpha = isEnabled ? 1f : velocityDisabledLabelOpacity;

			var currColor = isEnabled ? velocityColorCurrentNormal : velocityColorCurrentDisabled;
			velocityMesh.material.SetColor(ShaderConstants.HoloWidgetGridVelocitySelection.MinimumColor, currColor);
			velocityMesh.material.SetColor(ShaderConstants.HoloWidgetGridVelocitySelection.CurrentColor, currColor);

			for (var i = 0; i < options.Length; i++)
			{
				SetOptionIndex(options[i], i + 1, propellantUsage);
			}

			velocityMesh.material.SetFloat(ShaderConstants.HoloWidgetGridVelocitySelection.AnchorCurrent, lastVelocity.Value.GetVelocityByUsage(propellantUsage).Normal);
		}

		void SetOptionIndex(GridVelocityOptionLeaf leaf, int propellantUsageEntry, int propellantUsage)
		{
			var toggleEnabled = propellantUsageEntry <= propellantUsage;
			//Debug.Log("Toggle " + currentIndex + " is " + (toggleEnabled ? "enabled" : "disabled"));
			leaf.Toggle.LocalStyle = toggleEnabled ? optionToggleEnabledStyle : optionToggleDisabledStyle;
			leaf.Toggle.gameObject.SetActive(false);
			leaf.Toggle.gameObject.SetActive(true);
			leaf.Button.ForceApplyState();
		}

		protected override void OnOpacityStack(float opacity)
		{
			group.alpha = opacity;
			velocityMesh.material.SetFloat(ShaderConstants.HoloWidgetGridVelocitySelection.Alpha, opacity);
			group.interactable = Mathf.Approximately(opacity, 1f);
		}

		public override void Reset()
		{
			base.Reset();

			velocityOptionPrefab.gameObject.SetActive(false);

			ClearVelocities();

			lastVelocity = null;

			MultiplierSelection = ActionExtensions.GetEmpty<int>();
			VelocityUnit = string.Empty;
			ResourceUnit = string.Empty;
			ResourceWarning = string.Empty;
			resourceWarningGroup.alpha = 0f;
			showingResourceWarning = false;
			resourceWarningGroup.transform.localRotation = Quaternion.Euler(resourceWarningRotation);

			SetTransition(0f);
			sizeTransition = SizeTransitions.Minimized;
			sizeTransitionProgress = 0f;

			multiplierBeforePreview = null;
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


		void OnEnterOption(GridVelocityOptionLeaf leaf, int propellentUsageEntry)
		{
			if (!multiplierBeforePreview.HasValue) multiplierBeforePreview = lastVelocity.Value.PropellantUsage;

			frameOptionEntered = App.V.FrameCount;

			SetOptionIndices(propellentUsageEntry);

			leaf.EnterParticles.Emit(1);

			if (MultiplierSelection != null) MultiplierSelection(propellentUsageEntry);
		}

		void OnExitOption(GridVelocityOptionLeaf leaf)
		{
			frameOptionExited = App.V.FrameCount;

			if (frameOptionExited == frameOptionEntered) return;

			var result = multiplierBeforePreview ?? lastVelocity.Value.PropellantUsage;

			multiplierBeforePreview = null;

			SetOptionIndices(result);

			if (MultiplierSelection != null) MultiplierSelection(result);
		}

		void OnClickEnabledOption(GridVelocityOptionLeaf leaf, int propellantUsageEntry)
		{
			multiplierBeforePreview = propellantUsageEntry;
			if (MultiplierSelection != null) MultiplierSelection(propellantUsageEntry);
		}

		void OnClickDisabledOption(GridVelocityOptionLeaf leaf, int propellantUsageEntry)
		{
			multiplierBeforePreview = lastVelocity.Value.PropellantUsageLimit;
			//options[lastVelocity.Value.MultiplierEnabledMaximum].EnterParticles.Emit(1);

			leaf.DisabledParticles.Emit(1);
			resourceWarningParticles.Emit(1);
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
		Action<int> MultiplierSelection { set; }
		string VelocityUnit { set; }
		string ResourceUnit { set; }
		string ResourceWarning { set; }
		void SetVelocities(VelocityProfileState velocities);
	}
}
*/