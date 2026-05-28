using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ComputeTexture), true)]
public class ComputeTextureEditor : Editor
{
    private ComputeShader previousShader;
    private bool showParameters = true;
    
    public override void OnInspectorGUI()
    {
        var computeTexture = (ComputeTexture)target;
        var so = new SerializedObject(computeTexture);
        
        // Draw the default inspector first
        EditorGUI.BeginChangeCheck();
        
        // Custom shader field with auto-detection
        EditorGUILayout.LabelField("Compute Shader Settings", EditorStyles.boldLabel);
        
        var newShader = (ComputeShader)EditorGUILayout.ObjectField("Compute Shader", computeTexture.computeShader, typeof(ComputeShader), false);
        
        // Check if shader was changed
        if (newShader != previousShader)
        {
            computeTexture.computeShader = newShader;
            previousShader = newShader;
            
            // Auto-detect parameters when shader is assigned
            if (newShader != null)
            {
                ComputeTextureEditorUtility.DetectShaderParameters(computeTexture);
                EditorUtility.SetDirty(computeTexture);
            }
        }
        
        // Manual detection button
        if (computeTexture.computeShader != null)
        {
            if (GUILayout.Button("🔍 Detect Shader Parameters"))
            {
                ComputeTextureEditorUtility.DetectShaderParameters(computeTexture);
                EditorUtility.SetDirty(computeTexture);
            }
        }
        
        EditorGUILayout.Space();
        
        // Draw other properties
        EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
        
        // Asset Name with file dialog button
        EditorGUILayout.BeginHorizontal();
        computeTexture.assetName = EditorGUILayout.TextField("Asset Name", computeTexture.assetName);
        if (GUILayout.Button(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(30), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
        {
            var selectedPath = EditorUtility.SaveFilePanel("Save Texture Asset", "Assets", computeTexture.assetName, "asset");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // Convert absolute path to relative project path
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                
                // Remove the .asset extension for display
                if (selectedPath.EndsWith(".asset"))
                {
                    selectedPath = selectedPath.Substring(0, selectedPath.Length - 6);
                }
                
                computeTexture.assetName = selectedPath;
                EditorUtility.SetDirty(computeTexture);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Kernel selection dropdown
        if (computeTexture.availableKernels != null && computeTexture.availableKernels.Length > 0)
        {
            var kernelNames = new string[computeTexture.availableKernels.Length];
            for (var i = 0; i < computeTexture.availableKernels.Length; i++)
            {
                var kernel = computeTexture.availableKernels[i];
                kernelNames[i] = $"{kernel.name} ({kernel.threads.x}x{kernel.threads.y}x{kernel.threads.z})";
            }
            
            EditorGUI.BeginChangeCheck();
            var newKernelIndex = EditorGUILayout.Popup("Kernel", computeTexture.selectedKernelIndex, kernelNames);
            if (EditorGUI.EndChangeCheck())
            {
                computeTexture.selectedKernelIndex = newKernelIndex;
                
                // Update legacy fields for backward compatibility
                if (newKernelIndex >= 0 && newKernelIndex < computeTexture.availableKernels.Length)
                {
                    computeTexture.kernelName = computeTexture.availableKernels[newKernelIndex].name;
                    computeTexture.computeThreads = computeTexture.availableKernels[newKernelIndex].threads;
                }
                EditorUtility.SetDirty(computeTexture);
            }
        }
        else
        {
            // Fallback to text field if no kernels detected
            computeTexture.kernelName = EditorGUILayout.TextField("Kernel Name", computeTexture.kernelName);
        }
        
        computeTexture.squareResolution = EditorGUILayout.IntField("Square Resolution", computeTexture.squareResolution);
        
        // Compute threads (auto-detected from selected kernel)
        EditorGUILayout.LabelField("Compute Threads (Auto-detected)", EditorStyles.boldLabel);
        var threads = computeTexture.computeThreads;
        
        if (computeTexture.availableKernels != null && computeTexture.availableKernels.Length > 0 && 
            computeTexture.selectedKernelIndex >= 0 && computeTexture.selectedKernelIndex < computeTexture.availableKernels.Length)
        {
            // Show as read-only when auto-detected
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("X", threads.x);
            EditorGUILayout.IntField("Y", threads.y);
            EditorGUILayout.IntField("Z", threads.z);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.HelpBox("Thread counts are automatically detected from the selected kernel's [numthreads] attribute.", MessageType.Info);
        }
        else
        {
            // Allow manual input if no kernels detected
            threads.x = EditorGUILayout.IntField("X", threads.x);
            threads.y = EditorGUILayout.IntField("Y", threads.y);
            threads.z = EditorGUILayout.IntField("Z", threads.z);
            computeTexture.computeThreads = threads;
            
            EditorGUILayout.HelpBox("Manual thread count input (kernel auto-detection not available).", MessageType.Warning);
        }
        
        // Parameters section
        EditorGUILayout.Space();
        showParameters = EditorGUILayout.Foldout(showParameters, $"Parameters ({computeTexture.parameters?.Length ?? 0})", true);
        
        // if (showParameters && computeTexture.parameters != null)
        // {
        //     EditorGUI.indentLevel++;
        //
        //     for (var i = 0; i < computeTexture.parameters.Length; i++)
        //     {
        //         var param = computeTexture.parameters[i];
        //
        //         EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        //
        //         // Parameter name (read-only, detected from shader)
        //         EditorGUI.BeginDisabledGroup(true);
        //         EditorGUILayout.TextField("Parameter Name", param.name);
        //         EditorGUI.EndDisabledGroup();
        //
        //         // Parameter value (editable)
        //         param.value = EditorGUILayout.FloatField("Value", param.value);
        //
        //         EditorGUILayout.EndVertical();
        //
        //         computeTexture.parameters[i] = param;
        //     }
        //
        //     EditorGUI.indentLevel--;
        // }
        EditorGUILayout.PropertyField(so.FindProperty())
        
        EditorGUILayout.Space();
        
        // Generation buttons
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Generate Texture"))
        {
            if (computeTexture.computeShader != null)
            {
                ComputeTextureEditorUtility.SetParameters(computeTexture);
                ComputeTextureEditorUtility.SetTexture(computeTexture);
                ComputeTextureEditorUtility.GenerateTexture(computeTexture);
            }
            else
            {
                Debug.LogWarning("ComputeTexture: No compute shader assigned!");
            }
        }
        

        
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(computeTexture);
        }
    }
}
