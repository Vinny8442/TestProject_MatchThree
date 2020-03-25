using System.Collections.Generic;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Matching
{
	public class CellsMap
	{
		private Dictionary<string, Entity> _entities = new Dictionary<string, Entity>();
		private Dictionary<string, CellContent> _contents = new Dictionary<string, CellContent>();
		private EntityManager _em;

		public CellsMap(EntityManager em)
		{
			_em = em;
			Update();
		}

		public void Update()
		{
			_entities.Clear();
			_contents.Clear();
			EntityQuery query = _em.CreateEntityQuery(typeof(CellPosition), typeof(CellContent));
			var entities = query.ToEntityArray(Allocator.TempJob);
			var contents = query.ToComponentDataArray<CellContent>(Allocator.TempJob);
			var positions = query.ToComponentDataArray<CellPosition>(Allocator.TempJob);
			for (int i = 0; i < positions.Length; i++)
			{
				CellPosition position = positions[i];
				CellContent content = contents[i];
				string key = GetKey(position.x, position.y);
				_entities[key] = entities[i];
				_contents[key] = contents[i];
			}

			entities.Dispose();
			contents.Dispose();
			positions.Dispose();
		}

		public bool GetColor(int x, int y, out Color result)
		{
			string key = GetKey(x, y);
			if (_contents.TryGetValue(key, out var content))
			{
				result = content.Color;
				return true;
			}
			result = Color.black;
			return false;
		}

		public Color GetColor(int x, int y)
		{
			GetColor(x, y, out var result);
			return result;
		}

		public void Set(CellPosition position, Entity cell, CellContent content)
		{
			string key = GetKey(position.x, position.y);
			_entities[key] = cell;
			_contents[key] = content;
		}

		public bool GetCell(int x, int y, out Entity result)
		{
			string key = GetKey(x, y);
			if (_entities.TryGetValue(key, out var entity))
			{
				result = entity;
				return true;
			}
			result = Entity.Null;
			return false;
		}

		public Entity GetCell(int x, int y)
		{
			if (GetCell(x, y, out var result))
			{
				return result;
			}
			return Entity.Null;
		}

		private string GetKey(int x, int y)
		{
			return $"{x}:{y}";
		}
	}
}