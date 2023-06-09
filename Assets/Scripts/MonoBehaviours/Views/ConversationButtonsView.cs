﻿using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace LunraGames.SubLight.Views
{
	public struct ConversationButtonBlock
	{
		public string Message;
		public bool Used;
		public bool Interactable;
		public Action Click;
	}

	public struct ConversationButtonThemeBlock
	{
		public struct State
		{
			public XButtonSoundBlock Sounds;

			public XButtonStyleBlock LabelStyle;
			public XButtonStyleBlock BulletStyle;
			public XButtonStyleBlock UnderlineStyle;
			public XButtonStyleBlock BackgroundStyle;
		}

		public State Normal;
		public State Used;
		public State NotInteractable;
		public State Selected;
	}

	public class ConversationButtonsView : View, IConversationButtonsView
	{
		class ButtonEntry
		{
			public ConversationButtonBlock Block;
			public ConversationButtonLeaf Instance;

			public float IndexNormal;
			public bool IsSelection;
		}

		[Serializable]
		struct StyleBlock
		{
			[Serializable]
			public struct State
			{
				public XButtonSoundObject Sounds;

				public XButtonStyleObject LabelStyle;
				public XButtonStyleObject BulletStyle;
				public XButtonStyleObject UnderlineStyle;
				public XButtonStyleObject BackgroundStyle;
			}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			[SerializeField]
			State normal;
			[Header("Leave fields in the following states blank to default to the normal style")]
			[SerializeField]
			State used;
			[SerializeField]
			State notInteractable;
			[SerializeField]
			State selected;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

			State GetNormalized(State target)
			{
				return new State
				{
					// You'll be tempted to use ?? here, don't do that, Unity messes with null Objects.
					Sounds = target.Sounds == null ? normal.Sounds : target.Sounds,

					LabelStyle = target.LabelStyle == null ? normal.LabelStyle : target.LabelStyle,
					BulletStyle = target.BulletStyle == null ? normal.BulletStyle : target.BulletStyle,
					UnderlineStyle = target.UnderlineStyle == null ? normal.UnderlineStyle : target.UnderlineStyle,
					BackgroundStyle = target.BackgroundStyle == null ? normal.BackgroundStyle : target.BackgroundStyle
				};
			}

			public State Normal { get { return normal; } }
			public State Used { get { return GetNormalized(used); } }
			public State NotInteractable { get { return GetNormalized(notInteractable); } }
			public State Selected { get { return GetNormalized(selected); } }
		}

		[Serializable]
		struct ThemeModifier
		{
			public Color Hue;
			public HsvOperator Multiplier;
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		InterfaceScaleBlock buttonScales = InterfaceScaleBlock.Default;
		[SerializeField]
		Transform buttonsScaleArea;
		[SerializeField]
		Transform lookAtArea;
		[SerializeField]
		GameObject buttonArea;
		[SerializeField]
		ConversationButtonLeaf buttonPrefab;
		[SerializeField]
		GameObject spacerPrefab;
		[SerializeField]
		int minimumButtonsForSpacer;

		[Header("Button Animations")]
		[SerializeField]
		AnimationCurve buttonOpacityByOrder;
		[SerializeField]
		AnimationCurve buttonOpacityOnClose;
		[SerializeField]
		AnimationCurve buttonSelectedOpacityOnClose;

		[Header("Themes")]
		[SerializeField]
		StyleBlock defaultTheme;
		[SerializeField]
		ThemeModifier crewModifier;
		[SerializeField]
		ThemeModifier awayTeamModifier;
		[SerializeField]
		ThemeModifier foreignerModifier;
		[SerializeField]
		ThemeModifier downlinkModifier;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		Color GetModifiedStateThemeColor(Color baseColor, ThemeModifier modifier)
		{
			// Finally...
			return modifier.Multiplier.ApplyAddition(baseColor).NewH(modifier.Hue.GetH());
		}

		XButtonStyleBlock GetModifiedStateTheme(XButtonStyleObject baseObject, ThemeModifier modifier)
		{
			var result = baseObject.Block.Duplicate;
			var colors = result.Colors;
			colors.DisabledColor = GetModifiedStateThemeColor(colors.DisabledColor, modifier);
			colors.NormalColor = GetModifiedStateThemeColor(colors.NormalColor, modifier);
			colors.HighlightedColor = GetModifiedStateThemeColor(colors.HighlightedColor, modifier);
			colors.PressedColor = GetModifiedStateThemeColor(colors.PressedColor, modifier);
			result.Colors = colors;
			return result;
		}

		ConversationButtonThemeBlock.State GetModifiedStateTheme(StyleBlock.State baseState, ThemeModifier modifier)
		{
			return new ConversationButtonThemeBlock.State
			{
				Sounds = baseState.Sounds.Block,

				LabelStyle = GetModifiedStateTheme(baseState.LabelStyle, modifier),
				BulletStyle = GetModifiedStateTheme(baseState.BulletStyle, modifier),
				UnderlineStyle = GetModifiedStateTheme(baseState.UnderlineStyle, modifier),
				BackgroundStyle = GetModifiedStateTheme(baseState.BackgroundStyle, modifier)
			};
		}

		ConversationButtonThemeBlock GetModifiedTheme(StyleBlock baseTheme, ThemeModifier modifier)
		{
			return new ConversationButtonThemeBlock
			{
				Normal = GetModifiedStateTheme(baseTheme.Normal, modifier),
				Used = GetModifiedStateTheme(baseTheme.Used, modifier),
				NotInteractable = GetModifiedStateTheme(baseTheme.NotInteractable, modifier),
				Selected = GetModifiedStateTheme(baseTheme.Selected, modifier),
			};
		}

		public ConversationButtonThemeBlock DefaultTheme { get { return GetModifiedTheme(defaultTheme, new ThemeModifier { Hue = Color.white, Multiplier = new HsvOperator() }); } }
		public ConversationButtonThemeBlock CrewTheme { get { return GetModifiedTheme(defaultTheme, crewModifier); } }
		public ConversationButtonThemeBlock AwayTeamTheme { get { return GetModifiedTheme(defaultTheme, awayTeamModifier); } }
		public ConversationButtonThemeBlock ForeignerTheme { get { return GetModifiedTheme(defaultTheme, foreignerModifier); } }
		public ConversationButtonThemeBlock DownlinkTheme { get { return GetModifiedTheme(defaultTheme, downlinkModifier); } }

		public Action<Action> Click { set; private get; }

		List<ButtonEntry> entries = new List<ButtonEntry>();

		bool hasClicked;

		public void SetButtons(ConversationButtonThemeBlock theme, params ConversationButtonBlock[] blocks)
		{
			buttonArea.transform.ClearChildren();
			entries.Clear();

			var index = 0;
			var count = blocks.Length;
			foreach (var block in blocks)
			{
				var entry = new ButtonEntry();
				entry.Block = block;
				entry.Instance = buttonArea.InstantiateChild(buttonPrefab, setActive: true);
				entry.IndexNormal = index / (float)count;

				entry.Instance.Button.OnClick.AddListener(new UnityAction(() => OnClick(entry)));
				entry.Instance.MessageLabel.text = block.Message ?? string.Empty;
				entry.Instance.Group.alpha = 0f;

				var style = theme.Normal;

				if (!block.Interactable) style = theme.NotInteractable;
				else if (block.Used) style = theme.Used;

				entry.Instance.Button.LocalSounds = style.Sounds;

				entry.Instance.LabelArea.LocalStyle = style.LabelStyle;
				entry.Instance.BulletArea.LocalStyle = style.BulletStyle;
				entry.Instance.BulletDisabledArea.LocalStyle = style.BulletStyle;
				entry.Instance.UnderlineArea.LocalStyle = style.UnderlineStyle;
				entry.Instance.BackgroundArea.LocalStyle = style.BackgroundStyle;

				entry.Instance.BulletArea.gameObject.SetActive(block.Interactable);
				entry.Instance.BulletDisabledArea.gameObject.SetActive(!block.Interactable);

				entries.Add(entry);

				index++;
			}

			if (count <= minimumButtonsForSpacer) buttonArea.InstantiateChildObject(spacerPrefab, setActive: true);
		}

		public override void Reset()
		{
			base.Reset();

			buttonPrefab.gameObject.SetActive(false);
			spacerPrefab.SetActive(false);
			hasClicked = false;

			Click = ActionExtensions.GetEmpty<Action>();
			SetButtons(default(ConversationButtonThemeBlock));
		}

		protected override void OnPrepare()
		{
			base.OnPrepare();

			LayoutRebuilder.ForceRebuildLayoutImmediate(buttonArea.GetComponent<RectTransform>());
		}

		protected override void OnShowing(float scalar)
		{
			base.OnShowing(scalar);

			foreach (var entry in entries)
			{
				var opacityBias = 1f + buttonOpacityByOrder.Evaluate(entry.IndexNormal); // Approaches 2.0f
				entry.Instance.Group.alpha = opacityBias * scalar;
			}
		}

		protected override void OnShown()
		{
			base.OnShown();

			foreach (var entry in entries) entry.Instance.Group.alpha = 1f;
		}

		protected override void OnClosing(float scalar)
		{
			base.OnClosing(scalar);

			foreach (var entry in entries)
			{
				entry.Instance.Group.alpha = entry.IsSelection ? buttonSelectedOpacityOnClose.Evaluate(scalar) : buttonOpacityOnClose.Evaluate(scalar);
			}
		}

		protected override void OnClosed()
		{
			base.OnClosed();

			foreach (var entry in entries) entry.Instance.Group.alpha = 1f;
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			lookAtArea.LookAt(lookAtArea.position + (lookAtArea.position - App.V.CameraPosition).FlattenY());
		}

		protected override void OnSetInterfaceScale(int index)
		{
			buttonsScaleArea.localScale = buttonScales.GetVectorScale(index);
		}

		#region Events
		void OnClick(ButtonEntry entry)
		{
			if (hasClicked || !entry.Block.Interactable) return;

			hasClicked = true;

			entry.IsSelection = true;
			if (Click == null) Debug.LogError("A null Click was given, unable to complete...");
			else Click(entry.Block.Click);
		}
		#endregion
	}

	public interface IConversationButtonsView : IView
	{
		ConversationButtonThemeBlock DefaultTheme { get; }
		ConversationButtonThemeBlock CrewTheme { get; }
		ConversationButtonThemeBlock AwayTeamTheme { get; }
		ConversationButtonThemeBlock ForeignerTheme { get; }
		ConversationButtonThemeBlock DownlinkTheme { get; }

		Action<Action> Click { set; }

		void SetButtons(ConversationButtonThemeBlock theme, params ConversationButtonBlock[] blocks);
	}
}