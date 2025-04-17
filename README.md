# MGCustomDrawingPipeline

## Overview
MGCustomDrawingPipeline is an educational MonoGame project designed to help developers understand the fundamentals of custom drawing pipelines and shader programming. This project provides a clear, well-commented implementation of a basic 3D rendering system using MonoGame's low-level graphics APIs, leveraging the HiDef graphics profile for enhanced visual quality.

> **Update:** This project has been upgraded to use MonoGame 3.8.3 with .NET 9, providing improved performance, better compatibility with modern systems, and access to the latest framework features.

## Purpose
This project was created to:
- Demonstrate how to implement custom drawing code in MonoGame
- Provide an accessible introduction to shader programming concepts
- Show the connection between C# code and GPU operations
- Serve as a learning resource for graphics programming beginners
- Illustrate the benefits of using HiDef graphics profile over Reach
- Showcase an organized, component-based architecture for rendering pipelines

## What You'll Learn
- Creating and managing vertex and index buffers
- Understanding 3D transformations (world, view, projection matrices)
- Writing and using HLSL shaders with DirectX shader model 5.0
- Building a custom rendering pipeline from scratch
- How vertex and pixel shaders work together in real-time graphics
- Configuring HiDef graphics settings for improved visual quality
- Implementing advanced post-processing techniques like bloom effects
- Creating color-specific visual effects using shader technology
- Structuring rendering code using dedicated component classes

## Project Features
- A rotating 3D tree model with texture-based coloring (trunk and foliage)
- Custom HLSL shader with detailed comments explaining each section
- Transformation matrices for proper 3D positioning and animation
- Comprehensive code comments designed for educational purposes
- HiDef graphics profile with multi-sampling for anti-aliasing
- High-precision 24-bit depth buffer with 8-bit stencil
- DirectX 11 shader model 5.0 for maximum graphical fidelity
- Color-targeted bloom post-processing effect that makes the blue background glow
- Multi-pass rendering pipeline utilizing render targets
- Component-based architecture with dedicated rendering classes

## Getting Started

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or other compatible IDE
- MonoGame 3.8.3 framework
- Graphics hardware supporting DirectX 11 or higher (for shader model 5.0)

### Running the Project
1. Clone the repository
2. Open the solution file in Visual Studio
3. Build and run the project (F5)
4. Press 'P' key to toggle the bloom post-processing effect

### Troubleshooting
If you see a black screen after pressing 'P' to enable post-processing:
- Ensure all content (shader files) has been properly built and is included in the output directory
- Check that your graphics card supports the shader features being used
- Try increasing `BloomIntensity` in GameState.cs if the effect is too subtle to see
- Some systems may require a restart of the application after the first attempt at using post-processing
- If you modified the shader code, ensure all required parameters are properly set in DrawSceneToRenderTarget()

## Project Structure

### Key Components
- **Game1.cs**: Main game class that initializes the game components and orchestrates the rendering
- **GameState.cs**: Contains all state variables and configuration for the game
- **TreeRenderer.cs**: Handles the specific rendering of the 3D tree model
- **PostProcessingRenderer.cs**: Implements the multi-pass bloom post-processing
- **TreeModelGenerator.cs**: Creates the vertex and index buffers for the tree model
- **RenderTargetManager.cs**: Manages the render targets used in post-processing
- **ColorTextureCreator.cs**: Handles creation of 1x1 textures for model coloring
- **InputManager.cs**: Processes user input for controlling the application
- **ModelAnimator.cs**: Controls the animation of the tree model
- **TriangleShader.fx**: HLSL shader file that defines vertex and pixel shaders using shader model 5.0
- **BloomShader.fx**: HLSL shader file that implements the multi-pass bloom post-processing effect

### How It Works
The application demonstrates a complete rendering pipeline:
1. **Scene Rendering**: The 3D tree model is rendered using vertex and index buffers with 1x1 color textures
2. **Post-Processing Extraction**: The blue color from the background is extracted using a color-targeted bloom shader
3. **Gaussian Blur**: A two-pass (horizontal and vertical) Gaussian blur is applied to the extracted colors
4. **Final Composition**: The original scene and the blurred glow effect are combined for the final image

## Understanding the Post-Processing Pipeline

The project implements a multi-stage bloom effect specifically targeting the cornflower blue color of the background:

1. **Color Extraction**: The BlueBloomExtract shader pass extracts pixels similar to the cornflower blue color
2. **Horizontal Blur**: The first blur pass applies a Gaussian blur in the horizontal direction
3. **Vertical Blur**: The second blur pass applies a Gaussian blur in the vertical direction
4. **Combination**: The bloom effect is combined with the original image to create the final glowing effect

Each stage uses dedicated render targets to store intermediate results before final composition.

### Post-Processing Parameters
You can adjust the bloom effect by modifying these parameters in GameState.cs:
- **BloomIntensity**: Controls the brightness of the bloom effect (default: 1.5f)
- **BloomThreshold**: Determines minimum brightness for bloom to occur (default: 0.2f)
- **BloomBlurAmount**: Controls the spread of the blur effect (default: 4.0f)
- **ColorSensitivity**: Adjusts how selective the blue color targeting is (default: 0.35f)
- **TargetBlueColor**: The specific color to be targeted by the bloom effect (default: CornflowerBlue)

## Graphics Profile Details
The project uses MonoGame's HiDef graphics profile which provides:
- Support for high-end shader models (vs_5_0/ps_5_0) in DirectX 11
- Hardware anti-aliasing via multi-sampling
- Higher precision depth buffer (24-bit with 8-bit stencil)
- Support for more complex shader effects and larger textures
- Better visual quality at the cost of requiring more capable hardware
- Access to advanced DirectX 11 features like compute shaders and tessellation

## Component-Based Architecture
The project demonstrates a well-structured approach to game development:
- **Separation of Concerns**: Each class has a specific responsibility
- **Modularity**: Components like TreeRenderer and PostProcessingRenderer encapsulate specific functionality
- **State Management**: The GameState class centralizes all game state variables
- **Utility Classes**: Dedicated classes for tasks like texture creation and input handling
- **Organized Namespaces**: Code is organized into logical categories (Rendering, Animation, Input, etc.)

## Further Learning
To continue exploring graphics programming concepts:
- Experiment with modifying the bloom parameters for different effects
- Try targeting different colors for the bloom effect (the shader includes options for both blue and green targeting)
- Modify the tree model to add more complexity or detail
- Implement additional post-processing effects (HDR, tone mapping, etc.)
- Explore different blur algorithms beyond Gaussian blur
- Experiment with more complex 3D models and scenes
- Implement advanced shader techniques like normal mapping or shadow mapping

## Contributing
Contributions are welcome! If you have ideas for improving this educational resource or want to add more examples, please feel free to submit a pull request.