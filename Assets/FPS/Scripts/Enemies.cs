using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Enemies : MonoBehaviour
{
    public GameObject[] enemyList;
    public NavMeshSurface navMesh;
    public GameObject mapMagicContainer;
    public GameObject[] loot;
    public GameObject[] healthDrops;

    void Update()
    {
        //Tag the terrain to be used as a navmesh source
        //When these tagged terrains overlap with the LocalNavMeshBuilder (child object of player) the navmesh is updated
        foreach (Transform tile in mapMagicContainer.transform) {
            var mainTerrain = tile.Find("Main Terrain");
            var navMeshSourceTags = mainTerrain.GetComponentsInChildren(typeof(NavMeshSourceTag));
            if (navMeshSourceTags.Length==0) {
                NavMeshSourceTag navMeshSourceTag = 
                    mainTerrain.gameObject.AddComponent(typeof(NavMeshSourceTag)) as NavMeshSourceTag;
            }
        }

        if (transform.childCount < 20 && Random.value < 0.1) {
            //The size (diameter) of LocalNavMeshBuilder has to be at least twice the spawnDistanceMax (radius) in each dimension
            float spawnDistanceMin = 40;
            float spawnDistanceMax = spawnDistanceMin*2;
            var enemyToSpawn = enemyList[Random.Range(0, enemyList.Length)];
            var playerPos = Camera.main.transform.position;
            var spawnPos = GetRandomPoint(playerPos, spawnDistanceMax);
            var spawnDistance = (spawnPos-playerPos).magnitude;
            
            if (spawnDistance>spawnDistanceMin && spawnDistance<Mathf.Infinity) {
                Instantiate(enemyToSpawn, spawnPos, Quaternion.identity, transform);
            }
        }
    }

    public static Vector3 SphericalToCartesian(float radius, float polar, float elevation) {
        var outCart = new Vector3();
        float a = radius * Mathf.Cos(elevation);
        outCart.x = a * Mathf.Cos(polar);
        outCart.y = radius * Mathf.Sin(elevation);
        outCart.z = a * Mathf.Sin(polar);
        return outCart;
    }

    // Get Random Point on a Navmesh surface
    public static Vector3 GetRandomPoint(Vector3 center, float maxDistance) {
        // Get Random Point inside Sphere which position is center, radius is maxDistance
        Vector3 randomPos = Random.insideUnitSphere * maxDistance + center;

        UnityEngine.AI.NavMeshHit hit; // NavMesh Sampling Info Container

        // from randomPos find a nearest point on NavMesh surface in range of maxDistance
        UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, maxDistance, UnityEngine.AI.NavMesh.AllAreas);

        return hit.position;
    }
}
