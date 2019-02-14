using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class VerticalOptionsView : View, IVerticalOptionsView
	{
		[Serializable]
		struct IconEntry
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public VerticalOptionsIcons Icon;
			public Sprite Sprite;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}

		[Serializable]
		struct ThemeEntry
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public VerticalOptionsThemes Theme;

			public ColorStyleBlock PrimaryColor;
			public ColorStyleBlock SecondaryColor;
			public ColorStyleBlock TertiaryColor;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		GameObject entryArea;

		[SerializeField]
		RectTransform entryBackground;

		[SerializeField]
		CanvasGroup rootGroup;
		[SerializeField]
		CanvasGroup entryGroup;
		[SerializeField]
		CanvasGroup entryBackgroundGroup;

		[SerializeField]
		VerticalOptionsEntryLeaf[] entryPrefabs;

		[SerializeField]
		ThemeEntry[] themes;
		[SerializeField]
		IconEntry[] icons;

		[SerializeField]
		Graphic[] primaryColorGraphics;
		[SerializeField]
		Graphic[] secondaryColorGraphics;
		[SerializeField]
		Graphic[] tertiaryColorGraphics;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public void SetEntries(
			VerticalOptionsThemes theme = VerticalOptionsThemes.Neutral,
			params IVerticalOptionsEntry[] entries
		)
		{
			theme = theme == VerticalOptionsThemes.Unknown ? VerticalOptionsThemes.Neutral : theme;

			var themeEntry = themes.FirstOrDefault(t => t.Theme == theme);

			foreach (var graphic in primaryColorGraphics) graphic.color = themeEntry.PrimaryColor;
			foreach (var graphic in secondaryColorGraphics) graphic.color = themeEntry.SecondaryColor;
			foreach (var graphic in tertiaryColorGraphics) graphic.color = themeEntry.TertiaryColor;

			entryArea.transform.ClearChildren<VerticalOptionsEntryLeaf>();

			if (entries.None()) return;

			foreach (var entry in entries)
			{
				var entryType = entry.GetType();
				if (entryType == typeof(DividerVerticalOptionsEntry)) InstantiateEntry<DividerVerticalOptionsEntry, DividerVerticalOptionsEntryLeaf>(entry, themeEntry, ApplyEntry);
				else if (entryType == typeof(LabelVerticalOptionsEntry)) InstantiateEntry<LabelVerticalOptionsEntry, LabelVerticalOptionsEntryLeaf>(entry, themeEntry, ApplyEntry);
				else if (entryType == typeof(ButtonVerticalOptionsEntry)) InstantiateEntry<ButtonVerticalOptionsEntry, ButtonVerticalOptionsEntryLeaf>(entry, themeEntry, ApplyEntry);
				else
				{
					Debug.LogError("Unrecognized entry type: " + entryType.FullName);
				}
			}
		}

		void InstantiateEntry<E, L>(IVerticalOptionsEntry entry, ThemeEntry theme, Action<E, L, ThemeEntry> done)
			where E : VerticalOptionsEntry<L>
			where L : VerticalOptionsEntryLeaf
		{
			var prefab = entryPrefabs.FirstOrDefault(p => p.GetType() == typeof(L) && p.Style == entry.Style);

			if (prefab == null)
			{
				Debug.LogError("No prefab exists of type " + typeof(L).FullName + " with style " + entry.Style);
				return;
			}
			var instance = entryArea.InstantiateChild(prefab as L, setActive: true);

			foreach (var graphic in instance.PrimaryColorGraphics) graphic.color = theme.PrimaryColor;
			foreach (var graphic in instance.SecondaryColorGraphics) graphic.color = theme.SecondaryColor;
			foreach (var graphic in instance.TertiaryColorGraphics) graphic.color = theme.TertiaryColor;

			done(entry as E, instance, theme);
		}

		void ApplyEntry(
			DividerVerticalOptionsEntry entry,
			DividerVerticalOptionsEntryLeaf instance,
			ThemeEntry theme
		)
		{
			instance.DoubleSegmentArea.SetActive(entry.Segment == VerticalOptionsDividerSegments.DoubleSegment);

			switch (entry.Segment)
			{
				case VerticalOptionsDividerSegments.DoubleSegment: break;
				case VerticalOptionsDividerSegments.None:
					var rectTransform = instance.GetComponent<RectTransform>();
					rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0f);
					break;
				default:
					Debug.LogError("Unrecognized Segment: " + entry.Segment);
					break;
			}

			instance.TopArea.SetActive(entry.Fade == VerticalOptionsDividerFades.Top || entry.Fade == VerticalOptionsDividerFades.All);
			instance.BottomArea.SetActive(entry.Fade == VerticalOptionsDividerFades.Bottom || entry.Fade == VerticalOptionsDividerFades.All);
		}

		void ApplyEntry(
			LabelVerticalOptionsEntry entry,
			LabelVerticalOptionsEntryLeaf instance,
			ThemeEntry theme
		)
		{
			instance.Label.text = entry.Message;

			if (entry.Icon == VerticalOptionsIcons.None)
			{
				if (instance.Icon != null) instance.Icon.gameObject.SetActive(false);
				return;
			}
			if (instance.Icon == null)
			{
				Debug.LogError("Unable to set icon, ignoring", instance);
				return;
			}

			var iconEntry = icons.FirstOrDefault(i => i.Icon == entry.Icon);

			if (iconEntry.Sprite == null)
			{
				Debug.LogError("Unrecognized Icon: " + entry.Icon);
				instance.Icon.gameObject.SetActive(false);
			}
			else instance.Icon.sprite = iconEntry.Sprite;
		}

		void ApplyEntry(
			ButtonVerticalOptionsEntry entry,
			ButtonVerticalOptionsEntryLeaf instance,
			ThemeEntry theme
		)
		{
			instance.Label.text = entry.Message;
			instance.Button.OnClick.AddListener(new UnityEngine.Events.UnityAction(() => OnClick(entry)));

			foreach (var target in instance.DisabledAreas) target.gameObject.SetActive(entry.InteractionState != ButtonVerticalOptionsEntry.InteractionStates.Interactable);
			foreach (var target in instance.EnabledAreas) target.gameObject.SetActive(entry.InteractionState == ButtonVerticalOptionsEntry.InteractionStates.Interactable);

			var backgroundTarget = instance.BackgroundAreas.First(a => a.gameObject.activeSelf);
			var highlightTarget = instance.HighlightAreas.First(a => a.gameObject.activeSelf);
			var labelTarget = instance.LabelAreas.First(a => a.gameObject.activeSelf);

			var backgroundColors = backgroundTarget.LocalStyle.Colors.Duplicate;
			var highlightColors = highlightTarget.LocalStyle.Colors.Duplicate;
			var labelColors = labelTarget.LocalStyle.Colors.Duplicate;

			backgroundColors.NormalColor = theme.SecondaryColor.Color.NewA(backgroundColors.NormalColor.a);
			backgroundColors.HighlightedColor = theme.PrimaryColor.Color.NewA(backgroundColors.HighlightedColor.a);
			backgroundColors.PressedColor = theme.PrimaryColor.Color.NewA(backgroundColors.PressedColor.a);

			highlightColors.NormalColor = theme.SecondaryColor.Color.NewA(highlightColors.NormalColor.a);
			highlightColors.HighlightedColor = theme.PrimaryColor.Color.NewA(highlightColors.HighlightedColor.a);
			highlightColors.PressedColor = theme.PrimaryColor.Color.NewA(highlightColors.PressedColor.a);

			labelColors.NormalColor = theme.PrimaryColor.Color.NewA(labelColors.NormalColor.a);
			labelColors.HighlightedColor = theme.PrimaryColor.Color.NewA(labelColors.HighlightedColor.a);
			labelColors.PressedColor = theme.SecondaryColor.Color.NewA(labelColors.PressedColor.a);

			switch (entry.InteractionState)
			{
				case ButtonVerticalOptionsEntry.InteractionStates.Interactable: break;
				case ButtonVerticalOptionsEntry.InteractionStates.LooksNotInteractable:
				case ButtonVerticalOptionsEntry.InteractionStates.NotInteractable:
					labelColors.NormalColor = labelColors.NormalColor.NewA(labelColors.NormalColor.a * 0.5f);
					labelColors.HighlightedColor = Color.black.NewA(labelColors.HighlightedColor.a);
					labelColors.PressedColor = Color.black.NewA(labelColors.PressedColor.a);
					break;
				default:
					Debug.LogError("Unrecognized InteractionState: " + entry.InteractionState);
					break;
			}

			backgroundTarget.LocalStyle.Colors = backgroundColors;
			highlightTarget.LocalStyle.Colors = highlightColors;
			labelTarget.LocalStyle.Colors = labelColors;
		}

		protected override void OnOpacityStack(float opacity)
		{
			rootGroup.alpha = opacity;
		}

		public override void Reset()
		{
			base.Reset();

			foreach (var prefab in entryPrefabs) prefab.gameObject.SetActive(false);

			SetEntries();
		}

		#region Events
		void OnClick(ButtonVerticalOptionsEntry entry)
		{
			switch (entry.InteractionState)
			{
				case ButtonVerticalOptionsEntry.InteractionStates.NotInteractable: break;
				case ButtonVerticalOptionsEntry.InteractionStates.Interactable:
				case ButtonVerticalOptionsEntry.InteractionStates.LooksNotInteractable:
					if (entry.Click == null) Debug.LogError("Entry should be clickable, but no event provided");
					else entry.Click();
					break;
				default:
					Debug.LogError("Unrecognized InteractionState: " + entry.InteractionState);
					break;
			}
		}
		#endregion
	}

	public interface IVerticalOptionsView : IView
	{
		void SetEntries(VerticalOptionsThemes theme = VerticalOptionsThemes.Neutral, params IVerticalOptionsEntry[] entries);
	}

	public enum VerticalOptionsThemes
	{
		Unknown = 0,
		Neutral = 10,
		Warning = 20,
		Error = 30,
		Success = 40
	}

	public enum VerticalOptionsStyles
	{
		Unknown = 0,
		Title = 10,
		Header = 20,
		Divider = 30,
		Button = 40
	}

	public enum VerticalOptionsIcons
	{
		Unknown = 0,
		None = 10,
		Pause = 20,
		Save = 30,
		Return = 40,
		Quit = 50,
		// -- Game Icons
		GameFailure = 1000,
		GameSuccess = 1010,
	}

	public enum VerticalOptionsDividerFades
	{
		Unknown = 0,
		None = 10,
		All = 20,
		Top = 30,
		Bottom = 40
	}

	public enum VerticalOptionsDividerSegments
	{
		Unknown = 0,
		None = 10,
		DoubleSegment = 20
	}

	public interface IVerticalOptionsEntry
	{
		VerticalOptionsStyles Style { get; }
		Type PrefabType { get; }
	}

	public abstract class VerticalOptionsEntry<T> : IVerticalOptionsEntry
		where T : VerticalOptionsEntryLeaf
	{
		public abstract VerticalOptionsStyles Style { get; }
		public Type PrefabType { get { return typeof(T); } }
	}


	public class DividerVerticalOptionsEntry : VerticalOptionsEntry<DividerVerticalOptionsEntryLeaf>
	{
		public static DividerVerticalOptionsEntry CreateDivider(
			VerticalOptionsDividerSegments segment = VerticalOptionsDividerSegments.DoubleSegment,
			VerticalOptionsDividerFades fade = VerticalOptionsDividerFades.None
		)
		{ return new DividerVerticalOptionsEntry(VerticalOptionsStyles.Divider, segment, fade); }

		VerticalOptionsStyles style;
		public override VerticalOptionsStyles Style { get { return style; } }

		public VerticalOptionsDividerSegments Segment;
		public VerticalOptionsDividerFades Fade;

		DividerVerticalOptionsEntry(
			VerticalOptionsStyles style,
			VerticalOptionsDividerSegments segment = VerticalOptionsDividerSegments.DoubleSegment,
			VerticalOptionsDividerFades fade = VerticalOptionsDividerFades.None
		)
		{
			this.style = style;
			Segment = segment;
			Fade = fade;
		}
	}

	/* Currently unused
	public class GenericOptionsMenuEntry : OptionsMenuEntry<OptionsMenuEntryLeaf>
	{
		//public static GenericOptionsMenuEntry CreateDivider() { return new GenericOptionsMenuEntry(OptionsMenuStyles.Divider); }

		OptionsMenuStyles style;
		public override OptionsMenuStyles Style { get { return style; } }

		GenericOptionsMenuEntry(OptionsMenuStyles style)
		{
			this.style = style;
		}
	}
	*/

	public class LabelVerticalOptionsEntry : VerticalOptionsEntry<LabelVerticalOptionsEntryLeaf>
	{
		public static LabelVerticalOptionsEntry CreateTitle(string message, VerticalOptionsIcons icon = VerticalOptionsIcons.None) { return new LabelVerticalOptionsEntry(VerticalOptionsStyles.Title, message, icon); }
		public static LabelVerticalOptionsEntry CreateHeader(string message) { return new LabelVerticalOptionsEntry(VerticalOptionsStyles.Header, message); }

		VerticalOptionsStyles style;
		public override VerticalOptionsStyles Style { get { return style; } }

		public string Message;
		public VerticalOptionsIcons Icon;

		LabelVerticalOptionsEntry(
			VerticalOptionsStyles style,
			string message,
			VerticalOptionsIcons icon = VerticalOptionsIcons.None
		)
		{
			this.style = style;
			Message = message;
			Icon = icon;
		}
	}

	public class ButtonVerticalOptionsEntry : VerticalOptionsEntry<ButtonVerticalOptionsEntryLeaf>
	{
		public enum InteractionStates
		{
			Unknown = 0,
			Interactable = 10,
			NotInteractable = 20,
			/// <summary>
			/// The button can be interacted with, but it looks not interactable.
			/// </summary>
			/// <remarks>
			/// This is useful for showing a popup of why it's not interactable.
			/// </remarks>
			LooksNotInteractable = 30
		}

		public static ButtonVerticalOptionsEntry CreateButton(
			string message,
			Action click,
			InteractionStates interactionState = InteractionStates.Interactable
		)
		{
			return new ButtonVerticalOptionsEntry(
				VerticalOptionsStyles.Button,
				message,
				click,
				interactionState
			);
		}

		VerticalOptionsStyles style;
		public override VerticalOptionsStyles Style { get { return style; } }

		public string Message;
		public Action Click;
		public InteractionStates InteractionState;

		ButtonVerticalOptionsEntry(
			VerticalOptionsStyles style,
			string message,
			Action click,
			InteractionStates interactionState
		)
		{
			this.style = style;
			Message = message;
			Click = click;
			InteractionState = interactionState;
		}
	}
}