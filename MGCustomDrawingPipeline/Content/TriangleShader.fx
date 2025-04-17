#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
// Using shader model 5.0 for desktop DirectX
// This enables advanced shader features available in DirectX 11
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

//========================================================================
// BEGINNER'S GUIDE TO SHADER PROGRAMMING
//========================================================================
// Shaders are small programs that run directly on the graphics card (GPU).
// They control how 3D objects are rendered to the screen.
//
// This shader file uses DirectX shader model 5.0 (high-definition profile)
// which provides the most advanced shader capabilities available in DirectX 11:
// - Improved performance on modern hardware
// - Support for compute shaders and tessellation
// - More texture slots and larger shader programs
// - Better precision and more shader instructions
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
matrix World;

// Texture used for coloring the model
texture ModelTexture;
sampler2D ModelTextureSampler = sampler_state
{
    Texture = <ModelTexture>;
    MinFilter = Point;  // Use point filtering for 1x1 textures
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

// Lighting parameters
float3 LightDirection;      // Direction of the main light
float3 SunlightDirection;   // Direction of the sunlight
float3 AmbientLight;        // Ambient light color and intensity
float3 DiffuseLight;        // Diffuse light color and intensity
float3 SunlightColor;       // Sunlight color
float SunlightIntensity;    // Sunlight intensity
float3 SpecularLight;       // Specular light color and intensity
float SpecularPower;        // Specular power (sharpness of highlights)
float3 CameraPosition;      // Position of the camera for specular calculations

//========================================================================
// DATA STRUCTURES
//========================================================================
// Input structure for the vertex shader
// These values come from the C# code for each vertex
struct VertexShaderInput
{
    float4 Position : POSITION0;        // Position in 3D space (x,y,z,w)
    float3 Normal : NORMAL0;            // Normal vector for lighting
    float2 TextureCoord : TEXCOORD0;    // Texture coordinates (u,v)
};

// Output structure from vertex shader to pixel shader
// This data is interpolated (blended) across the triangle
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;       // Screen position (required output)
    float2 TextureCoord : TEXCOORD0;     // Texture coordinates to pass to pixel shader
    float3 Normal : TEXCOORD1;           // Normal in world space for lighting
    float3 WorldPos : TEXCOORD2;         // Position in world space for lighting
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
    
    // Pass the texture coordinates to the pixel shader
    output.TextureCoord = input.TextureCoord;
    
    // Transform the normal to world space
    output.Normal = normalize(mul(input.Normal, (float3x3)World));
    
    // Calculate the vertex position in world space for lighting
    output.WorldPos = mul(input.Position, World).xyz;
    
    return output;
}

//========================================================================
// PIXEL SHADER
//========================================================================
// The pixel shader runs once for each pixel that will be drawn
// It determines the final color that appears on screen
float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Normalize the interpolated normal
    float3 normal = normalize(input.Normal);
    
    // Sample the texture color at the current texture coordinate
    float4 textureColor = tex2D(ModelTextureSampler, input.TextureCoord);
    
    // Calculate main directional light contribution
    float mainLightFactor = max(0, dot(normal, -LightDirection));
    float3 mainLightContribution = DiffuseLight * mainLightFactor;
    
    // Calculate sunlight contribution
    float sunlightFactor = max(0, dot(normal, -SunlightDirection));
    float3 sunlightContribution = SunlightColor * sunlightFactor * SunlightIntensity;
    
    // Calculate specular reflection for main light
    float3 viewDirection = normalize(CameraPosition - input.WorldPos);
    float3 reflectionDirection = reflect(LightDirection, normal);
    float specularFactor = pow(max(0, dot(reflectionDirection, viewDirection)), SpecularPower);
    float3 mainSpecular = SpecularLight * specularFactor;
    
    // Calculate specular reflection for sunlight
    float3 sunReflectionDirection = reflect(SunlightDirection, normal);
    float sunSpecularFactor = pow(max(0, dot(sunReflectionDirection, viewDirection)), SpecularPower);
    float3 sunSpecular = SpecularLight * sunSpecularFactor * SunlightIntensity;
    
    // Combine all lighting contributions
    float3 finalColor = textureColor.rgb * (AmbientLight + mainLightContribution + sunlightContribution) + mainSpecular + sunSpecular;
    
    // Return the final color with the original alpha
    return float4(finalColor, textureColor.a);
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
}
