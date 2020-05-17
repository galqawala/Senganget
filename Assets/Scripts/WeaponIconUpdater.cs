using System.Collections.Generic;
using UnityEngine;

public class WeaponIconUpdater : MonoBehaviour
{
    public Sprite GetIcon(WeaponController weapon) {
        Camera iconCam = GameObject.Find("WeaponIconCamera").GetComponent<Camera>();
        var weaponObject = weapon.gameObject.transform.Find("GunRoot").GetChild(0);
        var rot = Quaternion.identity;
        var weaponInstance = Instantiate(weaponObject, Vector3.zero, rot);
        weaponInstance.transform.Rotate(new Vector3(0f, -90f, 0f), Space.World);
        weaponInstance.transform.localScale = new Vector3(10f, 10f, 10f);
        weaponInstance.gameObject.SetActive(true);
        int layer = LayerMask.NameToLayer("WeaponIcon");
        weaponInstance.gameObject.layer = layer;
        List<Transform> children = new List<Transform>();    
        Utilities.GetAllChildren(weaponInstance.transform, ref children);
        foreach (var child in children)
        {
            child.gameObject.SetActive(true);
            child.gameObject.layer = layer;
        }

        var originalIcon = weapon.weaponIcon;

        iconCam.cullingMask = 1 << layer;
        PositionCameraToCoverObject(weaponInstance.gameObject, iconCam);
        var iconTexture = RTImage(iconCam, 88, 56);
        weaponInstance.gameObject.SetActive(false);
        Destroy(weaponInstance.gameObject);
        return Sprite.Create(iconTexture, originalIcon.rect, originalIcon.pivot, originalIcon.pixelsPerUnit);
    }

    // Take a "screenshot" of a camera's Render Texture.
    Texture2D RTImage(Camera camera, int resWidth, int resHeight)
    {
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture renderTexture = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;//camera.targetTexture;
        camera.Render();
        Texture2D image = new Texture2D(resWidth, resHeight);
        image.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        image.Apply();
        return image;
    }

    void PositionCameraToCoverObject(GameObject gameObject, Camera camera) {
        // https://forum.unity.com/threads/fit-object-exactly-into-perspective-cameras-field-of-view-focus-the-object.496472/
        var bounds = getBounds(gameObject);
        float cameraDistance = 0.7f; // Constant factor
        Vector3 objectSizes = bounds.max - bounds.min;
        float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView); // Visible height 1 meter in front
        float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
        camera.transform.position = bounds.center - distance * camera.transform.forward;
    }

    Bounds getBounds(GameObject gameObject) {
        // https://forum.unity.com/threads/finding-center-of-a-group-of-objects.11607/

        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();

        Bounds bounds = rends[0].bounds;
        foreach (Renderer rend in rends)
        {
            bounds = bounds.GrowBounds( rend.bounds );
        }
        return bounds;
    }
}