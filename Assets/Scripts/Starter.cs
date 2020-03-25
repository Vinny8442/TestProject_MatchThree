using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class Starter : MonoBehaviour
{

    public GameObject CellBack;
    public GameObject CellContent;
    
    // Start is called before the first frame update
    void Start()
    {
        int sizeX = 10;
        int sizeY = 10;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        TMPArchetypeLibrary.Init(entityManager);

        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RenderSystem>().Init(CellBack, CellContent);

        NativeArray<Entity> cells = new NativeArray<Entity>(sizeX * sizeY, Allocator.Temp);
        World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity(TMPArchetypeLibrary.CellsArchetype, cells);
        int index = 0;
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                entityManager.SetComponentData(cells[index], new CellContent
                {
                    type = CellType.Simple,
                    Color = GameSettings.Instance.Colors[Random.Range(0, GameSettings.Instance.Colors.Length - 1)]
                });
                entityManager.SetComponentData(cells[index], new CellPosition
                {
                    x = i,
                    y = j,
                });
                index++;
            }
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
