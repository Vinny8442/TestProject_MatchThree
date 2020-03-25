using Unity.Entities;

namespace Notifications
{
	public struct CellDestroyNotification : IComponentData
	{
		public Entity Entity;
	}
}