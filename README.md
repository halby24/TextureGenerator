# Noise & Texture Generator for Unity

[![Unity Version](https://img.shields.io/badge/Unity-2019.4%2B-blue.svg)](https://unity3d.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![GitHub Release](https://img.shields.io/github/release/mtwoodard/NoiseGenerator.svg)](https://github.com/mtwoodard/NoiseGenerator/releases)

3D and 2D Texture generation using compute shaders within the Unity engine. This package provides a comprehensive solution for procedural texture generation with Unity Package Manager support.

![Image of 3D Noise](https://raw.githubusercontent.com/mtwoodard/NoiseGenerator/master/noiseGenerator.png)

## 📦 Installation

### Unity Package Manager (Recommended)
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button and select `Add package from git URL`
3. Enter: `https://github.com/mtwoodard/NoiseGenerator.git`
4. Click `Add`

### Manual Installation
1. Download the latest release from [GitHub Releases](https://github.com/mtwoodard/NoiseGenerator/releases)
2. Extract to your project's `Packages` folder

## 🎯 Purpose

This package handles the creation and serialization of different 3D/2D textures created via custom compute shaders through a custom **ComputeTexture** system. It's designed specifically for:

- **Raymarching Systems**: Generate 3D textures for volumetric rendering
- **Procedural Content**: Create noise textures for terrain, clouds, and effects
- **Performance**: Leverage GPU compute shaders for fast texture generation

## 🚀 Features

- **Unity Package Manager Support**: Easy installation and updates
- **Compute Shader Based**: High-performance GPU texture generation
- **3D & 2D Texture Support**: Generate both 2D and 3D textures
- **Editor Tools**: User-friendly inspector tools for texture generation
- **Sample Content**: Ready-to-use examples and presets
- **Assembly Definitions**: Proper code organization and compilation

## 📁 Package Structure

```
├── Runtime/                    # Runtime scripts and assets
│   ├── Materials/             # Material assets
│   ├── Prefabs/              # Prefab assets
│   ├── Shaders/              # Compute shaders
│   └── Unity.NoiseTextureGenerator.asmdef
├── Editor/                    # Editor-only scripts
│   ├── Scripts/              # Editor scripts and tools
│   └── Unity.NoiseTextureGenerator.Editor.asmdef
├── Samples~/                 # Sample content (optional import)
│   └── Examples/            # Example prefabs and assets
└── package.json             # Package manifest
```

## 🛠️ Usage

### Core Components

The package provides three main components:

1. **ComputeTexture** - Base class for 2D texture generation
2. **ComputeTexture3D** - Specialized for 3D texture generation  
3. **NoiseGenerator** - High-level noise generation system

### Basic Setup

1. **Import Samples**: In Package Manager, expand the package and import "Noise Generator Examples"
2. **Create Generator**: Add a `NoiseGenerator` component to a GameObject
3. **Configure Settings**:
   - **Asset Name**: Name for the generated texture asset
   - **Kernel Name**: Compute shader kernel to use
   - **RW Texture**: Name of the RWTexture in the compute shader
   - **Square Resolution**: Texture dimensions (e.g., 128 = 128³ for 3D)
   - **Parameters**: Float values passed to the compute shader
   - **Compute Threads**: Thread group sizes from the compute shader
   - **Compute Shader**: Your custom compute shader

### Available Compute Shaders

- **CurlNoise3D**: Generates 3D curl noise for fluid simulation
- **WhiteNoise**: Basic white noise generation
- **DebugNoise**: Debug visualization shader
- **Texture3DSlicer**: Converts 3D textures to 2D slices

### Editor Tools

- **ComputeTextureEditor**: Enhanced inspector for texture generation
- **ComputeTextureTestTool**: Testing utilities for compute shaders
- **Automatic Parameter Detection**: Automatically detects shader parameters

## 📋 Requirements

- Unity 2019.4 or newer
- Compute Shader support
- Graphics API with compute shader support (DirectX 11+, OpenGL 4.3+, Vulkan, Metal)

## 🎨 Examples

Check the `Samples~/Examples` folder for:
- Pre-configured noise generators
- Example compute shaders
- Material setups for different use cases

## 🔧 Advanced Usage

### Creating Custom Compute Shaders

```hlsl
#pragma kernel GenerateNoise

RWTexture3D<float4> NoiseTexture;

[numthreads(8,8,8)]
void GenerateNoise(uint3 id : SV_DispatchThreadID)
{
    float3 pos = float3(id) / 128.0;
    float noise = your_noise_function(pos);
    NoiseTexture[id] = float4(noise, noise, noise, 1.0);
}
```

### Extending ComputeTexture

```csharp
public class MyCustomTexture : ComputeTexture3D
{
    protected override void SetShaderParameters()
    {
        base.SetShaderParameters();
        // Add your custom parameters here
    }
}
```

## 📚 References

- [Guerrilla Games - Volumetric Clouds][clouds]: Inspiration for 3D texture use cases
- [greje656][greje]: Compute shader noise functions
- [Nesvi][nesvi]: 3D render texture save functions

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **greje656** for the wonderful compute shader noise functions
- **Nesvi** for the 3D render texture save functions
- **Guerrilla Games** for the volumetric cloud system inspiration

---

*Originally created for generating 3D textures for use in raymarching systems and volumetric cloud systems. The package has evolved to support a wide range of procedural texture generation needs.*

[clouds]: http://advances.realtimerendering.com/s2015/The%20Real-time%20Volumetric%20Cloudscapes%20of%20Horizon%20-%20Zero%20Dawn%20-%20ARTR.pdf
[greje]: https://bitsquid.blogspot.com/2016/07/volumetric-clouds.html
[nesvi]: http://answers.unity.com/answers/1243556/view.html
