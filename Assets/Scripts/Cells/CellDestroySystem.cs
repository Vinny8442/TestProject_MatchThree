using Notifications;
using Unity.Entities;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	[UpdateAfter(typeof(RenderSystem))]
	public class CellDestroySystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			// bool changed = false;
			Entities.ForEach((Entity entity, ref CellDestroyNotification notification) =>
			{
				Entity cell = notification.Entity;
				EntityManager.DestroyEntity(entity);
				EntityManager.DestroyEntity(cell);
				// changed = true;
			});
		}
	}
}