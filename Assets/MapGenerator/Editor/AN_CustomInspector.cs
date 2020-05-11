
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AN_SimpleCityGenerator))]
public class AN_CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // DrawDefaultInspector();

        AN_SimpleCityGenerator gen = (AN_SimpleCityGenerator)target;
        if (GUILayout.Button("Generate City")) gen.Generate();
    }
}
