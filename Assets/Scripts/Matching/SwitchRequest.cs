using Unity.Entities;

namespace Matching
{
	public struct SwitchRequest : IComponentData
	{
		public Entity cell1;
		public Entity cell2;
	}
}