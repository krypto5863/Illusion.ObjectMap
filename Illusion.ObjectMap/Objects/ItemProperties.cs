using UnityEngine;

namespace Core.ObjectMap
{
	public class ItemProperties : IObjectProperties
	{
		public string FullPathToObject { get; set; }
		public bool Visible { get; set; }
		public Vector3[] OriginalTransform { get; set; }
	}
}