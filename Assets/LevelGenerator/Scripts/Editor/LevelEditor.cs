using UnityEditor;
using UnityEngine;

namespace LevelGenerator.Scripts.Editor
{
    [CustomEditor(typeof(LevelGenerator))]
    public class GeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelGenerator myScript = (LevelGenerator)target;
            if (GUILayout.Button("Add Section Template"))
            {
                myScript.AddSectionTemplate();
            }

            if (GUILayout.Button("Add Dead End Template"))
            {
                myScript.AddDeadEndTemplate();
            }
        }
    }
}
