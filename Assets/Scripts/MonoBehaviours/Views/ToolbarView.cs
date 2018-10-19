using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SubLight.Views
{
	public class ToolbarView : View, IToolbarView
	{
		[Serializable]
		struct IconEntry
		{
			public SetFocusLayers Layer;
			public Sprite Icon;
		}
		
		class ButtonEntry
		{
			public int Index;
			public bool IsSelected;
			public ToolbarButtonLeaf Leaf;
			public ToolbarButtonBlock Block;
			public bool IsHighlighted;
			// 0 is not highlighted, 1 is highlighted;
			public float HighlightElapsed;
		}

		struct ToolbarPosition
		{
			public Vector3 Position;
			public Vector3 Forward;
		}

		[SerializeField]
		Transform orbitOrigin;
		[SerializeField]
		float orbitRadius;
		[SerializeField]
		float buttonSeparation;
		[SerializeField]
		GameObject buttonArea;
		[SerializeField]
		ToolbarButtonLeaf buttonPrefab;

		[SerializeField]
		AnimationCurve buttonHighlightScale;
		[SerializeField]
		AnimationCurve buttonHighlightOpacity;
		[SerializeField]
		float buttonHighlightMinimumScale;
		[SerializeField]
		float buttonHighlightDuration;

		[SerializeField]
		IconEntry[] iconEntries;

		[Header("Test")]
		[SerializeField]
		int previewCount;
		[SerializeField]
		float previewButtonDiameter;

		List<ButtonEntry> currentButtons = new List<ButtonEntry>();

		Vector3 RangeCenter { get { return orbitOrigin.position + (orbitOrigin.forward * orbitRadius); } }

		ToolbarPosition[] GetPositions(int max)
		{
			max = Mathf.Max(1, max);

			var results = new ToolbarPosition[max];
			var center = RangeCenter;
			var totalRange = buttonSeparation * (max - 1f);
			var rangeOffset = totalRange * 0.5f;

			for (var i = 0; i < max; i++)
			{
				var finalAngle = Mathf.Deg2Rad * (90f + ((buttonSeparation * i) - rangeOffset));
				var forward = orbitOrigin.rotation * new Vector3(Mathf.Cos(finalAngle), 0f, Mathf.Sin(finalAngle));

				results[i] = new ToolbarPosition
				{
					Position = orbitOrigin.position + (forward * orbitRadius),
					Forward = forward
				};

			}

			return results;
		}

		int selection;

		public int Selection
		{
			get { return selection; }
			set
			{
				selection = value;
				if (currentButtons == null) return;
				foreach (var button in currentButtons) button.IsSelected = button.Index == value;
			}
		}

		public Sprite GetIcon(SetFocusLayers layer) { return iconEntries.FirstOrDefault(e => e.Layer == layer).Icon; }

		public ToolbarButtonBlock[] Buttons
		{
			set
			{
				buttonArea.transform.ClearChildren<ToolbarButtonLeaf>();
				currentButtons.Clear();

				if (value == null) return;

				var positions = GetPositions(value.Length);

				for (var i = 0; i < positions.Length; i++)
				{
					var position = positions[i];
					var block = value[i];
					var button = buttonArea.InstantiateChild(buttonPrefab, setActive: true);

					var buttonEntry = new ButtonEntry
					{
						Index = i,
						IsSelected = i == Selection,
						Leaf = button,
						Block = block,
						HighlightElapsed = 0f
					};

					button.transform.position = position.Position;
					button.transform.forward = position.Forward;

					button.ButtonImage.sprite = block.Icon;

					button.ButtonLabelArea.SetActive(!string.IsNullOrEmpty(block.Text));
					button.ButtonLabel.text = block.Text ?? string.Empty;

					button.Button.OnEnter.AddListener(() => OnButtonEnter(buttonEntry));
					button.Button.OnExit.AddListener(() => OnButtonExit(buttonEntry));

					UpdateButton(buttonEntry, 1f);

					currentButtons.Add(buttonEntry);
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			Buttons = null;
			Selection = -1;

			buttonPrefab.gameObject.SetActive(false);
		}

		protected override void OnIdle(float delta)
		{
			base.OnIdle(delta);

			foreach (var button in currentButtons)
			{
				if (button.IsHighlighted && Mathf.Approximately(button.HighlightElapsed, buttonHighlightDuration)) continue;
				if (!button.IsHighlighted && Mathf.Approximately(button.HighlightElapsed, 0f)) continue;

				button.HighlightElapsed = Mathf.Clamp(button.HighlightElapsed + (button.IsHighlighted ? delta : -delta), 0f, buttonHighlightDuration);
				var scalar = button.HighlightElapsed / buttonHighlightDuration;

				UpdateButton(button, scalar);
			}
		}

		void UpdateButton(ButtonEntry button, float scalar)
		{
			var scale = buttonHighlightMinimumScale + (buttonHighlightScale.Evaluate(scalar) * (1f - buttonHighlightMinimumScale));
			var highlightedOpacity = buttonHighlightOpacity.Evaluate(scalar);
			var unHighlightedOpacity = 1f - highlightedOpacity;

			button.Leaf.ScalableArea.localScale = Vector3.one * scale;

			button.Leaf.UnHighlightedSelectedArea.alpha = button.IsSelected ? unHighlightedOpacity : 0f;
			button.Leaf.UnHighlightedUnSelectedArea.alpha = button.IsSelected ? 0f : unHighlightedOpacity;

			button.Leaf.HighlightedArea.alpha = highlightedOpacity;
		}

		#region Events
		void OnButtonEnter(ButtonEntry entry)
		{
			entry.IsHighlighted = true;
		}

		void OnButtonExit(ButtonEntry entry)
		{
			entry.IsHighlighted = false;
		}
		#endregion

		void OnDrawGizmos()
		{
#if UNITY_EDITOR
			Gizmos.color = Color.yellow.NewA(0.2f);
			Handles.color = Color.yellow.NewA(0.2f);
			Gizmos.DrawLine(orbitOrigin.position, RangeCenter);
			Handles.DrawWireDisc(orbitOrigin.position, orbitOrigin.up, orbitRadius);

			Gizmos.color = Color.green;
			var count = 0f;
			foreach (var position in GetPositions(previewCount))
			{
				Handles.color = Color.green;
				if (count == 0)
				{
					Gizmos.DrawLine(orbitOrigin.position, position.Position);
					Handles.DrawWireDisc(position.Position, position.Forward, previewButtonDiameter * 0.05f);
				}
				Handles.DrawWireDisc(position.Position, position.Forward, previewButtonDiameter * 0.5f);

				Handles.color = Color.red;
				Handles.DrawWireDisc(position.Position, position.Forward, (previewButtonDiameter * buttonHighlightMinimumScale) * 0.5f);
				count++;
			}
#endif
		}
	}

	public interface IToolbarView : IView
	{
		int Selection { set; }
		Sprite GetIcon(SetFocusLayers layer);
		ToolbarButtonBlock[] Buttons { set; }
	}
}