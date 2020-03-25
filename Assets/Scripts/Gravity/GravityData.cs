using Unity.Entities;

namespace DefaultNamespace
{
	public struct GravityData : IComponentData
	{
		public GravityDirection direction;
	}
}