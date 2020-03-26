using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[CreateAssetMenu(fileName = "GameSettings", menuName = "__TMP/GameSettings", order = 0)]
	public class GameSettings : ScriptableObject
	{
		public int Width;
		public int Height;
		public Color[] Colors;

	}
}