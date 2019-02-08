using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class OptionsMenuView : View, IOptionsMenuView
	{
		[Serializable]
		struct IconEntry
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public OptionsMenuIcons Icon;
			public Sprite Sprite;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
		}

		[Serializable]
		struct ThemeEntry
		{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
			public OptionsMenuThemes Theme;

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
		OptionsMenuEntryLeaf[] entryPrefabs;

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
			OptionsMenuThemes theme = OptionsMenuThemes.Neutral,
			params IOptionsMenuEntry[] entries
		)
		{
			theme = theme == OptionsMenuThemes.Unknown ? OptionsMenuThemes.Neutral : theme;

			var themeEntry = themes.FirstOrDefault(t => t.Theme == theme);

			foreach (var graphic in primaryColorGraphics) graphic.color = themeEntry.PrimaryColor;
			foreach (var graphic in secondaryColorGraphics) graphic.color = themeEntry.SecondaryColor;
			foreach (var graphic in tertiaryColorGraphics) graphic.color = themeEntry.TertiaryColor;

			entryArea.transform.ClearChildren<OptionsMenuEntryLeaf>();

			if (entries.None()) return;

			foreach (var entry in entries)
			{
				var entryType = entry.GetType();
				if (entryType == typeof(DividerOptionsMenuEntry)) InstantiateEntry<DividerOptionsMenuEntry, DividerOptionsMenuEntryLeaf>(entry, themeEntry, ApplyEntry);
				else if (entryType == typeof(LabelOptionsMenuEntry)) InstantiateEntry<LabelOptionsMenuEntry, LabelOptionsMenuEntryLeaf>(entry, themeEntry, ApplyEntry);
				else if (entryType == typeof(ButtonOptionsMenuEntry)) InstantiateEntry<ButtonOptionsMenuEntry, ButtonOptionsMenuEntryLeaf>(entry, themeEntry, ApplyEntry);
				else
				{
					Debug.LogError("Unrecognized entry type: " + entryType.FullName);
				}
			}
		}

		void InstantiateEntry<E, L>(IOptionsMenuEntry entry, ThemeEntry theme, Action<E, L, ThemeEntry> done)
			where E : OptionsMenuEntry<L>
			where L : OptionsMenuEntryLeaf
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
			DividerOptionsMenuEntry entry,
			DividerOptionsMenuEntryLeaf instance,
			ThemeEntry theme
		)
		{
			instance.DoubleSegmentArea.SetActive(entry.Segment == OptionsMenuDividerSegments.DoubleSegment);

			switch (entry.Segment)
			{
				case OptionsMenuDividerSegments.DoubleSegment: break;
				case OptionsMenuDividerSegments.None:
					var rectTransform = instance.GetComponent<RectTransform>();
					rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0f);
					break;
				default:
					Debug.LogError("Unrecognized Segment: " + entry.Segment);
					break;
			}

			instance.TopArea.SetActive(entry.Fade == OptionsMenuDividerFades.Top || entry.Fade == OptionsMenuDividerFades.All);
			instance.BottomArea.SetActive(entry.Fade == OptionsMenuDividerFades.Bottom || entry.Fade == OptionsMenuDividerFades.All);
		}

		void ApplyEntry(
			LabelOptionsMenuEntry entry,
			LabelOptionsMenuEntryLeaf instance,
			ThemeEntry theme
		)
		{
			instance.Label.text = entry.Message;

			if (entry.Icon == OptionsMenuIcons.None)
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
			ButtonOptionsMenuEntry entry,
			ButtonOptionsMenuEntryLeaf instance,
			ThemeEntry theme
		)
		{
			instance.Label.text = entry.Message;
			instance.Button.OnClick.AddListener(new UnityEngine.Events.UnityAction(() => OnClick(entry)));

			foreach (var target in instance.DisabledAreas) target.gameObject.SetActive(entry.InteractionState != ButtonOptionsMenuEntry.InteractionStates.Interactable);
			foreach (var target in instance.EnabledAreas) target.gameObject.SetActive(entry.InteractionState == ButtonOptionsMenuEntry.InteractionStates.Interactable);

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
				case ButtonOptionsMenuEntry.InteractionStates.Interactable: break;
				case ButtonOptionsMenuEntry.InteractionStates.LooksNotInteractable:
				case ButtonOptionsMenuEntry.InteractionStates.NotInteractable:
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

		public override void Reset()
		{
			base.Reset();

			foreach (var prefab in entryPrefabs) prefab.gameObject.SetActive(false);

			SetEntries();
		}

		#region Events
		void OnClick(ButtonOptionsMenuEntry entry)
		{
			Debug.LogWarning("Todo, this logic!");
		}
		#endregion
	}

	public interface IOptionsMenuView : IView
	{
		void SetEntries(OptionsMenuThemes theme = OptionsMenuThemes.Neutral, params IOptionsMenuEntry[] entries);
	}

	public enum OptionsMenuThemes
	{
		Unknown = 0,
		Neutral = 10,
		Warning = 20,
		Error = 30
	}

	public enum OptionsMenuStyles
	{
		Unknown = 0,
		Title = 10,
		Header = 20,
		Divider = 30,
		Button = 40
	}

	public enum OptionsMenuIcons
	{
		Unknown = 0,
		None = 10,
		Pause = 20
	}

	public enum OptionsMenuDividerFades
	{
		Unknown = 0,
		None = 10,
		All = 20,
		Top = 30,
		Bottom = 40
	}

	public enum OptionsMenuDividerSegments
	{
		Unknown = 0,
		None = 10,
		DoubleSegment = 20
	}

	public interface IOptionsMenuEntry
	{
		OptionsMenuStyles Style { get; }
		Type PrefabType { get; }
	}

	public abstract class OptionsMenuEntry<T> : IOptionsMenuEntry
		where T : OptionsMenuEntryLeaf
	{
		public abstract OptionsMenuStyles Style { get; }
		public Type PrefabType { get { return typeof(T); } }
	}


	public class DividerOptionsMenuEntry : OptionsMenuEntry<DividerOptionsMenuEntryLeaf>
	{
		public static DividerOptionsMenuEntry CreateDivider(
			OptionsMenuDividerSegments segment = OptionsMenuDividerSegments.DoubleSegment,
			OptionsMenuDividerFades fade = OptionsMenuDividerFades.None
		)
		{ return new DividerOptionsMenuEntry(OptionsMenuStyles.Divider, segment, fade); }

		OptionsMenuStyles style;
		public override OptionsMenuStyles Style { get { return style; } }

		public OptionsMenuDividerSegments Segment;
		public OptionsMenuDividerFades Fade;

		DividerOptionsMenuEntry(
			OptionsMenuStyles style,
			OptionsMenuDividerSegments segment = OptionsMenuDividerSegments.DoubleSegment,
			OptionsMenuDividerFades fade = OptionsMenuDividerFades.None
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

	public class LabelOptionsMenuEntry : OptionsMenuEntry<LabelOptionsMenuEntryLeaf>
	{
		public static LabelOptionsMenuEntry CreateTitle(string message, OptionsMenuIcons icon = OptionsMenuIcons.None) { return new LabelOptionsMenuEntry(OptionsMenuStyles.Title, message, icon); }
		public static LabelOptionsMenuEntry CreateHeader(string message) { return new LabelOptionsMenuEntry(OptionsMenuStyles.Header, message); }

		OptionsMenuStyles style;
		public override OptionsMenuStyles Style { get { return style; } }

		public string Message;
		public OptionsMenuIcons Icon;

		LabelOptionsMenuEntry(
			OptionsMenuStyles style,
			string message,
			OptionsMenuIcons icon = OptionsMenuIcons.None
		)
		{
			this.style = style;
			Message = message;
			Icon = icon;
		}
	}

	public class ButtonOptionsMenuEntry : OptionsMenuEntry<ButtonOptionsMenuEntryLeaf>
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

		public static ButtonOptionsMenuEntry CreateButton(
			string message,
			Action click,
			InteractionStates interactionState = InteractionStates.Interactable
		)
		{
			return new ButtonOptionsMenuEntry(
				OptionsMenuStyles.Button,
				message,
				click,
				interactionState
			);
		}

		OptionsMenuStyles style;
		public override OptionsMenuStyles Style { get { return style; } }

		public string Message;
		public Action Click;
		public InteractionStates InteractionState;

		ButtonOptionsMenuEntry(
			OptionsMenuStyles style,
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