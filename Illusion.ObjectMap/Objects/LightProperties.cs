using UnityEngine;

namespace Core.ObjectMap
{
	public class LightProperties : IObjectProperties
	{
		public Color Color;
		public float Intensity;
		public float Range;
		public bool Shadow;
		public float SpotAngle;
		public string FullPathToObject { get; set; }
		public bool Visible { get; set; }
		public Vector3[] OriginalTransform { get; set; }
	}
}