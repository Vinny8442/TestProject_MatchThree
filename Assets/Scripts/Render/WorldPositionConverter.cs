using UnityEngine;

namespace DefaultNamespace
{
	public class WorldPositionConverter
	{
		private Vector2 _offset;

		public WorldPositionConverter(GameFieldSize fieldSize)
		{
			_offset = new Vector2(fieldSize.Width / 2, fieldSize.Height / 2);
		}

		public Vector2 WorldToLogic(Vector2 input)
		{
			return input + _offset;
		}

		public Vector2 LogicToWorld(Vector2 input)
		{
			return input - _offset;
		}
	}
}