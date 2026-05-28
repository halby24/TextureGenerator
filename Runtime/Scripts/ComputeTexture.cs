using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class ComputeTexture : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------------------------
    // Public Structs
    //-------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public struct ComputeParameter
    {
        public string name;
        public float4 value;
    }

    //-------------------------------------------------------------------------------------------------------------------
    // Public Variables
    //-------------------------------------------------------------------------------------------------------------------
    public string assetName;
    public string kernelName;
    public int squareResolution;
    public ComputeParameter[] parameters;
    public int3 computeThreads;
    public ComputeShader computeShader;

    //-------------------------------------------------------------------------------------------------------------------
    // Generator Functions
    //-------------------------------------------------------------------------------------------------------------------
    public void Generate()
    {
        var rt = new RenderTexture(squareResolution, squareResolution, 24, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true
        };
        rt.Create();

        foreach (var param in parameters)
            computeShader.SetVector(param.name, param.value);

        if (availableKernels == null || selectedKernelIndex >= availableKernels.Length)
        {
            Debug.LogWarning("ComputeTexture: No valid kernel selected!");
            return;
        }

        var currentKernelName = availableKernels[selectedKernelIndex].name;
        var kernel = computeShader.FindKernel(currentKernelName);
        var currentThreads = availableKernels[selectedKernelIndex].threads;
        computeShader.SetTexture(kernel, rt.name, rt);
        computeShader.Dispatch(kernel,
            squareResolution / currentThreads.x,
            squareResolution / currentThreads.y,
            squareResolution / currentThreads.z);
    }

    // Utility


    //-------------------------------------------------------------------------------------------------------------------
    // Kernel Information
    //-------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public struct KernelInfo
    {
        public string name;
        public int3 threads;
    }

    public KernelInfo[] availableKernels;
    public int selectedKernelIndex;
}
