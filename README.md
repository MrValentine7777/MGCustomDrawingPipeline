# MGCustomDrawingPipeline

## Overview
MGCustomDrawingPipeline is an educational MonoGame project designed to help developers understand the fundamentals of custom drawing pipelines and shader programming. This project provides a clear, well-commented implementation of a basic 3D rendering system using MonoGame's low-level graphics APIs, leveraging the HiDef graphics profile for enhanced visual quality.

## Purpose
This project was created to:
- Demonstrate how to implement custom drawing code in MonoGame
- Provide an accessible introduction to shader programming concepts
- Show the connection between C# code and GPU operations
- Serve as a learning resource for graphics programming beginners
- Illustrate the benefits of using HiDef graphics profile over Reach

## What You'll Learn
- Creating and managing vertex and index buffers
- Understanding 3D transformations (world, view, projection matrices)
- Writing and using HLSL shaders with DirectX shader model 5.0
- Building a custom rendering pipeline from scratch
- How vertex and pixel shaders work together in real-time graphics
- Configuring HiDef graphics settings for improved visual quality
- Implementing advanced post-processing techniques like bloom effects
- Creating color-specific visual effects using shader technology

## Project Features
- A rotating 3D tree model with vertex coloring
- Custom HLSL shader with detailed comments explaining each section
- Transformation matrices for proper 3D positioning and animation
- Comprehensive code comments designed for educational purposes
- HiDef graphics profile with multi-sampling for anti-aliasing
- High-precision 24-bit depth buffer with 8-bit stencil
- DirectX 11 shader model 5.0 for maximum graphical fidelity
- Color-targeted bloom post-processing effect that makes green foliage glow
- Multi-pass rendering pipeline utilizing render targets

## Getting Started

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or other compatible IDE
- MonoGame framework
- Graphics hardware supporting DirectX 11 or higher (for shader model 5.0)

### Running the Project
1. Clone the repository
2. Open the solution file in Visual Studio
3. Build and run the project (F5)
4. Press 'P' key to toggle the bloom post-processing effect

## Project Structure

### Key Components
- **Game1.cs**: Main game class that implements the custom drawing pipeline with HiDef settings
- **TriangleShader.fx**: HLSL shader file that defines vertex and pixel shaders using shader model 5.0
- **BloomShader.fx**: HLSL shader file that implements the multi-pass bloom post-processing effect
- **Program.cs**: Application entry point
- **Content.mgcb**: Content pipeline configuration with HiDef profile settings

### How It Works
The application demonstrates a complete rendering pipeline:
1. **Scene Rendering**: The 3D tree model is rendered using vertex and index buffers
2. **Post-Processing Extraction**: The green color from the tree's foliage is extracted using a color-targeted bloom shader
3. **Gaussian Blur**: A two-pass (horizontal and vertical) Gaussian blur is applied to the extracted colors
4. **Final Composition**: The original scene and the blurred glow effect are combined for the final image

## Understanding the Post-Processing Pipeline

The project implements a multi-stage bloom effect specifically targeting the green colors of the tree:

1. **Color Extraction**: The GreenBloomExtract shader pass extracts pixels similar to the forest green color
2. **Horizontal Blur**: The first blur pass applies a Gaussian blur in the horizontal direction
3. **Vertical Blur**: The second blur pass applies a Gaussian blur in the vertical direction
4. **Combination**: The bloom effect is combined with the original image to create the final glowing effect

Each stage uses dedicated render targets to store intermediate results before final composition.

## Graphics Profile Details
The project uses MonoGame's HiDef graphics profile which provides:
- Support for high-end shader models (vs_5_0/ps_5_0) in DirectX 11
- Hardware anti-aliasing via multi-sampling
- Higher precision depth buffer (24-bit with 8-bit stencil)
- Support for more complex shader effects and larger textures
- Better visual quality at the cost of requiring more capable hardware
- Access to advanced DirectX 11 features like compute shaders and tessellation

## Further Learning
To continue exploring graphics programming concepts:
- Experiment with modifying the bloom parameters for different effects
- Try targeting different colors for the bloom effect
- Implement additional post-processing effects (HDR, tone mapping, etc.)
- Explore different blur algorithms beyond Gaussian blur
- Experiment with more complex 3D models and scenes
- Implement advanced shader techniques like normal mapping or shadow mapping

## Contributing
Contributions are welcome! If you have ideas for improving this educational resource or want to add more examples, please feel free to submit a pull request.