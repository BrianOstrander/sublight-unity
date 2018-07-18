using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using LunraGames;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class DestructionSpeedView : CanvasView, IDestructionSpeedView
	{
		struct SpeedEntry
		{
			public bool HasShown;
			public float Speed;
			public DayTime Start;
			public DestructionSpeedAlertLeaf Leaf;

			public SpeedEntry(float speed, DayTime start, DestructionSpeedAlertLeaf leaf)
			{
				HasShown = false;
				Speed = speed;
				Start = start;
				Leaf = leaf;
			}
		}

		[SerializeField]
		float unitsInTimeline;
		[SerializeField]
		RawImage timelineImage;
		[SerializeField]
		RectTransform alertArea;
		[SerializeField]
		RectTransform timelineMin;
		[SerializeField]
		RectTransform timelineMax;
		[SerializeField]
		DestructionSpeedAlertLeaf alertEntryPrefab;

		List<SpeedEntry> entries = new List<SpeedEntry>();

		public float TimeInUnit { set; private get; }
		public float Speed { set; private get; }
		DayTime current;
		public DayTime Current
		{
			get { return current; }
			set
			{
				current = value;
				// TODO: Make this take DaysInUnit into account...
				timelineImage.uvRect = timelineImage.uvRect.NewX(current.Time);
				UpdateAlerts();
			}
		}

		public void AddAlert(float speed, DayTime start)
		{
			var previousStart = Current;
			var previousSpeed = Speed;
			foreach (var entry in entries)
			{
				if (entry.Start < start && previousStart < entry.Start)
				{
					previousStart = entry.Start;
					previousSpeed = entry.Speed;
				}
			}
			var leaf = alertArea.gameObject.InstantiateChild(alertEntryPrefab, setActive: false);
			var delta = previousSpeed * 2f < speed ? 0 : 1;
			var isUp = previousSpeed < speed;
			for (var i = 0; i < leaf.UpArrows.Length; i++) leaf.UpArrows[i].SetActive(isUp && i == delta);
			for (var i = 0; i < leaf.DownArrows.Length; i++) leaf.DownArrows[i].SetActive(!isUp && i == delta);
			entries.Add(new SpeedEntry(speed, start, leaf));
		}

		public override void Reset()
		{
			base.Reset();

			alertEntryPrefab.gameObject.SetActive(false);
			alertArea.ClearChildren();
			entries.Clear();
			TimeInUnit = 1f;
			Speed = 0f;
			Current = DayTime.Zero;
		}

		void UpdateAlerts()
		{
			DayTime min = Current;
			DayTime max = Current + new DayTime(TimeInUnit * unitsInTimeline);
			var newEntries = new List<SpeedEntry>();
			foreach (var entry in entries)
			{
				if (entry.Start < min)
				{
					Destroy(entry.Leaf.gameObject);
					continue;
				}
				newEntries.Add(entry);
				if (max < entry.Start) continue;
				if (!entry.HasShown) entry.Leaf.gameObject.SetActive(true);
				entry.Leaf.transform.localPosition = timelineMin.localPosition + ((timelineMax.localPosition - timelineMin.localPosition) * entry.Start.ClampedNormal(min, max));
			}
			entries = newEntries;
		}
	}

	public interface IDestructionSpeedView : ICanvasView
	{
		float TimeInUnit { set; }
		float Speed { set; }
		DayTime Current { set; }
		void AddAlert(float speed, DayTime start);
	}
}