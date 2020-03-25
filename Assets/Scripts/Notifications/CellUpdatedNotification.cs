using Unity.Entities;

namespace DefaultNamespace
{
	public struct CellUpdatedNotification : IComponentData
	{
		public Entity Entity;
	}
}