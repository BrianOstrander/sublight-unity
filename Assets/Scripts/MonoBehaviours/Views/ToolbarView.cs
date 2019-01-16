using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

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
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public SetFocusLayers Layer;
			public Sprite Icon;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}

		class ButtonEntry
		{
			public int Index;
			public bool IsSelected;
			public ToolbarButtonLeaf Leaf;
			public ToolbarButtonBlock Block;
			public bool IsHighlighted;
			// 0 is not highlighted, max duration is highlighted;
			public float HighlightElapsed;
		}

		struct ToolbarPosition
		{
			public Vector3 Position;
			public Vector3 Forward;
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
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
		XButtonStyleObject selectedHaloStyle;
		[SerializeField]
		XButtonStyleObject selectedBackgroundStyle;
		[SerializeField]
		XButtonStyleObject unSelectedHaloStyle;
		[SerializeField]
		XButtonStyleObject unSelectedBackgroundStyle;

		[SerializeField]
		IconEntry[] iconEntries;

		[Header("Test")]
		[SerializeField]
		int previewCount;
		[SerializeField]
		float previewButtonDiameter;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

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
				var finalAngle = Mathf.Deg2Rad * (((buttonSeparation * i) - rangeOffset) - 90f);
				var forward = orbitOrigin.rotation * new Vector3(Mathf.Cos(finalAngle), 0f, Mathf.Sin(finalAngle));

				results[i] = new ToolbarPosition
				{
					Position = orbitOrigin.position + (forward * orbitRadius),
					Forward = -forward
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
				foreach (var button in currentButtons)
				{
					var wasSelected = button.IsSelected;
					var isSelected = button.Index == value;
					button.IsSelected = isSelected;

					if (wasSelected && !isSelected)
					{
						// No longer selected.
						UpdateButton(button, 0f);
						button.Leaf.HaloLeaf.GlobalStyle = unSelectedHaloStyle;
						button.Leaf.BackgroundLeaf.GlobalStyle = unSelectedBackgroundStyle;
					}
					else if (isSelected && !wasSelected)
					{
						// Now selected.
						UpdateButton(button, 0f);
						button.Leaf.HaloLeaf.GlobalStyle = selectedHaloStyle;
						button.Leaf.BackgroundLeaf.GlobalStyle = selectedBackgroundStyle;
					}
				}
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

					button.HaloLeaf.GlobalStyle = buttonEntry.IsSelected ? selectedHaloStyle : unSelectedHaloStyle;
					button.BackgroundLeaf.GlobalStyle = buttonEntry.IsSelected ? selectedBackgroundStyle : unSelectedBackgroundStyle;

					button.transform.position = position.Position;
					button.transform.forward = position.Forward;

					button.ButtonImage.sprite = block.Icon;

					button.ButtonLabelArea.SetActive(!string.IsNullOrEmpty(block.Text));
					button.ButtonLabel.text = block.Text ?? string.Empty;

					button.Button.OnEnter.AddListener(() => OnButtonEnter(buttonEntry));
					button.Button.OnExit.AddListener(() => OnButtonExit(buttonEntry));
					button.Button.OnClick.AddListener(() => OnButtonClick(buttonEntry));

					UpdateButton(buttonEntry, 0f);

					currentButtons.Add(buttonEntry);
				}
			}
		}

		protected override void OnOpacityStack(float opacity)
		{
			if (currentButtons == null) return;
			var interactionsEnabled = !Mathf.Approximately(0f, opacity);
			foreach (var button in currentButtons)
			{
				button.Leaf.OpacityArea.alpha = opacity;
				button.Leaf.OpacityArea.blocksRaycasts = interactionsEnabled;
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

		void OnButtonClick(ButtonEntry entry)
		{
			if (entry.Block.Click != null) entry.Block.Click();
		}
		#endregion

		void OnDrawGizmosSelected()
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