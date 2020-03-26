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
    
    void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RenderSystem>().Init(CellBack, CellContent);
        entityManager.CreateEntity(new GenerateAllCellsRequest());
    }

}
