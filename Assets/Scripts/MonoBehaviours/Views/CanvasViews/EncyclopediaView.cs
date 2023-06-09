﻿using System;

using UnityEngine;
using UnityEngine.Events;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class EncyclopediaView : CanvasView, IEncyclopediaView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		LabelButtonLeaf articleEntryPrefab;
		[SerializeField]
		ArticleSectionLeaf sectionEntryPrefab;
		[SerializeField]
		GameObject articleEntryArea;
		[SerializeField]
		GameObject sectionEntryArea;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public string Title { set { titleLabel.text = value ?? string.Empty; } }

		public LabelButtonBlock[] ArticleEntries
		{
			set
			{
				articleEntryArea.transform.ClearChildren<LabelButtonLeaf>();
				if (value == null) return;
				foreach (var entry in value)
				{
					var instance = articleEntryArea.InstantiateChild(articleEntryPrefab, setActive: true);
					instance.ButtonLabel.text = entry.Text ?? string.Empty;
					instance.Button.OnClick.AddListener(new UnityAction(entry.Click ?? ActionExtensions.Empty));
				}
			}
		}

		public ArticleSectionBlock[] SectionEntries
		{
			set
			{
				sectionEntryArea.transform.ClearChildren<ArticleSectionLeaf>();
				if (value == null) return;
				foreach (var entry in value)
				{
					var instance = sectionEntryArea.InstantiateChild(sectionEntryPrefab, setActive: true);
					instance.HeaderLabel.gameObject.SetActive(!string.IsNullOrEmpty(entry.Header));
					instance.HeaderLabel.text = entry.Header ?? string.Empty;
					instance.BodyLabel.text = entry.Body ?? string.Empty;
				}
			}
		}

		public Action BackClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			articleEntryPrefab.gameObject.SetActive(false);
			sectionEntryPrefab.gameObject.SetActive(false);

			Title = string.Empty;
			ArticleEntries = null;
			SectionEntries = null;
			BackClick = ActionExtensions.Empty;
		}


		#region Events
		public void OnBackClick() { BackClick(); }
		#endregion
	}

	public interface IEncyclopediaView : ICanvasView
	{
		string Title { set; }
		LabelButtonBlock[] ArticleEntries { set; }
		ArticleSectionBlock[] SectionEntries { set; }
		Action BackClick { set; }
	}
}