using UnityEngine;

[AddComponentMenu("Noise/Compute Texture 3D")]
public class ComputeTexture3D : ComputeTexture
{
    public ComputeShader texture3DSlicer;

    //-------------------------------------------------------------------------------------------------------------------
    // Generator Functions
    //-------------------------------------------------------------------------------------------------------------------
    public override void GenerateTexture()
    {
        int kernel = computeShader.FindKernel(kernelName);
        computeShader.Dispatch(kernel,
            squareResolution / computeThreads.x,
            squareResolution / computeThreads.y,
            squareResolution / computeThreads.z);
    }

    public override void CreateRenderTexture()
    {
        //3D RenderTexture with depth 0 for Unity 2022.3+ compatibility
        RenderTexture rt = new RenderTexture(squareResolution, squareResolution, 0, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        rt.volumeDepth = squareResolution;
        rt.Create();
        rwTexture.rt = rt;
    }

    //-------------------------------------------------------------------------------------------------------------------
    // Save/Utility Functions
    //-------------------------------------------------------------------------------------------------------------------
    protected RenderTexture Copy3DSliceToRenderTexture(int layer)
    {
        RenderTexture render = new RenderTexture(squareResolution, squareResolution, 0, RenderTextureFormat.ARGB32);
        render.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        render.enableRandomWrite = true;
        render.wrapMode = TextureWrapMode.Clamp;
        render.Create();

        int kernelIndex = texture3DSlicer.FindKernel("CSMain");
        texture3DSlicer.SetTexture(kernelIndex, "noise", rwTexture.rt);
        texture3DSlicer.SetInt("layer", layer);
        texture3DSlicer.SetTexture(kernelIndex, "Result", render);
        texture3DSlicer.Dispatch(kernelIndex, squareResolution, squareResolution, 1);

        return render;
    }

    public override void SaveAsset()
    {
        Debug.LogWarning(
            "ComputeTexture3D: SaveAsset() is only available in Editor. Please use the Editor version for saving assets.");
    }
}
