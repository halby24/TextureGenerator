# ComputeTexture Shader Parameter Auto-Detection

## 概要
ComputeTexture.csに自動的にコンピュートシェーダーのパラメータを検出する機能を追加しました！✨

## 新機能

### 1. 自動パラメータ検出
シェーダーをアサインすると、以下の要素が自動的に検出されます：

- **フロートパラメータ**: `float variableName;` 形式の変数
- **テクスチャ**: `RWTexture2D<float4> textureName;` および `RWTexture3D<float4> textureName;` 形式
- **カーネル**: `#pragma kernel KernelName` 形式の定義

### 2. 改良されたInspectorUI
- シェーダーアサイン時に自動的にパラメータを検出
- 「🔍 Detect Shader Parameters」ボタンで手動検出も可能
- 検出されたパラメータは折りたたみ可能なセクションで表示
- パラメータ名は自動設定（読み取り専用）、値は編集可能

### 3. テストツール
`Tools > Test Compute Texture Parameter Detection` から、シェーダーの検出機能をテストできます。

## 使用方法

### 基本的な使い方
1. ComputeTextureコンポーネントを追加
2. Compute Shaderフィールドにシェーダーをアサイン
3. 自動的にパラメータが検出されます
4. 必要に応じて検出されたパラメータの値を調整

### 対応するシェーダー形式
```hlsl
// カーネル定義
#pragma kernel MyKernel

// フロートパラメータ（自動検出）
float MyParameter;
float AnotherValue;

// テクスチャ（自動検出）
RWTexture2D<float4> OutputTexture;
RWTexture3D<float4> VolumeTexture;

[numthreads(8,8,1)]
void MyKernel(uint3 id : SV_DispatchThreadID)
{
    // シェーダーのコード
}
```

### 検出される例
ExampleNoise.computeを例に取ると：
- **Parameters**: `Tex3D1Res`, `Tex3D2Res`, `Tex2DRes`
- **Textures**: `Noise2D`, `Noise3D1`, `Noise3D2`
- **Kernels**: `Noise2DGen`, `Noise3D1Gen`, `Noise3D2Gen`

## 技術的詳細

### 実装されたメソッド
- `DetectShaderParameters()`: メインの検出メソッド
- `ParseShaderContent()`: シェーダーファイルの解析
- `IsBuiltInVariable()`: Unity組み込み変数の除外

### 検出パターン
- Float変数: `^float\s+\w+\s*;`
- RWTexture2D: `^RWTexture2D<\w+>\s+\w+\s*;`
- RWTexture3D: `^RWTexture3D<\w+>\s+\w+\s*;`
- Kernel: `#pragma kernel`

## 利点
- ✅ 手動でパラメータを設定する必要がない
- ✅ シェーダーの変更に自動的に対応
- ✅ タイプミスやパラメータ名の間違いを防げる
- ✅ 開発効率が大幅に向上

## 注意事項
- 組み込み変数（`_Time`、`_ScreenParams`等）は自動的に除外されます
- 複数のテクスチャが検出された場合、最初のものが自動選択されます
- 複数のカーネルが検出された場合、最初のものが自動選択されます

## 今後の拡張案
- int、Vector等の他の型のサポート
- Texture2D、Texture3D等の入力テクスチャのサポート
- カーネルの複数選択UI
- パラメータのデフォルト値の推定