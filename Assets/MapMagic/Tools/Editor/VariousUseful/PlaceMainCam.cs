using UnityEngine;
using UnityEditor;

public class PlaceMainCam
{
	[MenuItem ("Edit/PlaceMainCam")]
	static void PlaceMainCamFn ()
	{
		Camera.main.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
		Camera.main.transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
	}
}

public class SelectSceneCam
{
	[MenuItem ("Edit/SelectSceneCam")]
	static void SelectSceneCamFn ()
	{
		Selection.activeGameObject = SceneView.lastActiveSceneView.camera.gameObject;
		SceneView.lastActiveSceneView.camera.gameObject.hideFlags = HideFlags.None;
	}
}
