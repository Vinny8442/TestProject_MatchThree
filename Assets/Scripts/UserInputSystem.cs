using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class UserInputSystem : ComponentSystem
	{
		private EntityArchetype _notificationArchetype;

		protected override void OnStartRunning()
		{
			_notificationArchetype = EntityManager.CreateArchetype(typeof(CellClickedNotification));
		}

		protected override void OnUpdate()
		{
			if (Input.GetMouseButtonDown(0))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				Plane plane = new Plane(Vector3.forward, 0);
				if (plane.Raycast(ray, out var result))
				{
					var clickPosition = ray.GetPoint(result);
						
					Entities.WithAllReadOnly<CellPosition>().ForEach((Entity entity, ref CellPosition cellPosition, ref CellSelectionFlag selection) => 
					{
						if (clickPosition.x > cellPosition.x - 0.5f && clickPosition.x < cellPosition.x + 0.5f &&
						    clickPosition.y > cellPosition.y - 0.5f && clickPosition.y < cellPosition.y + 0.5f)
						{
							// EntityManager.SetComponentData(entity, new CellSelectionFlag{Value = !selection.Value});
							Entity notification = EntityManager.CreateEntity(_notificationArchetype);
							EntityManager.SetComponentData(notification, new CellClickedNotification{entity = entity});
						} 
					});
				}
			}
		}
	}
}