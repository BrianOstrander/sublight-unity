using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

using TMPro;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Views
{
	public struct BustBlock
	{
		public string BustId;

		public string TitleSource;
		public string TitleClassification;

		public string TransmitionType;
		public string TransmitionStrength;
		public int TransmitionStrengthIndex;

		public string PlacardName;
		public string PlacardDescription;

		public int? AvatarStaticIndex;
	}

	public class BustView : View, IBustView
	{
		class BustEntry
		{
			public enum States
			{
				Unknown = 0,
				Hidden = 10,
				Hiding = 20,
				Shown = 30,
				Showing = 40
			}

			public BustBlock Block;
			public BustLeaf Instance;

			public States State = States.Hidden;
			public float Progress;
			public bool? IsEnabled;
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Transform lookAtArea;
		[SerializeField]
		GameObject bustArea;
		[SerializeField]
		BustLeaf bustPrefab;
		[SerializeField, FormerlySerializedAs("AvatarsStatic")]
		Texture2D[] avatarsStatic;
		[SerializeField]
		float hideShowDuration;

		[Header("Bust Animations ( 0 is hidden 1 is shown )")]
		[SerializeField]
		CurveRange avatarDepth = CurveRange.Normal;
		[SerializeField]
		AnimationCurve avatarOpacity;
		[SerializeField]
		AnimationCurve labelOpacity;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		List<BustEntry> entries = new List<BustEntry>();

		public override void Reset()
		{
			base.Reset();

			bustArea.transform.ClearChildren<BustLeaf>();
			entries.Clear();

			bustPrefab.gameObject.SetActive(false);
		}

		public void InitializeBusts(params BustBlock[] busts)
		{
			foreach (var bust in busts)
			{
				var entry = new BustEntry();
				entry.Block = bust;
				entry.Instance = bustArea.InstantiateChild(bustPrefab);

				entries.Add(entry);

				ApplyBust(entry);
			}
		}

		public void FocusBust(string bustId, bool instant = false)
		{
			foreach (var bust in entries)
			{
				var isTarget = bust.Block.BustId == bustId;
				if (isTarget)
				{
					if (bust.State != BustEntry.States.Shown) bust.State = BustEntry.States.Showing;
				}
				else
				{
					if (bust.State != BustEntry.States.Hidden) bust.State = BustEntry.States.Hiding;
				}
			}
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			lookAtArea.LookAt(lookAtArea.position + (lookAtArea.position - App.V.CameraPosition).FlattenY());
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			foreach (var entry in entries) UpdateBust(entry, delta);
		}

		void UpdateBust(BustEntry bust, float delta)
		{
			switch (bust.State)
			{
				case BustEntry.States.Hidden:
				case BustEntry.States.Shown:
					return;
				case BustEntry.States.Hiding:
					bust.Progress = Mathf.Max(0f, bust.Progress - delta);
					if (Mathf.Approximately(0f, bust.Progress)) bust.State = BustEntry.States.Hidden;
					break;
				case BustEntry.States.Showing:
					bust.Progress = Mathf.Min(hideShowDuration, bust.Progress + delta);
					if (Mathf.Approximately(hideShowDuration, bust.Progress)) bust.State = BustEntry.States.Shown;
					break;
			}

			ApplyBust(bust);
		}

		void ApplyBust(BustEntry bust)
		{
			var instance = bust.Instance;
			var scalar = 0f;
			var desiredEnabledState = true;

			switch (bust.State)
			{
				case BustEntry.States.Hidden:
					scalar = 0f;
					desiredEnabledState = false;
					break;
				case BustEntry.States.Shown:
					scalar = 1f;
					desiredEnabledState = true;
					break;
				case BustEntry.States.Hiding:
				case BustEntry.States.Showing:
					scalar = bust.Progress / hideShowDuration;
					desiredEnabledState = true;
					break;
			}

			var currLabelsOpacity = labelOpacity.Evaluate(scalar);

			instance.TitleGroup.alpha = currLabelsOpacity;
			instance.PlacardGroup.alpha = currLabelsOpacity;
			instance.AvatarGroup.alpha = avatarOpacity.Evaluate(scalar);

			instance.AvatarAnchor.localPosition = instance.AvatarDepthAnchor.localPosition * avatarDepth.Evaluate(scalar);

			if (!bust.IsEnabled.HasValue || bust.IsEnabled.Value != desiredEnabledState)
			{
				bust.IsEnabled = desiredEnabledState;

				instance.gameObject.SetActive(desiredEnabledState);
			}
		}

		void ApplyBustBlock(BustEntry bust, BustBlock block)
		{
			bust.Block = block;
			var instance = bust.Instance;

			instance.TitleLabel.text = "<b>" + block.TitleSource + "</b> " + block.TitleClassification;
			instance.TransmissionLabel.text = "<b>" + block.TransmitionType + ":</b> " + block.TransmitionStrength;

			instance.PlacardName.text = block.PlacardName;
			instance.PlacardDescription.text = block.PlacardDescription;

			if (block.AvatarStaticIndex.HasValue)
			{
				instance.AvatarStaticImage.material.SetTexture(
					ShaderConstants.HoloBustAvatarStatic.ColorMap,
					avatarsStatic[Mathf.Clamp(block.AvatarStaticIndex.Value, 0, avatarsStatic.Length - 1)]
				);
			}

			for (var i = 0; i < instance.TransmissionStrengths.Length; i++) instance.TransmissionStrengths[i].SetActive(i == block.TransmitionStrengthIndex);
		}

		#region Events

		#endregion

		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(bustPrefab.AvatarAnchor.position, bustPrefab.AvatarDepthAnchor.position);
		}
	}

	public interface IBustView : IView
	{
		void InitializeBusts(params BustBlock[] busts);
		void FocusBust(string bustId, bool instant = false);
	}
}