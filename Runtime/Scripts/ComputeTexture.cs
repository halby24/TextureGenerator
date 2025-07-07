using UnityEngine;

[AddComponentMenu("Noise/Compute Texture")]
public class ComputeTexture : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------------------------
    // Public Structs
    //-------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public struct IntVector3
    {
        public int x, y, z;
    }

    [System.Serializable]
    public struct ComputeParameterFloat
    {
        public string name;
        public float value;
    }

    [System.Serializable]
    public struct ComputeRWTexture
    {
        public string name;
        [HideInInspector] public RenderTexture rt;
    }

    //-------------------------------------------------------------------------------------------------------------------
    // Public Variables
    //-------------------------------------------------------------------------------------------------------------------
    public string assetName;
    public string kernelName;
    public ComputeRWTexture rwTexture;
    public int squareResolution;
    public ComputeParameterFloat[] parameters;
    public IntVector3 computeThreads;
    public ComputeShader computeShader;

    //-------------------------------------------------------------------------------------------------------------------
    // Generator Functions
    //-------------------------------------------------------------------------------------------------------------------
    public virtual void GenerateTexture()
    {
        if (availableKernels == null || selectedKernelIndex >= availableKernels.Length)
        {
            Debug.LogWarning("ComputeTexture: No valid kernel selected!");
            return;
        }

        string currentKernelName = availableKernels[selectedKernelIndex].name;
        IntVector3 currentThreads = availableKernels[selectedKernelIndex].threads;

        int kernel = computeShader.FindKernel(currentKernelName);
        computeShader.Dispatch(kernel,
            squareResolution / currentThreads.x,
            squareResolution / currentThreads.y, 1);
    }

    public virtual void CreateRenderTexture()
    {
        RenderTexture rt = new RenderTexture(squareResolution, squareResolution, 24, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();
        rwTexture.rt = rt;
    }

    //-------------------------------------------------------------------------------------------------------------------
    // Kernel Information
    //-------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public struct KernelInfo
    {
        public string name;
        public IntVector3 threads;
    }

    public KernelInfo[] availableKernels;
    public int selectedKernelIndex;

    //-------------------------------------------------------------------------------------------------------------------
    // Compute Shader Getters/Setters
    //-------------------------------------------------------------------------------------------------------------------
    public void SetParameters()
    {
        /*Currently I have this hardcoded for float parameters,
         **however it very easily can be modified/extended.
         **If this becomes used more in the future, a full on
         **editor window could allow for more modularity.*/
        foreach (ComputeParameterFloat param in parameters)
            computeShader.SetFloat(param.name, param.value);
    }

    public void SetTexture()
    {
        if (availableKernels == null || selectedKernelIndex >= availableKernels.Length)
        {
            Debug.LogWarning("ComputeTexture: No valid kernel selected!");
            return;
        }

        string currentKernelName = availableKernels[selectedKernelIndex].name;
        int kernel = computeShader.FindKernel(currentKernelName);
        computeShader.SetTexture(kernel, rwTexture.name, rwTexture.rt);
    }

    //-------------------------------------------------------------------------------------------------------------------
    // Save/Utility Functions
    //-------------------------------------------------------------------------------------------------------------------
    protected Texture2D ConvertFromRenderTexture(RenderTexture rt)
    {
        Texture2D output = new Texture2D(squareResolution, squareResolution);
        RenderTexture.active = rt;
        output.ReadPixels(new Rect(0, 0, squareResolution, squareResolution), 0, 0);
        output.Apply();
        return output;
    }

    public virtual void SaveAsset()
    {
        Debug.LogWarning(
            "ComputeTexture: SaveAsset() is only available in Editor. Please use the Editor version for saving assets.");
    }

    public void DetectShaderParameters()
    {
        Debug.LogWarning("ComputeTexture: DetectShaderParameters() is only available in Editor.");
    }
}
