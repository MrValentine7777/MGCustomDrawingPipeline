//======================================================================
// BLOOM SHADER EFFECT
//======================================================================
// This shader implements a bloom post-processing effect that makes
// bright areas of the image glow. It uses a three-step process:
// 1. Extract bright areas from the source image (BloomExtract)
// 2. Blur the bright areas using a two-pass gaussian blur (GaussianBlur)
// 3. Combine the blurred bright areas with the original image (BloomCombine)

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
texture InputTexture;
sampler2D InputSampler = sampler_state
{
    Texture = <InputTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

// Original scene texture (used in the final combination)
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
float BloomThreshold = 0.3f;

// Intensity of the bloom effect
float BloomIntensity = 1.5f;

// Amount of blur to apply
float BlurAmount = 4.0f;

// Direction of blur (horizontal = (1,0), vertical = (0,1))
float2 BlurDirection;

// Screen texture size (used for correct pixel sampling)
float2 ScreenSize;

//======================================================================
// HELPER FUNCTIONS
//======================================================================
// Helper function to get the half-pixel offset for texel sampling
float2 GetHalfPixel()
{
    return float2(0.5f / ScreenSize.x, 0.5f / ScreenSize.y);
}

// Helper function to calculate the gaussian blur weight for a given distance
float CalcGaussianWeight(int sampleDistance)
{
    float sigma = BlurAmount;
    float g = 1.0f / sqrt(2.0f * 3.14159f * sigma * sigma);
    return g * exp(-(sampleDistance * sampleDistance) / (2 * sigma * sigma));
}

//======================================================================
// VERTEX SHADER (for full-screen quad)
//======================================================================
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
float4 BloomExtractPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample the original scene texture
    float4 color = tex2D(InputSampler, texCoord);
    
    // Calculate the pixel brightness (luminance) using the standard formula
    // This weights RGB based on human eye sensitivity
    float brightness = dot(color.rgb, float3(0.299f, 0.587f, 0.114f));
    
    // Subtract the threshold
    brightness = max(0, brightness - BloomThreshold);
    
    // Return the color if it's bright enough, otherwise black (no bloom)
    return color * brightness;
}

//======================================================================
// GAUSSIAN BLUR PIXEL SHADER
//======================================================================
// This shader applies a one-dimensional gaussian blur in either the horizontal
// or vertical direction depending on the BlurDirection parameter
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
