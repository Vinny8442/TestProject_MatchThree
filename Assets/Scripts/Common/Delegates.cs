using Unity.Entities;

namespace DefaultNamespace
{
	public delegate void CompletableCallback<Entity, TComponent, TMarker>(Entity entity, ref TComponent completable,
		ref TMarker marker)
		where TMarker : struct, IComponentData
		where TComponent : struct, IComponentData, ICompletable;
}