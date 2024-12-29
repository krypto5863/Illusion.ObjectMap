using Illusion.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.ObjectMap
{
	public static class GameObjectPathUtility
	{
		/// <summary>
		/// Gets the full hierarchical path of a GameObject, including sibling indices to differentiate duplicates.
		/// </summary>
		/// <param name="gameObject">The GameObject to retrieve the path for.</param>
		/// <returns>The unique path for the GameObject.</returns>
		public static string GetPathWithSiblingIndex(this GameObject gameObject)
		{
			if (gameObject == null)
				return null;

			var path = new StringBuilder();
			var current = gameObject.transform;

			while (current != null)
			{
				var nameWithIndex = $"{current.name}[{current.GetSiblingIndex()}]";
				if (path.Length > 0)
					path.Insert(0, $"{nameWithIndex}/");
				else
					path.Insert(0, nameWithIndex);

				current = current.parent;
			}

			return path.ToString();
		}

		public static GameObject FindByPathWithSiblingIndex(string path)
		{
			var parts = path.Split('/');
			Transform current = null;

			foreach (var part in parts)
			{
				// Extract name and index from the part (e.g., "Child[1]")
				var startIndex = part.LastIndexOf('[');
				var endIndex = part.LastIndexOf(']');
				if (startIndex == -1 || endIndex == -1)
					return null;

				var name = part.Substring(0, startIndex);
				var siblingIndex = int.Parse(part.Substring(startIndex + 1, endIndex - startIndex - 1));

				if (current == null)
				{
					// Start at the root
					var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
					current = Array.Find(rootObjects, obj => obj.name == name && obj.transform.GetSiblingIndex() == siblingIndex)?.transform;
				}
				else
				{
					// Find child by name and sibling index
					current = GetChildByNameAndIndex(current, name, siblingIndex);
				}

				if (current == null)
					return null;
			}

			return current?.gameObject;
		}

		private static Transform GetChildByNameAndIndex(this Transform parent, string name, int siblingIndex)
		{
			foreach (Transform child in parent)
			{
				if (child.name == name)
				{
					if (child.GetSiblingIndex() == siblingIndex)
						return child;
				}
			}
			return null;
		}

		public static int GetDescendantCount(this GameObject obj)
		{
			var count = 0;
			foreach (Transform child in obj.transform)
			{
				count++;
				count += child.gameObject.GetDescendantCount();
			}
			return count;
		}
	}
}