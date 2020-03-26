using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class UserInputSystem : ComponentSystem
	{
		private WorldPositionConverter _converter;

		protected override void OnStartRunning()
		{
			_converter = GameStateHelper.CreateWorldPositionConverter(Entities);
		}

		protected override void OnUpdate()
		{
			if (Input.GetMouseButtonDown(0))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				Plane plane = new Plane(Vector3.forward, 0);
				if (plane.Raycast(ray, out var result))
				{
					var clickPosition = _converter.WorldToLogic(ray.GetPoint(result));
						
					Entities.WithAllReadOnly<CellPosition>().ForEach((Entity entity, ref CellPosition cellPosition, ref CellSelectionFlag selection) => 
					{
						if (clickPosition.x > cellPosition.x - 0.5f && clickPosition.x < cellPosition.x + 0.5f &&
						    clickPosition.y > cellPosition.y - 0.5f && clickPosition.y < cellPosition.y + 0.5f)
						{
							EntityManager.CreateEntity(new CellClickedNotification{entity = entity});
						} 
					});
				}
			}
		}
	}
}