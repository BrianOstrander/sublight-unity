using System.Collections.Generic;
using UnityEngine;

public class CanvasGroupOpacityHack : MonoBehaviour
{
	[SerializeField]
	CanvasGroup[] listeningGroups = new CanvasGroup[0];
	[SerializeField]
	RectTransform[] targets = new RectTransform[0];

	[SerializeField, HideInInspector]
	bool hasInitialized;
	[SerializeField, HideInInspector]
	List<CanvasGroup> added = new List<CanvasGroup>();

	void Awake()
	{
		if (hasInitialized) return;
		hasInitialized = true;
		foreach (var target in targets)
		{
			if (target == null) continue;
			//var existing = 
			added.Add(target.gameObject.AddComponent<CanvasGroup>());
		}
		OnUpdate();
	}

	void Update()
	{
		OnUpdate(); 
	}

	// Update is called once per frame
	void OnUpdate()
    {
		if (!hasInitialized) return;
		var opacity = 1f;
		foreach (var group in listeningGroups) opacity *= group.alpha;
		foreach (var entry in added) entry.alpha = opacity;
    }
}
