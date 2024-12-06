using UnityEngine;

namespace Core.ObjectMap
{
	public interface IObjectProperties
	{
		string FullPathToObject { get; }
		bool Visible { get; set; }
		Vector3[] OriginalTransform { get; }
	}
}