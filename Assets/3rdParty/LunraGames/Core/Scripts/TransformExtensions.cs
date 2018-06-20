using Object = UnityEngine.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LunraGames
{
	public static class TransformExtensions
	{
		public static void ClearChildren(this Transform transform, Func<Transform, bool> condition = null)
		{
			transform.ClearChildren<Transform>(condition);
		}

		public static void ClearChildren<T>(this Transform transform, Func<T, bool> condition = null) where T : Component
		{
			foreach (var child in transform.GetChildren(condition))
			{
				Object.Destroy(child.gameObject);
			}
		}

		public static void SetChildrenActive(this Transform transform, bool active) {
			for (var i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(active);
		}

		public static List<Transform> GetChildren(this Transform transform, Func<Transform, bool> condition = null)
		{
			return transform.GetChildren<Transform>(condition);
		}

		public static List<T> GetChildren<T>(this Transform transform, Func<T, bool> condition = null) where T : Component
		{
			var result = new List<T>();

			for (var i = 0; i < transform.childCount; i++) 
			{
				var child = transform.GetChild(i).GetComponent<T>();
				if (child != null && (condition == null || condition(child))) result.Add(child);
			}

			return result;
		}

		public static List<Transform> GetDescendants(this Transform transform, Func<Transform, bool> condition = null)
		{
			return transform.GetDescendants<Transform>(condition);
		}

		public static List<T> GetDescendants<T>(this Transform transform, Func<T, bool> condition = null) where T : Component
		{
			var result = new List<T>();
			var children = transform.GetChildren();
			foreach (var child in children) 
			{
				var component = child.GetComponent<T>();
				if (component != null && (condition == null || condition(component))) result.Add(component);
				result.AddRange(child.GetDescendants(condition));
			}

			return result;
		}

		public static Transform GetFirstDescendantOrDefault(this Transform transform, Func<Transform, bool> condition = null)
		{
			return transform.GetFirstDescendantOrDefault<Transform>(condition);
		}

		public static T GetFirstDescendantOrDefault<T>(this Transform transform, Func<T, bool> condition = null) where T : Component
		{
			var children = transform.GetChildren();
			foreach (var child in children) 
			{
				var component = child.GetComponent<T>();
				if (component != null && (condition == null || condition(component))) return component;
				var result = child.GetFirstDescendantOrDefault(condition);
				if (result != null) return result;
			}

			return null;
		}

		/// <summary>
		/// Play mode only!
		/// </summary>
		/// <remarks>
		/// Thanks Onur!
		/// </remarks>
		public static bool IsPrefab(this Transform transform) {
			if (Application.isEditor && !Application.isPlaying) throw new InvalidOperationException("IsPrefab only allowed to be used in playmode");

			return transform.gameObject.scene.buildIndex < 0;
		}
	}
}