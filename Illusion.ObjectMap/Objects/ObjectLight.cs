using MessagePack;

namespace Core.ObjectMap
{
	[MessagePackObject(true)]
	public class ObjectLight
	{
		public string Id;
		public string Color;
		public float Intensity;
		public float Range;
		public float SpotAngle;
		public bool Shadow;
		public bool Enable;
		public bool DrawTarget;
	}
}