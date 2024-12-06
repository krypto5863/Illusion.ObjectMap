using MessagePack;
using UnityEngine;

namespace Core.ObjectMap
{
	[MessagePackObject(true)]
	public class ObjectPosition
	{
		public string Id;
		public Vector3[] Transform;
	}
}