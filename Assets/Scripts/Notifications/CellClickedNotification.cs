using Unity.Entities;

namespace DefaultNamespace
{
	public struct CellClickedNotification : IComponentData
	{
		public Entity entity;
	}
}