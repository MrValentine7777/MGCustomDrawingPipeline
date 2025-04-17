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
        /// This provides a base level of illumination to all surfaces regardless of orientation
        /// </summary>
        public Vector3 AmbientLight { get; set; } = new Vector3(0.2f, 0.2f, 0.2f);

        /// <summary>
        /// Diffuse light color and intensity
        /// This is the main directional light that illuminates surfaces based on their angle to the light
        /// </summary>
        public Vector3 DiffuseLight { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// Sunlight color and intensity - warm yellowish tone for realistic sunlight
        /// RGB values (1.0, 0.9, 0.5) create a warm yellow-orange color
        /// </summary>
        public Vector3 SunlightColor { get; set; } = new Vector3(1.0f, 0.9f, 0.5f);

        /// <summary>
        /// Sunlight intensity - controls brightness of the sunlight effect
        /// Higher values create a stronger, more dramatic lighting effect
        /// </summary>
        public float SunlightIntensity { get; set; } = 1.2f;

        /// <summary>
        /// Specular light color and intensity - controls the color of light reflections
        /// This creates bright highlights on surfaces facing the light
        /// </summary>
        public Vector3 SpecularLight { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// Specular power - controls the sharpness of specular highlights
        /// Higher values create smaller, more focused highlights simulating smoother surfaces
        /// Lower values create broader highlights simulating rougher surfaces
        /// </summary>
        public float SpecularPower { get; set; } = 20.0f;

        /// <summary>
        /// Position of the camera in world space
        /// Used for calculating view direction and specular reflections
        /// </summary>
        public Vector3 CameraPosition { get; set; } = new Vector3(0, 0, 2);
        
        //===== Post-Processing Components =====//
        
        /// <summary>
        /// Render target for the main scene
        /// This is where the initial 3D scene is rendered before post-processing
        /// Acts as an off-screen texture buffer that we can apply effects to
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
        /// Higher values create a more pronounced glow effect
        /// </summary>
        public float BloomIntensity { get; set; } = 2.5f;
        
        /// <summary>
        /// Bloom threshold parameter
        /// Determines the minimum brightness level that will produce bloom
        /// Lower values cause more areas to bloom, creating a more diffuse glow
        /// </summary>
        public float BloomThreshold { get; set; } = 0.1f;
        
        /// <summary>
        /// Bloom blur amount parameter
        /// Controls the spread of the bloom effect
        /// Higher values create a wider, more diffuse glow
        /// </summary>
        public float BloomBlurAmount { get; set; } = 6.0f;
        
        /// <summary>
        /// Render target for bloom extraction
        /// Stores the extracted colors from the scene that will later be blurred
        /// This isolates the bright areas that should glow
        /// </summary>
        public RenderTarget2D BloomExtractTarget { get; set; }
        
        /// <summary>
        /// Render target for horizontal blur
        /// Stores the intermediate result of applying horizontal Gaussian blur
        /// Part of the two-pass blur process for better performance
        /// </summary>
        public RenderTarget2D BloomHorizontalBlurTarget { get; set; }
        
        /// <summary>
        /// Render target for vertical blur
        /// Stores the result of applying vertical Gaussian blur to the horizontal blur result
        /// Completes the two-pass blur process for the bloom effect
        /// </summary>
        public RenderTarget2D BloomVerticalBlurTarget { get; set; }
        
        /// <summary>
        /// Sensitivity for color bloom extraction
        /// Higher values make more shades of the target color produce bloom
        /// This controls how precisely the shader targets specific colors
        /// </summary>
        public float ColorSensitivity { get; set; } = 0.5f;
        
        /// <summary>
        /// Target blue color for bloom extraction
        /// This color specifically targets the cornflower blue background
        /// Stored as normalized RGB values (0.0-1.0 instead of 0-255)
        /// </summary>
        public Vector3 TargetBlueColor { get; set; } = new Vector3(100.0f / 255.0f, 149.0f / 255.0f, 237.0f / 255.0f); // CornflowerBlue
        
        //===== Animation Properties =====//
        
        /// <summary>
        /// Current rotation angle around X axis (in radians)
        /// Controls the tilt of the tree forward/backward
        /// </summary>
        public float RotationAngleX { get; set; } = 0f;
        
        /// <summary>
        /// Current rotation angle around Y axis (in radians)
        /// Controls the spin of the tree left/right
        /// </summary>
        public float RotationAngleY { get; set; } = 0f;
        
        /// <summary>
        /// Current rotation angle around Z axis (in radians)
        /// Controls the roll of the tree clockwise/counterclockwise
        /// </summary>
        public float RotationAngleZ { get; set; } = 0f;
        
        /// <summary>
        /// Determines how triangles are drawn (front/back face rendering)
        /// Used for standard rendering with both front and back faces visible
        /// Enables seeing both sides of each triangle
        /// </summary>
        public RasterizerState DoubleSidedRasterizerState { get; set; }
        
        /// <summary>
        /// Determines how triangles are drawn in wireframe mode
        /// Shows only the edges of triangles for better understanding of the model structure
        /// Useful for visualizing the geometry of the 3D model
        /// </summary>
        public RasterizerState WireframeRasterizerState { get; set; }
        
        /// <summary>
        /// Toggle between solid and wireframe rendering
        /// When true, the model is rendered as wireframe showing only triangle edges
        /// When false, the model is rendered as solid triangles with color and lighting
        /// </summary>
        public bool UseWireframe { get; set; } = false;
        
        /// <summary>
        /// Toggle between normal rendering and post-processing
        /// When true, bloom post-processing is applied to the scene
        /// When false, the scene is rendered directly without effects
        /// </summary>
        public bool UsePostProcessing { get; set; } = false;

        /// <summary>
        /// Previous keyboard state for detecting key presses
        /// Used to detect when a key is first pressed (not held down)
        /// Allows toggling of features on a single key press
        /// </summary>
        public KeyboardState PreviousKeyboardState { get; set; }

        /// <summary>
        /// Total number of vertices in the tree model
        /// This helps track the size of the model for debugging and information purposes
        /// Each vertex contains position, normal, and texture coordinate data
        /// </summary>
        public int TotalVertices { get; set; }

        /// <summary>
        /// Total number of indices in the tree model
        /// This determines how many vertex lookups are needed for drawing the model
        /// Each triangle requires 3 indices, so this is typically 3x the triangle count
        /// </summary>
        public int TotalIndices { get; set; }

        /// <summary>
        /// Total number of triangles in the tree model
        /// Each triangle is composed of 3 indices, so this equals TotalIndices / 3
        /// Triangles are the basic primitive for 3D rendering
        /// </summary>
        public int TotalTriangles { get; set; }
    }
}
