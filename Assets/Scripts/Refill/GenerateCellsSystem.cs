using System;
using System.Collections.Generic;
using DefaultNamespace.Game;
using Matching;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
	internal struct GenerateCellData : IComponentData
	{
		public int x;
		public int y;
		public Color Color;
	}

	public struct GenerateLineRequest : IComponentData, IProcessEntity
	{
		public RequestStatus Status;
		public bool Completed => Status == RequestStatus.Completed;
	}
	
	internal struct GenerateGravityChangerRequest : IComponentData, IProcessEntity
	{
		public CellPosition Position;
		public CellContent Content;
		public bool Completed { get; internal set; }
	}

	public struct GenerateAllCellsRequest : IComponentData, IProcessEntity
	{
		public RequestStatus Status;
		public bool Completed { get; internal set; }
	}

	public class GenerateCellsSystem : ComponentSystemWithExtras
	{
		private static float GenerateTimeout = 0.05f;
		
		private GameStateHelper _helper;
		private EntityArchetype _cellsArchetype;

		protected override void OnStartRunning()
		{
			_cellsArchetype = EntityManager.CreateArchetype(
				typeof(CellContent), 
				typeof(CellPosition), 
				typeof(CellSelectionFlag), 
				typeof(WorldPositionLink));
			
			_helper = new GameStateHelper(EntityManager, Entities);
		}

		protected override void OnUpdate()
		{
			UpdateProcesses();
			
			GenerateGravityChangers();
			
			GenerateAll();
			GenerateTopLine();
		}

		private void GenerateAll()
		{
			Entities.ForEach((Entity entity, ref GenerateAllCellsRequest request) =>
			{
				if (request.Status != RequestStatus.New) return;
				
				GameFieldSize size = _helper.GetSize();
				Color[] colors = _helper.GetColors();
				CellsMap map = new CellsMap(EntityManager);

				for (int i = 0; i < size.Width; i++)
				{
					for (int j = 0; j < size.Height; j++)
					{
						AddCellAt(i,j,colors.GetRandom());
					}
				}

				request.Status = RequestStatus.Completed;
			});
			
		}

		private void GenerateTopLine()
		{
			bool active = false;
			Entity  refillEntity = Entity.Null;
			Entities.ForEach((Entity entity, ref GenerateLineRequest request) =>
			{
				if (request.Status == RequestStatus.New)
				{
					refillEntity = entity;
					active = true;
					request.Status = RequestStatus.Processing;
				}
			});

			if (!active)
			{
				return;
			}

			var process = new ProcessGroup<TimerComponent, GenerateCellData>(EntityManager, Entities);
			GameFieldSize fieldSize = _helper.GetSize();
			Color[] colors = _helper.GetColors();
			CellsMap map = new CellsMap(EntityManager);
			float delay = 0;
			int y = _helper.Gravity == GravityDirection.Down ? fieldSize.Height - 1 : 0;
			for (int i = 0; i < fieldSize.Width; i++)
			{
				if (!map.GetCell(i, y, out var cell))
				{
					delay += GenerateTimeout;
					process.Add(new TimerComponent(delay), new GenerateCellData{x = i, y = y, Color = colors.GetRandom()});
				}
			}

			process.OnItemCompleted += (Entity entity, ref TimerComponent completable, ref GenerateCellData marker) =>
			{
				AddCellAt(marker.x, marker.y, marker.Color);
			};
			process.OnCompleted += group =>
			{
				EntityManager.SetComponentData(refillEntity, new GenerateLineRequest{Status = RequestStatus.Completed});
				DestroyEntityAfterFrame(refillEntity, 2);
			};
			HoldProcess(process);
		}

		private void GenerateGravityChangers()
		{
			Entities.ForEach((Entity entity, ref GenerateGravityChangerRequest gcRequest) =>
			{
				if (!gcRequest.Completed)
				{
					AddCellAt(gcRequest.Position.x, gcRequest.Position.y, gcRequest.Content.Color, CellType.Special);
					gcRequest.Completed = true;
				}
				else
				{
					PostUpdateCommands.DestroyEntity(entity);
				}
			});
		}

		private void AddCellAt(int x, int y, Color color, CellType type = CellType.Simple)
		{
			Entity entity = EntityManager.CreateEntity(_cellsArchetype);
			EntityManager.SetComponentData(entity, new CellPosition{x = x, y = y});
			EntityManager.SetComponentData(entity, new CellContent
			{
				type = type,
				Color = color
			});
		}
	}
	
}