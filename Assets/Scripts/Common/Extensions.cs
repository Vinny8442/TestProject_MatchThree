using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public static class Extensions
	{
		public static T[] ToArray<T>(this DynamicBuffer<T> buffer) where T : struct
		{
			T[] result = new T[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
			{
				result[i] = buffer[i];
			}

			return result;
		}

		public static Entity CreateEntity<T>(this EntityManager em, T component) where T : struct, IComponentData
		{
			Entity result = em.CreateEntity(typeof(T));
			em.SetComponentData(result, component);
			return result;
		}

		public static T GetRandom<T>(this T[] source)
		{
			return source[Random.Range(0, source.Length)];
		}
	}
}