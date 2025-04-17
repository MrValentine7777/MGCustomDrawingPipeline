using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MGCustomDrawingPipeline.VertexTypes;

namespace MGCustomDrawingPipeline
{
    /// <summary>
    /// Stores all the state data for the game
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// Manages the graphics device and provides methods for setting display modes
        /// </summary>
        public GraphicsDeviceManager Graphics { get; set; }
        
        /// <summary>
        /// Helper for drawing sprites (not used in this demo, but commonly included)
        /// </summary>
        public SpriteBatch SpriteBatch { get; set; }
        
        //===== 3D Rendering Components =====//
        
        /// <summary>
        /// Stores tree vertex data (positions and texture coordinates) in GPU memory
        /// </summary>
        public VertexBuffer VertexBuffer { get; set; }
        
        /// <summary>
        /// Stores the order in which vertices should be drawn to form triangles
        /// </summary>
        public IndexBuffer IndexBuffer { get; set; }
        
        /// <summary>
        /// Shader effect that handles the rendering of our tree
        /// </summary>
        public Effect TriangleEffect { get; set; }
        
        /// <summary>
        /// Defines the layout of our vertex data (not used in this demo, but declared for completeness)
        /// </summary>
        public VertexDeclaration VertexDeclaration { get; set; }
        
        /// <summary>
        /// Texture for trunk color (1x1 pixel brown)
        /// </summary>
        public Texture2D TrunkTexture { get; set; }
        
        /// <summary>
        /// Texture for leaf color (1x1 pixel green)
        /// </summary>
        public Texture2D LeafTexture { get; set; }
        
        /// <summary>
        /// Direction of the main light in the scene (normalized)
        /// </summary>
        public Vector3 LightDirection { get; set; } = Vector3.Normalize(new Vector3(1, -1, -1));

        /// <summary>
        /// Direction of the sunlight in the scene (normalized)
        /// </summary>
        public Vector3 SunlightDirection { get; set; } = Vector3.Normalize(new Vector3(-0.5f, -1, 0.2f));

        /// <summary>
        /// Ambient light color and intensity
        /// </summary>
        public Vector3 AmbientLight { get; set; } = new Vector3(0.2f, 0.2f, 0.2f);

        /// <summary>
        /// Diffuse light color and intensity
        /// </summary>
        public Vector3 DiffuseLight { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// Sunlight color and intensity
        /// </summary>
        public Vector3 SunlightColor { get; set; } = new Vector3(1.0f, 0.9f, 0.5f); // More saturated sunlight

        /// <summary>
        /// Sunlight intensity
        /// </summary>
        public float SunlightIntensity { get; set; } = 1.2f; // Increased from 0.8f

        /// <summary>
        /// Specular light color and intensity
        /// </summary>
        public Vector3 SpecularLight { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// Specular power - controls the sharpness of specular highlights
        /// </summary>
        public float SpecularPower { get; set; } = 20.0f;

        /// <summary>
        /// Position of the camera in world space
        /// </summary>
        public Vector3 CameraPosition { get; set; } = new Vector3(0, 0, 2);
        
        //===== Post-Processing Components =====//
        
        /// <summary>
        /// Render target for the main scene
        /// </summary>
        public RenderTarget2D SceneRenderTarget { get; set; }
        
        /// <summary>
        /// Shader effect for the bloom post-processing
        /// This shader implements a multi-pass bloom effect targeting specific colors
        /// </summary>
        public Effect BloomEffect { get; set; }
        
        /// <summary>
        /// Bloom intensity parameter
        /// Controls how bright the bloom effect appears in the final image
        /// </summary>
        public float BloomIntensity { get; set; } = 2.5f; // Increased from 1.5f
        
        /// <summary>
        /// Bloom threshold parameter
        /// Determines the minimum brightness level that will produce bloom
        /// </summary>
        public float BloomThreshold { get; set; } = 0.1f; // Lowered from 0.2f
        
        /// <summary>
        /// Bloom blur amount parameter
        /// Controls the spread of the bloom effect
        /// </summary>
        public float BloomBlurAmount { get; set; } = 6.0f; // Increased from 4.0f
        
        /// <summary>
        /// Render target for bloom extraction
        /// Stores the extracted colors from the scene
        /// </summary>
        public RenderTarget2D BloomExtractTarget { get; set; }
        
        /// <summary>
        /// Render target for horizontal blur
        /// Stores the intermediate result of applying horizontal Gaussian blur
        /// </summary>
        public RenderTarget2D BloomHorizontalBlurTarget { get; set; }
        
        /// <summary>
        /// Render target for vertical blur
        /// Stores the result of applying vertical Gaussian blur to the horizontal blur result
        /// </summary>
        public RenderTarget2D BloomVerticalBlurTarget { get; set; }
        
        /// <summary>
        /// Sensitivity for blue color bloom extraction
        /// Higher values make more shades of blue produce bloom
        /// </summary>
        public float ColorSensitivity { get; set; } = 0.5f; // Increased from 0.35f
        
        /// <summary>
        /// Target blue color for bloom extraction
        /// This color specifically targets the cornflower blue background
        /// </summary>
        public Vector3 TargetBlueColor { get; set; } = new Vector3(100.0f / 255.0f, 149.0f / 255.0f, 237.0f / 255.0f); // CornflowerBlue
        
        //===== Animation Properties =====//
        
        /// <summary>
        /// Current rotation angle around X axis (in radians)
        /// </summary>
        public float RotationAngleX { get; set; } = 0f;
        
        /// <summary>
        /// Current rotation angle around Y axis (in radians)
        /// </summary>
        public float RotationAngleY { get; set; } = 0f;
        
        /// <summary>
        /// Current rotation angle around Z axis (in radians)
        /// </summary>
        public float RotationAngleZ { get; set; } = 0f;
        
        /// <summary>
        /// Determines how triangles are drawn (front/back face rendering)
        /// </summary>
        public RasterizerState DoubleSidedRasterizerState { get; set; }
        
        /// <summary>
        /// Determines how triangles are drawn in wireframe mode
        /// </summary>
        public RasterizerState WireframeRasterizerState { get; set; }
        
        /// <summary>
        /// Toggle between solid and wireframe rendering
        /// </summary>
        public bool UseWireframe { get; set; } = false;
        
        /// <summary>
        /// Toggle between normal rendering and post-processing
        /// </summary>
        public bool UsePostProcessing { get; set; } = false;

        /// <summary>
        /// Previous keyboard state for detecting key presses
        /// </summary>
        public KeyboardState PreviousKeyboardState { get; set; }

        // Tree properties
        public int TotalVertices { get; set; }
        public int TotalIndices { get; set; }
        public int TotalTriangles { get; set; }
    }
}
