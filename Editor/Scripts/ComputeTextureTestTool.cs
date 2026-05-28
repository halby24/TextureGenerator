using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ComputeTextureTestTool : EditorWindow
{
    [MenuItem("Tools/Test Compute Texture Parameter Detection")]
    public static void ShowWindow()
    {
        GetWindow<ComputeTextureTestTool>("Compute Texture Test");
    }

    private ComputeShader testShader;
    private string detectionResults = "";

    private void OnGUI()
    {
        GUILayout.Label("Compute Texture Parameter Detection Test", EditorStyles.boldLabel);
        
        testShader = (ComputeShader)EditorGUILayout.ObjectField("Test Shader", testShader, typeof(ComputeShader), false);
        
        if (GUILayout.Button("Test Parameter Detection"))
        {
            if (testShader != null)
            {
                TestParameterDetection();
            }
            else
            {
                detectionResults = "No shader selected!";
            }
        }
        
        if (!string.IsNullOrEmpty(detectionResults))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Detection Results:");
            EditorGUILayout.TextArea(detectionResults, GUILayout.Height(200));
        }
    }

    private void TestParameterDetection()
    {
        var detectedParams = new List<ComputeTexture.ComputeParameter>();
        var detectedTextures = new List<ComputeTexture.ComputeRWTexture>();
        var detectedKernels = new List<string>();
        
        string shaderPath = AssetDatabase.GetAssetPath(testShader);
        if (string.IsNullOrEmpty(shaderPath))
        {
            detectionResults = "Could not find shader file path!";
            return;
        }

        string shaderContent = System.IO.File.ReadAllText(shaderPath);
        
        // Create a temporary ComputeTexture to test the parsing
        var tempObject = new GameObject("TempComputeTexture");
        var computeTexture = tempObject.AddComponent<ComputeTexture>();
        computeTexture.computeShader = testShader;
        
        // Test the detection
        computeTexture.DetectShaderParameters();
        
        // Build results string
        var results = new System.Text.StringBuilder();
        results.AppendLine($"Shader: {testShader.name}");
        results.AppendLine($"Path: {shaderPath}");
        results.AppendLine();
        
        results.AppendLine($"Detected Parameters ({computeTexture.parameters?.Length ?? 0}):");
        if (computeTexture.parameters != null)
        {
            foreach (var param in computeTexture.parameters)
            {
                results.AppendLine($"  - {param.name}: {param.value}");
            }
        }
        
        results.AppendLine();
        results.AppendLine($"Detected Texture: {computeTexture.rwTexture.name}");
        results.AppendLine($"Detected Kernel: {computeTexture.kernelName}");
        
        detectionResults = results.ToString();
        
        // Clean up
        DestroyImmediate(tempObject);
    }
}