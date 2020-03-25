using Unity.Entities;

namespace DefaultNamespace
{
	public static class TMPArchetypeLibrary
	{
		private static EntityManager _em;
		public static EntityArchetype CellsArchetype { get; private set; }


		public static void Init(EntityManager em)
		{
			_em = em;
			CellsArchetype = _em.CreateArchetype(
				typeof(CellContent), 
				typeof(CellPosition), 
				typeof(CellSelectionFlag), 
				typeof(WorldPositionLink));
		}
	}
}