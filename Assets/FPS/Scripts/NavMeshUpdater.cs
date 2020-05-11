using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshUpdater : MonoBehaviour
{
    public NavMeshSurface navMesh;

    // Start is called before the first frame update
    void Start()
    {
        navMesh.BuildNavMesh();
    }
}
