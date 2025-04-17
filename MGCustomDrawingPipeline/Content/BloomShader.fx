#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

//======================================================================
// SHADER PARAMETERS
//======================================================================
// The texture that contains the scene to process
// This is the primary input image that we'll apply effects to
texture InputTexture;
sampler2D InputSampler = sampler_state
{
    Texture = <InputTexture>;
    MinFilter = Linear;  // Use linear filtering for smoother results when scaling
    MagFilter = Linear;
    AddressU = Clamp;    // Clamp texture coordinates to prevent wrapping
    AddressV = Clamp;
};

// Original scene texture (used in the final combination)
// This preserves the original image that bloom will be added to
texture BaseTexture;
sampler2D BaseSampler = sampler_state
{
    Texture = <BaseTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

// Bloom texture (the blurred bright areas)
// This contains the glow effect that will be added to the original image
texture BloomTexture;
sampler2D BloomSampler = sampler_state
{
    Texture = <BloomTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

// Brightness threshold for bloom extraction (pixels brighter than this will bloom)
// Lower values cause more of the scene to glow, higher values limit glow to the brightest areas
float BloomThreshold = 0.3f;

// Intensity of the bloom effect
// Controls how bright the glow appears in the final image
float BloomIntensity = 1.5f;

// Amount of blur to apply
// Higher values create a wider, more diffuse glow
float BlurAmount = 4.0f;

// Direction of blur (horizontal = (1,0), vertical = (0,1))
// We use separate horizontal and vertical blur passes for better performance
float2 BlurDirection;

// Screen texture size (used for correct pixel sampling)
// This helps the shader calculate proper pixel offsets for sampling
float2 ScreenSize;

// Target color to bloom (for color-specific bloom)
// RGB values should be between 0 and 1
float3 TargetColor = float3(100.0f/255.0f, 149.0f/255.0f, 237.0f/255.0f); // CornflowerBlue

// Color sensitivity (how close to the target color a pixel needs to be to bloom)
// Higher values cause more colors to bloom, making the effect less targeted
float ColorSensitivity = 0.25f;

//======================================================================
// HELPER FUNCTIONS
//======================================================================
// Helper function to get the half-pixel offset for texel sampling
// This helps prevent sampling artifacts by centering samples on texels
float2 GetHalfPixel()
{
    return float2(0.5f / ScreenSize.x, 0.5f / ScreenSize.y);
}

// Helper function to calculate the gaussian blur weight for a given distance
// Implements a gaussian distribution for natural-looking blur
float CalcGaussianWeight(int sampleDistance)
{
    // Standard deviation of our gaussian function is the blur amount
    float sigma = BlurAmount;
    
    // Gaussian function constant
    float g = 1.0f / sqrt(2.0f * 3.14159f * sigma * sigma);
    
    // Gaussian function for the given distance
    return g * exp(-(sampleDistance * sampleDistance) / (2 * sigma * sigma));
}

// Helper function to calculate how close a color is to the target color
// Returns a value between 0 and 1, where 1 means identical colors
float ColorSimilarity(float3 color, float3 targetColor)
{
    // Normalized color distance (0 = identical, 1 = maximally different)
    float distance = length(color - targetColor);
    
    // Invert and scale to create a similarity factor (1 = identical, 0 = maximally different)
    // ColorSensitivity controls how strictly we match the target color
    return saturate(1.0f - distance / ColorSensitivity);
}

//======================================================================
// VERTEX SHADER (for full-screen quad)
//======================================================================
// This simple vertex shader just passes through vertex data without changes
// It's used for rendering full-screen quads for post-processing
void VertexShaderFunction(inout float4 position : POSITION0,
                          inout float2 texCoord : TEXCOORD0)
{
    // The vertex shader doesn't do any transformation; the full-screen quad
    // is already in the correct position.
}

//======================================================================
// BLOOM EXTRACT PIXEL SHADER
//======================================================================
// This shader extracts the bright areas of the scene by thresholding
// Used as the first step in the bloom process to isolate bright areas
float4 BloomExtractPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample the original scene texture
    float4 color = tex2D(InputSampler, texCoord);
    
    // Calculate the pixel brightness (luminance) using the standard formula
    // This weights RGB based on human eye sensitivity
    float brightness = dot(color.rgb, float3(0.299f, 0.587f, 0.114f));
    
    // Subtract the threshold to isolate only bright areas
    // If brightness is below threshold, this will be zero
    brightness = max(0, brightness - BloomThreshold);
    
    // Return the color if it's bright enough, otherwise black (no bloom)
    return color * brightness;
}

//======================================================================
// GREEN COLOR BLOOM EXTRACT PIXEL SHADER
//======================================================================
// This shader extracts specifically green colors for bloom
// Used to make green elements like foliage glow
float4 GreenBloomExtractPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample the original scene texture
    float4 color = tex2D(InputSampler, texCoord);
    
    // Calculate how similar this pixel is to our target green color
    float similarity = ColorSimilarity(color.rgb, TargetColor);
    
    // Calculate brightness, factoring in the color similarity
    float brightness = dot(color.rgb, float3(0.299f, 0.587f, 0.114f));
    brightness = max(0, brightness - BloomThreshold);
    
    // Apply stronger bloom to colors that are closer to our target green
    float bloomFactor = brightness * similarity;
    
    // Return the color with the bloom factor applied
    return color * bloomFactor;
}

//======================================================================
// BLUE COLOR BLOOM EXTRACT PIXEL SHADER
//======================================================================
// This shader extracts specifically blue colors for bloom
// Used to make blue elements like water glow
float4 BlueBloomExtractPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample the original scene texture
    float4 color = tex2D(InputSampler, texCoord);
    
    // Calculate how similar this pixel is to our target blue color
    float similarity = ColorSimilarity(color.rgb, TargetColor);
    
    // Calculate brightness, factoring in the color similarity
    float brightness = dot(color.rgb, float3(0.299f, 0.587f, 0.114f));
    brightness = max(0, brightness - BloomThreshold);
    
    // Apply stronger bloom to colors that are closer to our target blue
    float bloomFactor = brightness * similarity;
    
    // Return the color with the bloom factor applied
    return color * bloomFactor;
}

//======================================================================
// SUNLIGHT BLOOM EXTRACT PIXEL SHADER
//======================================================================
// This shader extracts areas affected by sunlight for bloom effect
// Used to enhance sunlight effects in the scene
float4 SunlightBloomExtractPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample the original scene texture
    float4 color = tex2D(InputSampler, texCoord);
    
    // Calculate brightness using luminance
    float brightness = dot(color.rgb, float3(0.299f, 0.587f, 0.114f));
    
    // Target warmer colors (sunlight tends to be yellowish)
    float3 sunlightColor = float3(1.0f, 0.9f, 0.7f);
    
    // Calculate how similar this pixel is to our target sunlight color
    float similarity = ColorSimilarity(color.rgb, sunlightColor);
    
    // Calculate brightness, factoring in the color similarity
    brightness = max(0, brightness - BloomThreshold * 0.5f); // Lower threshold for sunlight
    
    // Apply stronger bloom to colors that are closer to our sunlight color
    float bloomFactor = brightness * similarity * 1.5f; // Intensify sunlight bloom
    
    // Return the color with the bloom factor applied
    return color * bloomFactor;
}

//======================================================================
// GAUSSIAN BLUR PIXEL SHADER
//======================================================================
// This shader applies a one-dimensional gaussian blur in either the horizontal
// or vertical direction depending on the BlurDirection parameter
// Used to create a smooth glow effect for bloom
float4 GaussianBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Final color will be accumulated here
    float4 color = float4(0, 0, 0, 0);
    
    // Normalize the blur direction to ensure consistent blur radius
    // regardless of screen aspect ratio
    float2 dir = normalize(BlurDirection) / ScreenSize;
    
    // Blur kernel size (number of samples on each side)
    const int KERNEL_SIZE = 7;
    
    // Sample weights (calculated based on a gaussian function)
    float totalWeight = 0.0f;
    float weight;
    
    // Sample multiple pixels and blend them with the appropriate weights
    for (int i = -KERNEL_SIZE; i <= KERNEL_SIZE; i++)
    {
        // Calculate the sample position
        float2 samplePos = texCoord + (dir * i * BlurAmount);
        
        // Calculate weight based on distance from center
        weight = CalcGaussianWeight(i);
        totalWeight += weight;
        
        // Sample the texture at the calculated position
        float4 sample = tex2D(InputSampler, samplePos);
        
        // Add the weighted sample to the result
        color += sample * weight;
    }
    
    // Normalize the result by the total weight
    return color / totalWeight;
}

//======================================================================
// BLOOM COMBINE PIXEL SHADER
//======================================================================
// This shader combines the original scene with the bloom texture
// Used as the final step in the bloom process
float4 BloomCombinePS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample the original scene texture
    float4 baseColor = tex2D(BaseSampler, texCoord);
    
    // Sample the bloom texture
    float4 bloomColor = tex2D(BloomSampler, texCoord);
    
    // Combine the two textures
    float4 finalColor = baseColor + bloomColor * BloomIntensity;
    
    // Ensure alpha is correct (optional - depends on your use case)
    finalColor.a = baseColor.a;
    
    return finalColor;
}

//======================================================================
// TECHNIQUES
//======================================================================
technique BloomExtract
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL BloomExtractPS();
    }
}

technique GreenBloomExtract
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL GreenBloomExtractPS();
    }
}

technique BlueBloomExtract
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL BlueBloomExtractPS();
    }
}

technique SunlightBloomExtract
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL SunlightBloomExtractPS();
    }
}

technique GaussianBlur
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL GaussianBlurPS();
    }
}

technique BloomCombine
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL BloomCombinePS();
    }
}
