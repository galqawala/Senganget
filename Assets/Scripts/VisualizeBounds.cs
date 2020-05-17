using UnityEngine;

public class VisualizeBounds : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
       Renderer[] rends = GetComponentsInChildren<Renderer>();
       Bounds bounds = rends[0].bounds;
       foreach (Renderer rend in rends)
       {
         bounds = bounds.GrowBounds( rend.bounds );
       }
       Vector3 center = bounds.center;
 
       Gizmos.DrawCube( bounds.center, bounds.size );
 
       Gizmos.DrawSphere( bounds.center, 0.1f );
    }
}