using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComputeTexture), true)]
public class ComputeTextureEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ComputeTexture computeTexture = (ComputeTexture)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Detect Shader Parameters"))
        {
            ComputeTextureEditorUtility.DetectShaderParameters(computeTexture);
            EditorUtility.SetDirty(computeTexture);
        }

        if (GUILayout.Button("Save Asset"))
        {
            if (computeTexture is ComputeTexture3D computeTexture3D)
            {
                ComputeTextureEditorUtility.SaveAsset(computeTexture3D);
            }
            else
            {
                ComputeTextureEditorUtility.SaveAsset(computeTexture);
            }
        }
    }
}
