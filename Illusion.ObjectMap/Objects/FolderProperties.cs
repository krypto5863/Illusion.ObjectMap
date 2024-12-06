using UnityEngine;

namespace Core.ObjectMap
{
	public class FolderProperties : IObjectProperties
	{
		public string FullPathToObject { get; set; }
		public bool Visible { get; set; }
		public string FolderName { get; set; }
		public Vector3[] OriginalTransform { get; set; }
	}
}