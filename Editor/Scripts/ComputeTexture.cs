using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("Noise/Compute Texture")]
public class ComputeTexture : MonoBehaviour {
	//-------------------------------------------------------------------------------------------------------------------
	// Public Structs
	//-------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public struct IntVector3{ public int x,y,z; }

	//[System.Serializable]
	//public struct ComputeParameter<T>{ public string name; public T value; }
	//Couldn't get this to serialize with unity's inspector so I'm hardcoding 
	//it for now until I can figure out some solution
	[System.Serializable]
	public struct ComputeParameterFloat{ public string name; public float value; }

	[System.Serializable]
	public struct ComputeRWTexture{ 
		public string name;
		[HideInInspector] 
		public RenderTexture rt;
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
	public virtual void GenerateTexture(){
		if (availableKernels == null || selectedKernelIndex >= availableKernels.Length)
		{
			Debug.LogWarning("ComputeTexture: No valid kernel selected!");
			return;
		}
		
		string currentKernelName = availableKernels[selectedKernelIndex].name;
		IntVector3 currentThreads = availableKernels[selectedKernelIndex].threads;
		
		int kernel = computeShader.FindKernel(currentKernelName);
		computeShader.Dispatch(kernel, 
			squareResolution/currentThreads.x, 
			squareResolution/currentThreads.y, 1);
	}

    public virtual void CreateRenderTexture(){
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
	public void SetParameters(){
		/*Currently I have this hardcoded for float parameters,
		**however it very easily can be modified/extended.
		**If this becomes used more in the future, a full on
		**editor window could allow for more modularity.*/
		foreach(ComputeParameterFloat param in parameters)
			computeShader.SetFloat(param.name, param.value);
	}

	public void SetTexture(){
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
	// Shader Parameter Detection
	//-------------------------------------------------------------------------------------------------------------------
	
	public void DetectShaderParameters()
	{
		if (computeShader == null)
		{
			Debug.LogWarning("ComputeTexture: No compute shader assigned!");
			return;
		}

		var detectedParams = new List<ComputeParameterFloat>();
		var detectedTextures = new List<ComputeRWTexture>();
		var detectedKernels = new List<KernelInfo>();
		
		string shaderPath = AssetDatabase.GetAssetPath(computeShader);
		if (string.IsNullOrEmpty(shaderPath))
		{
			Debug.LogWarning("ComputeTexture: Could not find shader file path!");
			return;
		}

		string shaderContent = System.IO.File.ReadAllText(shaderPath);
		ParseShaderContent(shaderContent, detectedParams, detectedTextures, detectedKernels);
		
		// Update parameters array
		parameters = detectedParams.ToArray();
		
		// Update texture if we found any
		if (detectedTextures.Count > 0)
		{
			rwTexture = detectedTextures[0]; // Use first detected texture
		}
		
		// Update kernels array
		availableKernels = detectedKernels.ToArray();
		
		// Keep current selection if valid, otherwise select first kernel
		if (selectedKernelIndex >= availableKernels.Length)
		{
			selectedKernelIndex = 0;
		}
		
		// Update legacy kernelName for backward compatibility
		if (availableKernels.Length > 0)
		{
			kernelName = availableKernels[selectedKernelIndex].name;
			computeThreads = availableKernels[selectedKernelIndex].threads;
		}
		
		Debug.Log($"ComputeTexture: Detected {detectedParams.Count} parameters, {detectedTextures.Count} textures, {detectedKernels.Count} kernels");
	}

	private void ParseShaderContent(string content, List<ComputeParameterFloat> parameters, List<ComputeRWTexture> textures, List<KernelInfo> kernels)
	{
		string[] lines = content.Split('\n');
		var kernelNames = new List<string>();
		var kernelThreads = new Dictionary<string, IntVector3>();
		
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
								kernelThreads[funcName] = new IntVector3 { x = x, y = y, z = z };
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
						parameters.Add(new ComputeParameterFloat { name = paramName, value = 1.0f });
					}
				}
			}
			
			// Detect RWTexture2D
			else if (Regex.IsMatch(trimmedLine, @"^RWTexture2D<\w+>\s+\w+\s*;"))
			{
				Match match = Regex.Match(trimmedLine, @"RWTexture2D<\w+>\s+(\w+)\s*;");
				if (match.Success)
				{
					string textureName = match.Groups[1].Value;
					textures.Add(new ComputeRWTexture { name = textureName });
				}
			}
			
			// Detect RWTexture3D
			else if (Regex.IsMatch(trimmedLine, @"^RWTexture3D<\w+>\s+\w+\s*;"))
			{
				Match match = Regex.Match(trimmedLine, @"RWTexture3D<\w+>\s+(\w+)\s*;");
				if (match.Success)
				{
					string textureName = match.Groups[1].Value;
					textures.Add(new ComputeRWTexture { name = textureName });
				}
			}
		}
		
		// Combine kernel names with their thread counts
		foreach (string kernelName in kernelNames)
		{
			IntVector3 threads = kernelThreads.ContainsKey(kernelName) ? 
				kernelThreads[kernelName] : 
				new IntVector3 { x = 1, y = 1, z = 1 }; // Default values
			
			kernels.Add(new KernelInfo { name = kernelName, threads = threads });
		}
	}

	private bool IsBuiltInVariable(string varName)
	{
		// List of common built-in variables to skip
		string[] builtInVars = { "unity_ObjectToWorld", "unity_WorldToObject", "_Time", "_SinTime", "_CosTime", "_DeltaTime", "_ScreenParams" };
		foreach (string builtIn in builtInVars)
		{
			if (varName.Equals(builtIn, System.StringComparison.OrdinalIgnoreCase))
				return true;
		}
		return false;
	}

	//-------------------------------------------------------------------------------------------------------------------
	// Save/Utility Functions
	//-------------------------------------------------------------------------------------------------------------------
	protected Texture2D ConvertFromRenderTexture(RenderTexture rt){
		Texture2D output = new Texture2D(squareResolution, squareResolution);
		RenderTexture.active = rt;
		output.ReadPixels(new Rect(0,0,squareResolution, squareResolution), 0, 0);
		output.Apply();
		return output;
	}
	
	public virtual void SaveAsset(){
		Texture2D output = ConvertFromRenderTexture(rwTexture.rt);
		AssetDatabase.CreateAsset(output, "Assets/Noise/" + assetName + ".asset");
	}
}
