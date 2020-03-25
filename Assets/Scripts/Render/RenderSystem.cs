using System.Collections.Generic;
using Notifications;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class RenderSystem : ComponentSystem
	{
		private GameObject _cellContentPrefab;
		private GameObject _cellBackPrefab;
		private Dictionary<Entity, CellContentController> _cellViews = new Dictionary<Entity, CellContentController>();
		private GameStateHelper _gameHelper;

		public void Init(GameObject cellBackPrefab, GameObject cellContentPrefab)
		{
			_cellBackPrefab = cellBackPrefab;
			_cellContentPrefab = cellContentPrefab;
		}

		protected override void OnStartRunning()
		{
			_gameHelper = new GameStateHelper(EntityManager, Entities);

			for (int i = 0; i < _gameHelper.GetSize().Width; i++)
			{
				for (int j = 0; j < _gameHelper.GetSize().Height; j++)
				{
					Transform cellBack = GameObject.Instantiate(_cellBackPrefab).transform;
					cellBack.position = new Vector3(i * 1.0f,j * 1.0f);
				}
			}
			
		}

		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<CellDestroyNotification>().ForEach((Entity entity, ref CellDestroyNotification notification) =>
			{
				if (_cellViews.TryGetValue(notification.Entity, out var cellView))
				{
					if (cellView != null)
					{
						GameObject.Destroy(cellView.gameObject);
					}

					_cellViews.Remove(notification.Entity);
				}
			});
			
			Entities.WithAllReadOnly<CellWorldPosition>().ForEach((ref CellWorldPosition position) =>
			{
				
				if (!_cellViews.TryGetValue(position.Cell, out var cellView))
				{
					cellView = GameObject.Instantiate(_cellContentPrefab).GetComponent<CellContentController>();
					_cellViews.Add(position.Cell, cellView);
					cellView.SetContent(EntityManager.GetComponentData<CellContent>(position.Cell));
					cellView.SetSelected(false);
				}
				cellView.transform.position = new Vector3(position.x * 1.0f, position.y * 1.0f);
			});
			
			Entities.WithAllReadOnly<CellUpdatedNotification>().ForEach((Entity entity, ref CellUpdatedNotification notification) =>
			{
				var cellEntity = notification.Entity;
				if (_cellViews.TryGetValue(cellEntity, out var cellView))
				{
					cellView.SetSelected(EntityManager.GetComponentData<CellSelectionFlag>(cellEntity).Value);
					// var position = EntityManager.GetComponentData<CellWorldPosition>(cellEntity);
					// cellView.transform.position = new Vector3(position.x * 1.0f, position.y * 1.0f);
				}
				PostUpdateCommands.DestroyEntity(entity);
			});
			
			
		}

	}
}