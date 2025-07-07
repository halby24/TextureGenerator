# Unity用ノイズ & テクスチャジェネレータ

[![Unity Version](https://img.shields.io/badge/Unity-2019.4%2B-blue.svg)](https://unity3d.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![GitHub Release](https://img.shields.io/github/release/mtwoodard/NoiseGenerator.svg)](https://github.com/mtwoodard/NoiseGenerator/releases)

Unityエンジン内でコンピュートシェーダーを使用した3D・2Dテクスチャ生成パッケージです。Unity Package Managerに対応した、プロシージャルテクスチャ生成のための包括的なソリューションを提供します。

![3Dノイズの画像](https://raw.githubusercontent.com/mtwoodard/NoiseGenerator/master/noiseGenerator.png)

## 📦 インストール

### Unity Package Manager（推奨）
1. Unity Package Managerを開く（`Window > Package Manager`）
2. `+`ボタンをクリックして`Add package from git URL`を選択
3. 以下のURLを入力: `https://github.com/mtwoodard/NoiseGenerator.git`
4. `Add`をクリック

### 手動インストール
1. [GitHub Releases](https://github.com/mtwoodard/NoiseGenerator/releases)から最新版をダウンロード
2. プロジェクトの`Packages`フォルダに展開

## 🎯 目的

このパッケージは、カスタム**ComputeTexture**システムを通じて、カスタムコンピュートシェーダーで作成された様々な3D・2Dテクスチャの生成と直列化を処理します。特に以下の用途に設計されています：

- **レイマーチングシステム**: ボリュメトリックレンダリング用の3Dテクスチャ生成
- **プロシージャルコンテンツ**: 地形、雲、エフェクト用のノイズテクスチャ作成
- **パフォーマンス**: GPU コンピュートシェーダーを活用した高速テクスチャ生成

## 🚀 機能

- **Unity Package Manager対応**: 簡単なインストールとアップデート
- **コンピュートシェーダーベース**: 高性能なGPUテクスチャ生成
- **3D・2Dテクスチャ対応**: 2Dと3Dの両方のテクスチャ生成
- **エディターツール**: テクスチャ生成のためのユーザーフレンドリーなインスペクターツール
- **サンプルコンテンツ**: すぐに使える例とプリセット
- **アセンブリ定義**: 適切なコード整理とコンパイル

## 📁 パッケージ構造

```
├── Runtime/                    # ランタイムスクリプトとアセット
│   ├── Materials/             # マテリアルアセット
│   ├── Prefabs/              # プレハブアセット
│   ├── Shaders/              # コンピュートシェーダー
│   └── Unity.NoiseTextureGenerator.asmdef
├── Editor/                    # エディター専用スクリプト
│   ├── Scripts/              # エディタースクリプトとツール
│   └── Unity.NoiseTextureGenerator.Editor.asmdef
├── Samples~/                 # サンプルコンテンツ（オプションインポート）
│   └── Examples/            # サンプルプレハブとアセット
└── package.json             # パッケージマニフェスト
```

## 🛠️ 使い方

### コアコンポーネント

パッケージは3つのメインコンポーネントを提供します：

1. **ComputeTexture** - 2Dテクスチャ生成のベースクラス
2. **ComputeTexture3D** - 3Dテクスチャ生成に特化
3. **NoiseGenerator** - 高レベルなノイズ生成システム

### 基本セットアップ

1. **サンプルをインポート**: Package Managerでパッケージを展開し、"Noise Generator Examples"をインポート
2. **ジェネレータを作成**: GameObjectに`NoiseGenerator`コンポーネントを追加
3. **設定を構成**:
   - **Asset Name**: 生成されるテクスチャアセットの名前
   - **Kernel Name**: 使用するコンピュートシェーダーのカーネル
   - **RW Texture**: コンピュートシェーダー内のRWTextureの名前
   - **Square Resolution**: テクスチャの次元（例：128 = 3Dの場合128³）
   - **Parameters**: コンピュートシェーダーに渡すfloat値
   - **Compute Threads**: コンピュートシェーダーからのスレッドグループサイズ
   - **Compute Shader**: カスタムコンピュートシェーダー

### 利用可能なコンピュートシェーダー

- **CurlNoise3D**: 流体シミュレーション用の3Dカールノイズ生成
- **WhiteNoise**: 基本的なホワイトノイズ生成
- **DebugNoise**: デバッグ可視化シェーダー
- **Texture3DSlicer**: 3Dテクスチャを2Dスライスに変換

### エディターツール

- **ComputeTextureEditor**: テクスチャ生成のための拡張インスペクター
- **ComputeTextureTestTool**: コンピュートシェーダーのテストユーティリティ
- **自動パラメーター検出**: シェーダーパラメーターの自動検出

## 📋 要件

- Unity 2019.4以降
- コンピュートシェーダー対応
- コンピュートシェーダー対応のGraphics API（DirectX 11+、OpenGL 4.3+、Vulkan、Metal）

## 🎨 例

`Samples~/Examples`フォルダで以下を確認できます：
- 事前設定済みのノイズジェネレータ
- サンプルコンピュートシェーダー
- 様々な用途向けのマテリアル設定

## 🔧 高度な使用法

### カスタムコンピュートシェーダーの作成

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

### ComputeTextureの拡張

```csharp
public class MyCustomTexture : ComputeTexture3D
{
    protected override void SetShaderParameters()
    {
        base.SetShaderParameters();
        // カスタムパラメーターをここに追加
    }
}
```

## 📚 参考文献

- [Guerrilla Games - Volumetric Clouds][clouds]: 3Dテクスチャ使用例のインスピレーション
- [greje656][greje]: コンピュートシェーダーノイズ関数
- [Nesvi][nesvi]: 3Dレンダーテクスチャ保存関数

## 🤝 貢献

貢献歓迎！お気軽にissueやpull requestをお送りください。

## 📄 ライセンス

このプロジェクトはMITライセンスの下でライセンスされています - 詳細は[LICENSE](LICENSE)ファイルを参照してください。

## 🙏 謝辞

- **greje656** 素晴らしいコンピュートシェーダーノイズ関数の提供
- **Nesvi** 3Dレンダーテクスチャ保存関数の提供
- **Guerrilla Games** ボリュメトリッククラウドシステムのインスピレーション

---

*元々はレイマーチングシステムとボリュメトリッククラウドシステムで使用する3Dテクスチャ生成のために作成されました。パッケージは現在、幅広いプロシージャルテクスチャ生成のニーズに対応するよう進化しています。*

[clouds]: http://advances.realtimerendering.com/s2015/The%20Real-time%20Volumetric%20Cloudscapes%20of%20Horizon%20-%20Zero%20Dawn%20-%20ARTR.pdf
[greje]: https://bitsquid.blogspot.com/2016/07/volumetric-clouds.html
[nesvi]: http://answers.unity.com/answers/1243556/view.html