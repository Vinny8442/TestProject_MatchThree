using System.Collections.Generic;
using Matching;
using Unity.Entities;

namespace DefaultNamespace
{
	public class SwitchSystem : ComponentSystem
	{
		private IMatchDetector[] _matchers = {new HorizontalMatcher(), new VerticalMatcher()};

		private EntityArchetype _updateNotificationArchetype;

		protected override void OnStartRunning()
		{
			_updateNotificationArchetype = EntityManager.CreateArchetype(typeof(CellUpdatedNotification));
		}

		protected override void OnUpdate()
		{
			Entities.ForEach((Entity entity, ref SwitchRequest switchRequest) =>
			{
				if (ReplacementPossible(switchRequest.cell1, switchRequest.cell2, out var matchResults))
				{
					// EntityManager.CreateEntity(_resetSelectionArchetype);
					EntityManager.CreateEntity(new ResetSelection());
					
					var position1 = EntityManager.GetComponentData<CellPosition>(switchRequest.cell1);
					var position2 = EntityManager.GetComponentData<CellPosition>(switchRequest.cell2);
					EntityManager.SetComponentData(switchRequest.cell1, position2);
					EntityManager.SetComponentData(switchRequest.cell2, position1);
					GenerateNotification(switchRequest.cell1);
					GenerateNotification(switchRequest.cell2);

					// var archetype = EntityManager.CreateArchetype(typeof(CollapseCells));
					foreach (var matchResult in matchResults)
					{
						Entity collapse = EntityManager.CreateEntity(typeof(CellToCollapse), typeof(CollapseCellsRequest));
						EntityManager.AddComponentData(collapse, new CollapseCellsRequest());
						var cellsToCollapse = EntityManager.AddBuffer<CellToCollapse>(collapse);
						foreach (Entity resultEntity in matchResult.Entities)
						{
							cellsToCollapse.Add(new CellToCollapse{Value = resultEntity});
						}
					}
				}
				EntityManager.DestroyEntity(entity);
			});
		}

		private void GenerateNotification(Entity entity)
		{
			EntityManager.CreateEntity(new CellUpdatedNotification{Entity = entity});
		}
		
		private bool ReplacementPossible(Entity cell1, Entity cell2, out List<MatchResult> result)
		{
			CellsMap map = new CellsMap(EntityManager);
			
			var cell1Position = EntityManager.GetComponentData<CellPosition>(cell1);
			var cell1Content = EntityManager.GetComponentData<CellContent>(cell1);
			var cell2Position = EntityManager.GetComponentData<CellPosition>(cell2);
			var cell2Content = EntityManager.GetComponentData<CellContent>(cell2);
			map.Set(cell1Position, cell2, cell2Content);
			map.Set(cell2Position, cell1, cell1Content);

			result = new List<MatchResult>();
			foreach (var matcher in _matchers)
			{
				var matchResult = matcher.Check(map, cell1Position);
				if (matchResult.Length >= 3)
				{
					result.Add(matchResult);
				}
				matchResult = matcher.Check(map, cell2Position);
				if (matchResult.Length >= 3)
				{
					result.Add(matchResult);
				}
			}

			return result.Count > 0;
		}

	}

	public struct CellToCollapse : IBufferElementData
	{
		public Entity Value;
	}

	public struct CollapseCellsRequest : IComponentData
	{
		public RequestStatus Status;
	}
}