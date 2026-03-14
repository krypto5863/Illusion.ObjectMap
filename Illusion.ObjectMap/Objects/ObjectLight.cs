using MessagePack;

namespace Core.ObjectMap
{
	[MessagePackObject(true)]
	public class ObjectLight
	{
		public string Color;
		public bool DrawTarget;
		public bool Enable;
		public string Id;
		public float Intensity;
		public float Range;
		public bool Shadow;
		public float SpotAngle;
	}
}