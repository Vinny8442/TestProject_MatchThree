﻿using System;
 using System.Collections.Generic;
 using DefaultNamespace;
 using DefaultNamespace.Game;
 using Notifications;
 using UnityEngine;
 using Random = UnityEngine.Random;

 namespace Unity.Entities
{
	public struct CollapseSystemMarker : IComponentData
	{
		public Entity Cell;
	}
	
	public class CellCollapseSystem : ComponentSystemWithExtras
	{
		private static float CollapseTimeout = 0.2f;

		private List<CellPosition> _gravityChangerPlannedPositions = new List<CellPosition>();

		protected override void OnUpdate()
		{
			float timeout = CollapseTimeout;
			Dictionary<Entity, bool> processedCells = null;
			
			ProcessGroup<TimerComponent, CollapseSystemMarker> timerProcess = null;
			Entities.WithAll<CollapseCellsRequest>().ForEach((Entity entity) =>
			{
				if (processedCells == null)
				{
					processedCells = new Dictionary<Entity, bool>();
				}

				if (timerProcess == null)
				{
					timerProcess = HoldProcess(new ProcessGroup<TimerComponent, CollapseSystemMarker>(EntityManager, Entities));
					timerProcess.OnCompleted += OnCollapseAnimationComplete;
				}
				var cellsToCollapse = EntityManager.GetBuffer<CellToCollapse>(entity).ToArray();
				if (cellsToCollapse.Length >= 4)
				{
					_gravityChangerPlannedPositions.Add(EntityManager.GetComponentData<CellPosition>(cellsToCollapse[Random.Range(0, cellsToCollapse.Length)].Value));
				}

				bool invertGravity = false;
				for (int i = 0; i < cellsToCollapse.Length; i++)
				{
					var cellToCollapse = cellsToCollapse[i];
					if (processedCells.ContainsKey(cellToCollapse.Value)) continue;
					timerProcess.Add(new TimerComponent(timeout), new CollapseSystemMarker{Cell = cellToCollapse.Value});
					timerProcess.OnItemCompleted += OnCellCollapsed;
					timeout += CollapseTimeout;
					processedCells[cellToCollapse.Value] = true;

					if (EntityManager.GetComponentData<CellContent>(cellToCollapse.Value).type == CellType.Special)
					{
						invertGravity = true;
					} 
				}

				if (invertGravity)
				{
					EntityManager.CreateEntity(new GravityInvert());
				}
				EntityManager.DestroyEntity(entity);
			});
			
			UpdateProcesses();
		}

		private void OnCellCollapsed(Entity entity, ref TimerComponent timer, ref CollapseSystemMarker marker)
		{
			EntityManager.CreateEntity(new CellDestroyNotification{Entity = marker.Cell});
		}

		private void OnCollapseAnimationComplete(ProcessGroup<TimerComponent, CollapseSystemMarker> group)
		{
			if (_gravityChangerPlannedPositions.Count > 0)
			{
				HoldPromise(GenerateGravityChangers())
					.Then(RequestGravity)
					.Then(RequestRefill)
					.Then(RequestGravity);
			}
			else
			{
				HoldPromise(RequestGravity())
					.Then(RequestRefill)
					.Then(RequestGravity);
			}
		}

		private IProcess GenerateGravityChangers()
		{
			var process = new ProcessGroup<GenerateGravityChangerRequest, CollapseSystemMarker>(EntityManager, Entities);
			foreach (var cellPosition in _gravityChangerPlannedPositions)
			{
				process.Add(new GenerateGravityChangerRequest {Position = cellPosition}, new CollapseSystemMarker());
			}
			_gravityChangerPlannedPositions.Clear();
			return process;
		}
		
		private IProcess RequestRefill()
		{
			Debug.Log($"-- Refill Request {GameStateHelper.GetCounter(Entities)}");
			return new Process<GenerateLineRequest, CollapseSystemMarker>(EntityManager, Entities, new GenerateLineRequest(), new CollapseSystemMarker());
		}

		private IProcess RequestGravity()
		{
			Debug.Log($"-- Gravity Request {GameStateHelper.GetCounter(Entities)}");
			
			var process = new ProcessGroup<GravityRequest, CollapseSystemMarker>(EntityManager, Entities);
			process.Add(new GravityRequest(), new CollapseSystemMarker());

			return process;
		}
	}
}