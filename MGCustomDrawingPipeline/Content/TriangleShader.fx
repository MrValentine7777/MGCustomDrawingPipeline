#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

//========================================================================
// BEGINNER'S GUIDE TO SHADER PROGRAMMING
//========================================================================
// Shaders are small programs that run directly on the graphics card (GPU).
// They control how 3D objects are rendered to the screen.
//
// This shader file contains two main programs:
// 1. A vertex shader (MainVS) - Transforms vertex positions and prepares data
// 2. A pixel shader (MainPS) - Determines the color of each pixel

//========================================================================
// SHADER PARAMETERS
//========================================================================
// This matrix combines world, view and projection transforms
// It's used to convert 3D positions to 2D screen coordinates
matrix WorldViewProjection;

//========================================================================
// DATA STRUCTURES
//========================================================================
// Input structure for the vertex shader
// These values come from the C# code for each vertex
struct VertexShaderInput
{
    float4 Position : POSITION0;  // Position in 3D space (x,y,z,w)
    float4 Color : COLOR0;        // Color of the vertex (r,g,b,a)
};

// Output structure from vertex shader to pixel shader
// This data is interpolated (blended) across the triangle
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;  // Screen position (required output)
    float4 Color : COLOR0;          // Color to pass to pixel shader
};

//========================================================================
// VERTEX SHADER
//========================================================================
// The vertex shader runs once for each vertex of our triangle
// It transforms positions from 3D world space to 2D screen space
VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Transform vertex position using the combined matrix
    // This converts the vertex from 3D model space to 2D screen space
    output.Position = mul(input.Position, WorldViewProjection);
    
    // Pass the vertex color directly to the pixel shader
    // This will allow colors to be blended across the triangle
    output.Color = input.Color;
    
    return output;
}

//========================================================================
// PIXEL SHADER
//========================================================================
// The pixel shader runs once for each pixel that will be drawn
// It determines the final color that appears on screen
float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Simply return the interpolated color
    // This creates a smooth gradient between the vertex colors
    return input.Color;
}

//========================================================================
// TECHNIQUES
//========================================================================
// A technique defines how to render an object
// It combines vertex and pixel shaders into passes
technique BasicColorDrawing
{
    // First rendering pass
    pass Pass1
    {
        // Specify which shaders to use
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
    
    // Second pass (not used in this example, but could be used
    // for multi-pass effects like outlines, glow, etc.)
    pass Pass2
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
