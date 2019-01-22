using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace LunraGames.SubLight.Views
{
	public struct BustButtonBlock
	{
		public string Message;
		public bool Used;
		public bool Interactable;
		public Action Click;
	}

	public struct BustButtonThemeBlock
	{
		public struct State
		{
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

	public class BustButtonsView : View, IBustButtonsView
	{
		class ButtonEntry
		{
			public BustButtonBlock Block;
			public BustButtonLeaf Instance;

			public float IndexNormal;
			public bool IsSelection;
		}

		[Serializable]
		struct BustButtonStyleBlock
		{
			[Serializable]
			public struct State
			{
				public XButtonStyleObject LabelStyle;
				public XButtonStyleObject BulletStyle;
				public XButtonStyleObject UnderlineStyle;
				public XButtonStyleObject BackgroundStyle;
			}

			[SerializeField]
			State normal;
			[Header("Leave fields in the following states blank to default to the normal style")]
			[SerializeField]
			State used;
			[SerializeField]
			State notInteractable;
			[SerializeField]
			State selected;

			State GetNormalized(State target)
			{
				return new State
				{
					// You'll be tempted to use ?? here, don't do that, Unity messes with null Objects.
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
			public HsvMultiplier Multiplier;
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		Transform lookAtArea;
		[SerializeField]
		GameObject buttonArea;
		[SerializeField]
		BustButtonLeaf buttonPrefab;

		[Header("Button Animations ( 0 is hidden 1 is shown )")]
		[SerializeField]
		AnimationCurve buttonOpacityByOrder;

		[Header("Themes")]
		[SerializeField]
		BustButtonStyleBlock defaultTheme;
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
			return modifier.Multiplier.Apply(baseColor.NewH(modifier.Hue.GetH()));
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

		BustButtonThemeBlock.State GetModifiedStateTheme(BustButtonStyleBlock.State baseState, ThemeModifier modifier)
		{
			return new BustButtonThemeBlock.State
			{
				LabelStyle = GetModifiedStateTheme(baseState.LabelStyle, modifier),
				BulletStyle = GetModifiedStateTheme(baseState.BulletStyle, modifier),
				UnderlineStyle = GetModifiedStateTheme(baseState.UnderlineStyle, modifier),
				BackgroundStyle = GetModifiedStateTheme(baseState.BackgroundStyle, modifier)
			};
		}

		BustButtonThemeBlock GetModifiedTheme(BustButtonStyleBlock baseTheme, ThemeModifier modifier)
		{
			return new BustButtonThemeBlock
			{
				Normal = GetModifiedStateTheme(baseTheme.Normal, modifier),
				Used = GetModifiedStateTheme(baseTheme.Used, modifier),
				NotInteractable = GetModifiedStateTheme(baseTheme.NotInteractable, modifier),
				Selected = GetModifiedStateTheme(baseTheme.Selected, modifier),
			};
		}

		public BustButtonThemeBlock DefaultTheme { get { return GetModifiedTheme(defaultTheme, new ThemeModifier { Hue = Color.white, Multiplier = new HsvMultiplier() }); } }
		public BustButtonThemeBlock CrewTheme { get { return GetModifiedTheme(defaultTheme, crewModifier); } }
		public BustButtonThemeBlock AwayTeamTheme { get { return GetModifiedTheme(defaultTheme, awayTeamModifier); } }
		public BustButtonThemeBlock ForeignerTheme { get { return GetModifiedTheme(defaultTheme, foreignerModifier); } }
		public BustButtonThemeBlock DownlinkTheme { get { return GetModifiedTheme(defaultTheme, downlinkModifier); } }

		BustButtonThemeBlock activeTheme;
		List<ButtonEntry> entries = new List<ButtonEntry>();

		public void SetButtons(
			BustButtonThemeBlock theme,
			bool instant,
			params BustButtonBlock[] blocks
		)
		{
			buttonArea.transform.ClearChildren<BustButtonLeaf>();
			entries.Clear();

			activeTheme = theme;

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

				entry.Instance.LabelArea.LocalStyle = style.LabelStyle;
				entry.Instance.BulletArea.LocalStyle = style.BulletStyle;
				entry.Instance.UnderlineArea.LocalStyle = style.UnderlineStyle;
				entry.Instance.BackgroundArea.LocalStyle = style.BackgroundStyle;

				index++;
			}

			if (instant) Debug.LogError("TODO INSTANT");
		}

		public override void Reset()
		{
			base.Reset();

			buttonPrefab.gameObject.SetActive(false);

			SetButtons(default(BustButtonThemeBlock), true);
		}

		protected override void OnLateIdle(float delta)
		{
			base.OnLateIdle(delta);

			lookAtArea.LookAt(lookAtArea.position + (lookAtArea.position - App.V.CameraPosition).FlattenY());
		}

		#region Events
		void OnClick(ButtonEntry entry)
		{
			Debug.Log("clicked: " + entry.Block.Message);
		}
		#endregion

		void OnDrawGizmos()
		{
			//Gizmos.color = Color.red;
			//Gizmos.DrawLine(bustPrefab.AvatarAnchor.position, bustPrefab.AvatarDepthAnchor.position);
		}
	}

	public interface IBustButtonsView : IView
	{
		BustButtonThemeBlock DefaultTheme { get; }
		BustButtonThemeBlock CrewTheme { get; }
		BustButtonThemeBlock AwayTeamTheme { get; }
		BustButtonThemeBlock ForeignerTheme { get; }
		BustButtonThemeBlock DownlinkTheme { get; }

		void SetButtons(
			BustButtonThemeBlock theme,
			bool instant,
			params BustButtonBlock[] blocks
		);
	}
}