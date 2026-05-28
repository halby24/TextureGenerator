using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public static class ComputeTextureEditorUtility
{
    //-------------------------------------------------------------------------------------------------------------------
    // Editor-only Asset Save Functions
    //-------------------------------------------------------------------------------------------------------------------
    public static void SaveAsset(ComputeTexture computeTexture)
    {
        if (computeTexture.rwTexture.rt == null)
        {
            Debug.LogWarning("ComputeTexture: No render texture to save!");
            return;
        }

        Texture2D output = ConvertFromRenderTexture(computeTexture.rwTexture.rt, computeTexture.squareResolution);
        
        // Handle different assetName formats
        string assetPath;
        if (computeTexture.assetName.StartsWith("Assets/"))
        {
            // Full path already provided (from file dialog)
            assetPath = computeTexture.assetName + ".asset";
        }
        else
        {
            // Legacy format: just filename, use default path
            assetPath = "Assets/Noise/" + computeTexture.assetName + ".asset";
        }
        
        AssetDatabase.CreateAsset(output, assetPath);
    }

    public static void SaveAsset(ComputeTexture3D computeTexture3D)
    {
        if (computeTexture3D.rwTexture.rt == null)
        {
            Debug.LogWarning("ComputeTexture3D: No render texture to save!");
            return;
        }

        //for readability
        int dim = computeTexture3D.squareResolution;
        //Slice 3D Render Texture to individual layers
        RenderTexture[] layers = new RenderTexture[dim];
        for (int i = 0; i < dim; i++)
            layers[i] = Copy3DSliceToRenderTexture(computeTexture3D, i);
        //Write RenderTexture slices to static textures
        Texture2D[] finalSlices = new Texture2D[dim];
        for (int i = 0; i < dim; i++)
            finalSlices[i] = ConvertFromRenderTexture(layers[i], dim);
        //Build 3D Texture from 2D slices
        Texture3D output = new Texture3D(dim, dim, dim, TextureFormat.ARGB32, true);
        output.filterMode = FilterMode.Trilinear;
        Color[] outputPixels = output.GetPixels();
        for (int k = 0; k < dim; k++)
        {
            Color[] layerPixels = finalSlices[k].GetPixels();
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    outputPixels[i + j * dim + k * dim * dim] = layerPixels[i + j * dim];
                }
            }
        }

        output.SetPixels(outputPixels);
        output.Apply();

        // Handle different assetName formats
        string assetPath;
        if (computeTexture3D.assetName.StartsWith("Assets/"))
        {
            // Full path already provided (from file dialog)
            assetPath = computeTexture3D.assetName + ".asset";
        }
        else
        {
            // Legacy format: just filename, use default path
            assetPath = "Assets/Noise/" + computeTexture3D.assetName + ".asset";
        }
        
        AssetDatabase.CreateAsset(output, assetPath);
    }

    //-------------------------------------------------------------------------------------------------------------------
    // Shader Parameter Detection
    //-------------------------------------------------------------------------------------------------------------------
    public static void DetectShaderParameters(ComputeTexture computeTexture)
    {
        if (computeTexture.computeShader == null)
        {
            Debug.LogWarning("ComputeTexture: No compute shader assigned!");
            return;
        }

        var detectedParams = new List<ComputeTexture.ComputeParameter>();
        var detectedKernels = new List<ComputeTexture.KernelInfo>();

        string shaderPath = AssetDatabase.GetAssetPath(computeTexture.computeShader);
        if (string.IsNullOrEmpty(shaderPath))
        {
            Debug.LogWarning("ComputeTexture: Could not find shader file path!");
            return;
        }

        string shaderContent = System.IO.File.ReadAllText(shaderPath);
        ParseShaderContent(shaderContent, detectedParams, detectedKernels);

        // Update parameters array
        computeTexture.parameters = detectedParams.ToArray();


        // Update kernels array
        computeTexture.availableKernels = detectedKernels.ToArray();

        // Keep current selection if valid, otherwise select first kernel
        if (computeTexture.selectedKernelIndex >= computeTexture.availableKernels.Length)
        {
            computeTexture.selectedKernelIndex = 0;
        }

        // Update legacy kernelName for backward compatibility
        if (computeTexture.availableKernels.Length > 0)
        {
            computeTexture.kernelName = computeTexture.availableKernels[computeTexture.selectedKernelIndex].name;
            computeTexture.computeThreads = computeTexture.availableKernels[computeTexture.selectedKernelIndex].threads;
        }

        Debug.Log(
            $"ComputeTexture: Detected {detectedParams.Count} parameters, {detectedTextures.Count} textures, {detectedKernels.Count} kernels");
    }

    //-------------------------------------------------------------------------------------------------------------------
    // Editor-only Texture Generation Functions
    //-------------------------------------------------------------------------------------------------------------------
    public static void CreateRenderTexture(ComputeTexture computeTexture)
    {
        if (computeTexture.rwTexture.rt != null)
        {
            computeTexture.rwTexture.rt.Release();
        }

        RenderTexture rt = new RenderTexture(computeTexture.squareResolution, computeTexture.squareResolution, 0, RenderTextureFormat.ARGB32);
        rt.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        rt.enableRandomWrite = true;
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.Create();

        var rwTexture = computeTexture.rwTexture;
        rwTexture.rt = rt;
        computeTexture.rwTexture = rwTexture;
        
        Debug.Log($"ComputeTexture: Created render texture {computeTexture.squareResolution}x{computeTexture.squareResolution}");
    }

    public static void SetParameters(ComputeTexture computeTexture)
    {
        if (computeTexture.computeShader == null)
        {
            Debug.LogWarning("ComputeTexture: No compute shader assigned!");
            return;
        }

        if (computeTexture.parameters == null) return;

        int kernelIndex = computeTexture.computeShader.FindKernel(computeTexture.kernelName);
        
        foreach (var param in computeTexture.parameters)
        {
            computeTexture.computeShader.SetFloat(param.name, param.value);
        }
        
        Debug.Log($"ComputeTexture: Set {computeTexture.parameters.Length} parameters");
    }

    public static void SetTexture(ComputeTexture computeTexture)
    {
        if (computeTexture.computeShader == null || computeTexture.rwTexture.rt == null)
        {
            Debug.LogWarning("ComputeTexture: Missing compute shader or render texture!");
            return;
        }

        int kernelIndex = computeTexture.computeShader.FindKernel(computeTexture.kernelName);
        computeTexture.computeShader.SetTexture(kernelIndex, computeTexture.rwTexture.name, computeTexture.rwTexture.rt);
        
        Debug.Log($"ComputeTexture: Set texture '{computeTexture.rwTexture.name}' for kernel '{computeTexture.kernelName}'");
    }

    public static void GenerateTexture(ComputeTexture computeTexture)
    {
        if (computeTexture.computeShader == null || computeTexture.rwTexture.rt == null)
        {
            Debug.LogWarning("ComputeTexture: Missing compute shader or render texture!");
            return;
        }

        int kernelIndex = computeTexture.computeShader.FindKernel(computeTexture.kernelName);
        
        // Calculate dispatch groups
        int groupsX = Mathf.CeilToInt(computeTexture.squareResolution / (float)computeTexture.computeThreads.x);
        int groupsY = Mathf.CeilToInt(computeTexture.squareResolution / (float)computeTexture.computeThreads.y);
        int groupsZ = Mathf.CeilToInt(1 / (float)computeTexture.computeThreads.z);
        
        computeTexture.computeShader.Dispatch(kernelIndex, groupsX, groupsY, groupsZ);
        
        Debug.Log($"ComputeTexture: Generated texture using kernel '{computeTexture.kernelName}' with dispatch({groupsX}, {groupsY}, {groupsZ})");
    }

    //-------------------------------------------------------------------------------------------------------------------
    // Utility Functions
    //-------------------------------------------------------------------------------------------------------------------
    private static Texture2D ConvertFromRenderTexture(RenderTexture rt, int resolution)
    {
        Texture2D output = new Texture2D(resolution, resolution);
        RenderTexture.active = rt;
        output.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        output.Apply();
        return output;
    }

    private static RenderTexture Copy3DSliceToRenderTexture(ComputeTexture3D computeTexture3D, int layer)
    {
        RenderTexture render = new RenderTexture(computeTexture3D.squareResolution, computeTexture3D.squareResolution,
            0, RenderTextureFormat.ARGB32);
        render.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        render.enableRandomWrite = true;
        render.wrapMode = TextureWrapMode.Clamp;
        render.Create();

        int kernelIndex = computeTexture3D.texture3DSlicer.FindKernel("CSMain");
        computeTexture3D.texture3DSlicer.SetTexture(kernelIndex, "noise", computeTexture3D.rwTexture.rt);
        computeTexture3D.texture3DSlicer.SetInt("layer", layer);
        computeTexture3D.texture3DSlicer.SetTexture(kernelIndex, "Result", render);
        computeTexture3D.texture3DSlicer.Dispatch(kernelIndex, computeTexture3D.squareResolution,
            computeTexture3D.squareResolution, 1);

        return render;
    }

    private static void ParseShaderContent(string content, List<ComputeTexture.ComputeParameter> parameters, List<ComputeTexture.KernelInfo> kernels)
    {
        string[] lines = content.Split('\n');
        var kernelNames = new List<string>();
        var kernelThreads = new Dictionary<string, int3>();

        for (int i = 0; i < lines.Length; i++)
        {
            string trimmedLine = lines[i].Trim();

            // Detect kernel definitions
            if (trimmedLine.StartsWith("#pragma kernel"))
            {
                string kernelName = trimmedLine.Replace("#pragma kernel", "").Trim();
                if (!string.IsNullOrEmpty(kernelName))
                {
                    kernelNames.Add(kernelName);
                }
            }

            // Detect numthreads attribute
            else if (Regex.IsMatch(trimmedLine, @"^\[numthreads\("))
            {
                Match match = Regex.Match(trimmedLine, @"\[numthreads\((\d+),\s*(\d+),\s*(\d+)\)\]");
                if (match.Success)
                {
                    int x = int.Parse(match.Groups[1].Value);
                    int y = int.Parse(match.Groups[2].Value);
                    int z = int.Parse(match.Groups[3].Value);

                    // Find the next function definition to get the kernel name
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        string nextLine = lines[j].Trim();
                        if (nextLine.StartsWith("void "))
                        {
                            Match funcMatch = Regex.Match(nextLine, @"void\s+(\w+)\s*\(");
                            if (funcMatch.Success)
                            {
                                string funcName = funcMatch.Groups[1].Value;
                                kernelThreads[funcName] = new ComputeTexture.IntVector3 { x = x, y = y, z = z };
                                break;
                            }
                        }
                    }
                }
            }

            // Detect float parameters (uniform variables)
            else if (Regex.IsMatch(trimmedLine, @"^float\s+\w+\s*;"))
            {
                Match match = Regex.Match(trimmedLine, @"float\s+(\w+)\s*;");
                if (match.Success)
                {
                    string paramName = match.Groups[1].Value;
                    // Skip common built-in variables
                    if (!IsBuiltInVariable(paramName))
                    {
                        parameters.Add(new ComputeTexture.ComputeParameter { name = paramName, value = 1.0f });
                    }
                }
            }
        }

        // Combine kernel names with their thread counts
        foreach (string kernelName in kernelNames)
        {
            ComputeTexture.int3 threads = kernelThreads.ContainsKey(kernelName)
                ? kernelThreads[kernelName]
                : new ComputeTexture.IntVector3 { x = 1, y = 1, z = 1 }; // Default values

            kernels.Add(new ComputeTexture.KernelInfo { name = kernelName, threads = threads });
        }
    }

    private static bool IsBuiltInVariable(string varName)
    {
        // List of common built-in variables to skip
        string[] builtInVars =
        {
            "unity_ObjectToWorld", "unity_WorldToObject", "_Time", "_SinTime", "_CosTime", "_DeltaTime", "_ScreenParams"
        };
        foreach (string builtIn in builtInVars)
        {
            if (varName.Equals(builtIn, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
