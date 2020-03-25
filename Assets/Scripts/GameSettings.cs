using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class GameSettings
	{
		private static GameSettings _instance;
		public static GameSettings Instance => _instance ?? (_instance = new GameSettings()); 
		public int Width;
		public int Height;
		public Color[] Colors;

		public GameSettings()
		{
			Width = 10;
			Height = 10;
			Colors = new[] {Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow};
		}
	}
}