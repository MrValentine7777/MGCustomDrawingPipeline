using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MGCustomDrawingPipeline.VertexTypes;

namespace MGCustomDrawingPipeline
{
    /// <summary>
    /// Stores all the state data for the game
    /// 
    /// ===== BEGINNER'S GUIDE: GAME STATE MANAGEMENT =====
    /// 
    /// In game development, it's important to organize game data in a structured way.
    /// This GameState class acts as a central repository for all the data our application needs:
    /// 
    /// 1. Graphics Resources - Buffers, textures, and shaders used for rendering
    /// 2. Lighting Parameters - Colors, directions, and intensities for scene illumination
    /// 3. Post-Processing Settings - Parameters that control visual effects
    /// 4. Animation State - Current rotation angles for animated objects
    /// 5. Rendering Options - Flags that control how objects are drawn
    /// 
    /// By centralizing this data, we make the code more maintainable and easier to understand.
    /// It also facilitates passing information between different components of our application.
    /// 
    /// For example, our TreeRenderer needs access to the vertex buffer, shader, and textures,
    /// while the PostProcessingRenderer needs access to render targets and shader parameters.
    /// Storing everything in one place means each component can access what it needs.
    /// </summary>
    public class GameState
    {
        //===== Core Graphics Components =====//
        
        /// <summary>
        /// Manages the graphics device and provides methods for setting display modes
        /// 
        /// The GraphicsDeviceManager is responsible for:
        /// - Creating and configuring the GraphicsDevice
        /// - Setting screen resolution and display mode
        /// - Configuring graphics quality options like anti-aliasing
        /// - Managing the application window properties
        /// </summary>
        public GraphicsDeviceManager Graphics { get; set; }
        
        /// <summary>
        /// Helper for drawing 2D sprites and textures to the screen
        /// 
        /// SpriteBatch batches multiple draw operations together for better performance.
        /// In this project, we use it primarily for:
        /// - Drawing the post-processed scene to the screen
        /// - Applying shader effects to full-screen textures in the post-processing pipeline
        /// </summary>
        public SpriteBatch SpriteBatch { get; set; }
        
        //===== 3D Rendering Components =====//
        
        /// <summary>
        /// Stores tree vertex data (positions, normals, and texture coordinates) in GPU memory
        /// 
        /// A VertexBuffer is a block of graphics memory that stores the vertices of our 3D model.
        /// Each vertex contains position, normal, and texture coordinate data.
        /// Storing this data on the GPU allows for efficient rendering without having to 
        /// transfer data from CPU to GPU each frame.
        /// </summary>
        public VertexBuffer VertexBuffer { get; set; }
        
        /// <summary>
        /// Stores the order in which vertices should be drawn to form triangles
        /// 
        /// An IndexBuffer contains integer indices that reference vertices in the vertex buffer.
        /// This allows us to reuse vertices when defining triangles, which saves memory
        /// and improves performance. For example, a cube with 8 vertices would need
        /// 36 indices to define its 12 triangles (2 triangles per face).
        /// </summary>
        public IndexBuffer IndexBuffer { get; set; }
        
        /// <summary>
        /// Shader effect that handles the rendering of our tree
        /// 
        /// A shader is a small program that runs on the GPU and determines how objects are rendered.
        /// Our TriangleEffect shader includes:
        /// - Vertex shader: Transforms vertex positions and calculates lighting data
        /// - Pixel shader: Determines the final color of each pixel based on textures and lighting
        /// </summary>
        public Effect TriangleEffect { get; set; }
        
        /// <summary>
        /// Defines the layout of our vertex data (not used in this demo, but declared for completeness)
        /// 
        /// A VertexDeclaration tells the graphics device how to interpret the data in a vertex buffer,
        /// including the size and meaning of each component (position, normal, texture coordinates).
        /// </summary>
        public VertexDeclaration VertexDeclaration { get; set; }
        
        /// <summary>
        /// Texture for trunk color (1x1 pixel brown)
        /// 
        /// This is a simple 1x1 pixel texture used to color the trunk of our tree.
        /// Using a tiny texture is more efficient than setting a color directly,
        /// as it fits into the standard rendering pipeline that expects textures.
        /// </summary>
        public Texture2D TrunkTexture { get; set; }
        
        /// <summary>
        /// Texture for leaf color (1x1 pixel green)
        /// 
        /// Similar to the trunk texture, this is a 1x1 pixel texture used for the tree's foliage.
        /// Using separate textures for different parts allows us to color them differently.
        /// </summary>
        public Texture2D LeafTexture { get; set; }
        
        //===== Lighting Parameters =====//
        
        /// <summary>
        /// Direction of the main light in the scene (normalized)
        /// 
        /// This vector points from the light source toward the scene.
        /// It's normalized (length = 1) to ensure consistent lighting calculations.
        /// This light provides general illumination to the scene.
        /// </summary>
        public Vector3 LightDirection { get; set; } = Vector3.Normalize(new Vector3(1, -1, -1));

        /// <summary>
        /// Direction of the sunlight in the scene (normalized)
        /// 
        /// This is our secondary light source, representing the sun.
        /// It has a different direction and color than the main light,
        /// creating more interesting and dynamic lighting in the scene.
        /// </summary>
        public Vector3 SunlightDirection { get; set; } = Vector3.Normalize(new Vector3(-0.5f, -1, 0.2f));

        /// <summary>
        /// Ambient light color and intensity
        /// 
        /// Ambient light provides a base level of illumination to all surfaces,
        /// regardless of their orientation. This simulates light that has bounced
        /// around the environment multiple times, illuminating areas not directly
        /// facing any light source.
        /// 
        /// Values are in RGB format, where (0,0,0) is black (no ambient light)
        /// and (1,1,1) would be bright white ambient light.
        /// </summary>
        public Vector3 AmbientLight { get; set; } = new Vector3(0.2f, 0.2f, 0.2f);

        /// <summary>
        /// Diffuse light color and intensity
        /// 
        /// Diffuse light is directional light that illuminates surfaces based on
        /// their angle to the light source. Surfaces facing directly toward the light
        /// receive full illumination, while surfaces facing away receive none.
        /// 
        /// This creates the basic shading that gives objects their 3D appearance.
        /// Values are in RGB format, controlling both color and intensity.
        /// </summary>
        public Vector3 DiffuseLight { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// Sunlight color and intensity - warm yellowish tone for realistic sunlight
        /// 
        /// RGB values (1.0, 0.9, 0.5) create a warm yellow-orange color that simulates
        /// natural sunlight. The higher red and green components with a lower blue
        /// component create the characteristic warmth of sunlight.
        /// </summary>
        public Vector3 SunlightColor { get; set; } = new Vector3(1.0f, 0.9f, 0.5f);

        /// <summary>
        /// Sunlight intensity - controls brightness of the sunlight effect
        /// 
        /// This multiplier is applied to the sunlight color to control its overall brightness.
        /// Values greater than 1.0 create a more intense effect, while values less than 1.0
        /// would create a dimmer effect. The value of 1.2 creates slightly intensified sunlight
        /// for a more dramatic lighting effect.
        /// </summary>
        public float SunlightIntensity { get; set; } = 1.2f;

        /// <summary>
        /// Specular light color and intensity - controls the color of light reflections
        /// 
        /// Specular highlights are the bright spots that appear on shiny surfaces.
        /// These highlights represent direct reflections of light sources and give
        /// surfaces a glossy appearance. The color and brightness of these highlights
        /// are controlled by this vector (RGB format).
        /// </summary>
        public Vector3 SpecularLight { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// Specular power - controls the sharpness of specular highlights
        /// 
        /// This parameter determines how focused the specular highlights are:
        /// - Higher values (e.g., 50+): Small, sharp highlights suggesting a very smooth surface like metal or glass
        /// - Medium values (e.g., 10-30): Moderately sized highlights for surfaces like plastic or painted wood
        /// - Lower values (e.g., 1-5): Broad highlights suggesting a rougher surface like cloth or matte paint
        /// 
        /// A value of 20.0 creates moderately sharp highlights, appropriate for a semi-glossy surface.
        /// </summary>
        public float SpecularPower { get; set; } = 20.0f;

        /// <summary>
        /// Position of the camera in world space
        /// 
        /// The camera position is needed for calculating specular reflections, which
        /// depend on the viewing angle. It represents the point from which the scene is viewed.
        /// 
        /// In this example, the camera is positioned 2 units away from the origin along the Z axis,
        /// looking toward the origin (0,0,0) where our tree is located.
        /// </summary>
        public Vector3 CameraPosition { get; set; } = new Vector3(0, 0, 2);
        
        //===== Post-Processing Components =====//
        
        /// <summary>
        /// Render target for the main scene
        /// 
        /// A render target is like a virtual canvas that we can draw to instead of the screen.
        /// This particular render target holds our complete 3D scene before any post-processing.
        /// 
        /// Think of it like taking a photograph of our 3D scene - we can then apply
        /// effects to this "photograph" before showing it to the user.
        /// </summary>
        public RenderTarget2D SceneRenderTarget { get; set; }
        
        /// <summary>
        /// Shader effect for the bloom post-processing
        /// 
        /// This shader contains multiple techniques that implement our bloom effect:
        /// 1. Extraction: Identifies bright areas based on brightness and color
        /// 2. Blurring: Applies Gaussian blur to create a glow
        /// 3. Combination: Merges the original scene with the blurred glow
        /// </summary>
        public Effect BloomEffect { get; set; }
        
        /// <summary>
        /// Bloom intensity parameter
        /// 
        /// Controls how bright the bloom effect appears in the final image.
        /// Higher values (like our 2.5) create a more pronounced, vibrant glow,
        /// while lower values would create a subtler effect.
        /// 
        /// This is a key parameter to adjust when fine-tuning your bloom effect.
        /// </summary>
        public float BloomIntensity { get; set; } = 2.5f;
        
        /// <summary>
        /// Bloom threshold parameter
        /// 
        /// Determines the minimum brightness level that will produce bloom.
        /// Only pixels brighter than this threshold will glow.
        /// 
        /// Our low value of 0.1f means that many areas will produce some bloom,
        /// creating a more dreamy, diffuse glow. Higher values would limit the
        /// bloom to only the very brightest areas in the scene.
        /// </summary>
        public float BloomThreshold { get; set; } = 0.1f;
        
        /// <summary>
        /// Bloom blur amount parameter
        /// 
        /// Controls how far the glow spreads from bright areas.
        /// Higher values (like our 6.0) create a wider, more diffuse glow,
        /// while lower values would create a tighter, more focused glow.
        /// 
        /// This value affects the radius of the Gaussian blur applied in the bloom shader.
        /// </summary>
        public float BloomBlurAmount { get; set; } = 6.0f;
        
        /// <summary>
        /// Render target for bloom extraction
        /// 
        /// This render target stores the first stage of the bloom effect - the extracted
        /// bright areas from the scene that will glow. These are identified by the
        /// bloom extraction shader based on brightness and color similarity.
        /// 
        /// It contains only the bright areas, with everything else black.
        /// </summary>
        public RenderTarget2D BloomExtractTarget { get; set; }
        
        /// <summary>
        /// Render target for horizontal blur
        /// 
        /// This render target stores the intermediate result after applying horizontal blur.
        /// We split the blur into two passes (horizontal and vertical) for better performance.
        /// 
        /// A separable 2D Gaussian blur applied in two 1D passes is much faster than
        /// a single 2D blur, especially for larger blur radii.
        /// </summary>
        public RenderTarget2D BloomHorizontalBlurTarget { get; set; }
        
        /// <summary>
        /// Render target for vertical blur
        /// 
        /// This render target stores the final blurred result after applying vertical blur
        /// to the horizontal blur result. This completes our two-pass blur process.
        /// 
        /// The result is a smoothly blurred version of the extracted bright areas,
        /// ready to be combined with the original scene to create the final bloom effect.
        /// </summary>
        public RenderTarget2D BloomVerticalBlurTarget { get; set; }
        
        /// <summary>
        /// Sensitivity for color bloom extraction
        /// 
        /// This parameter controls how strictly the bloom extraction shader matches colors.
        /// Higher values (like our 0.5) make the shader less strict, allowing more colors
        /// similar to the target color to produce bloom.
        /// 
        /// This is useful for creating a more artistic, stylized bloom effect that
        /// targets specific color ranges in your scene.
        /// </summary>
        public float ColorSensitivity { get; set; } = 0.5f;
        
        /// <summary>
        /// Target blue color for bloom extraction
        /// 
        /// This color defines what the bloom shader looks for when determining which areas should glow.
        /// In this case, we're targeting CornflowerBlue (100, 149, 237), which is the background color.
        /// 
        /// Note that colors are stored as normalized RGB values (0.0-1.0 instead of 0-255),
        /// which is why we divide each component by 255.
        /// </summary>
        public Vector3 TargetBlueColor { get; set; } = new Vector3(100.0f / 255.0f, 149.0f / 255.0f, 237.0f / 255.0f); // CornflowerBlue
        
        //===== Animation Properties =====//
        
        /// <summary>
        /// Current rotation angle around X axis (in radians)
        /// 
        /// This controls the forward/backward tilt of the tree.
        /// In 3D graphics, rotations are typically measured in radians rather than degrees.
        /// A full circle is 2π radians (approximately 6.28).
        /// </summary>
        public float RotationAngleX { get; set; } = 0f;
        
        /// <summary>
        /// Current rotation angle around Y axis (in radians)
        /// 
        /// This controls the left/right spin of the tree.
        /// The Y rotation is what creates the continuous spinning animation of our tree model.
        /// </summary>
        public float RotationAngleY { get; set; } = 0f;
        
        /// <summary>
        /// Current rotation angle around Z axis (in radians)
        /// 
        /// This controls the clockwise/counterclockwise roll of the tree.
        /// While not actively used in the current animation, this is included for completeness
        /// and to allow for more complex animations in the future.
        /// </summary>
        public float RotationAngleZ { get; set; } = 0f;
        
        //===== Rendering Options =====//
        
        /// <summary>
        /// Determines how triangles are drawn (front/back face rendering)
        /// 
        /// This rasterizer state is configured to draw both sides of triangles,
        /// which means we can see our model correctly even from inside or behind.
        /// 
        /// By default, graphics systems only draw the front faces of triangles
        /// (those whose vertices are defined in counter-clockwise order).
        /// </summary>
        public RasterizerState DoubleSidedRasterizerState { get; set; }
        
        /// <summary>
        /// Determines how triangles are drawn in wireframe mode
        /// 
        /// When active, this rasterizer state draws only the edges of triangles,
        /// not their filled interiors. This is useful for visualizing the structure
        /// of 3D models and understanding how they're built from triangles.
        /// 
        /// Like the standard rasterizer, this also draws both sides of triangles.
        /// </summary>
        public RasterizerState WireframeRasterizerState { get; set; }
        
        /// <summary>
        /// Toggle between solid and wireframe rendering
        /// 
        /// When true, the model is rendered in wireframe mode, showing only the triangle edges.
        /// When false, the model is rendered normally with filled triangles.
        /// 
        /// This can be toggled during runtime by pressing the 'W' key.
        /// </summary>
        public bool UseWireframe { get; set; } = false;
        
        /// <summary>
        /// Toggle between normal rendering and post-processing
        /// 
        /// When true, the bloom post-processing effect is applied to the scene.
        /// When false, the scene is rendered directly without special effects.
        /// 
        /// This can be toggled during runtime by pressing the 'P' key.
        /// </summary>
        public bool UsePostProcessing { get; set; } = false;

        /// <summary>
        /// Previous keyboard state for detecting key presses
        /// 
        /// This stores the state of the keyboard from the last frame, allowing
        /// us to detect when keys are first pressed rather than held down.
        /// 
        /// By comparing current and previous states, we can detect a key press
        /// event exactly once, even if the key remains held down.
        /// </summary>
        public KeyboardState PreviousKeyboardState { get; set; }

        //===== Model Statistics =====//
        
        /// <summary>
        /// Total number of vertices in the tree model
        /// 
        /// Each vertex represents a point in 3D space with associated data like
        /// normals and texture coordinates. This count helps us understand the
        /// complexity of our model and is used in debugging and performance monitoring.
        /// </summary>
        public int TotalVertices { get; set; }

        /// <summary>
        /// Total number of indices in the tree model
        /// 
        /// Indices refer to vertices in the vertex buffer. A higher number of indices
        /// means more triangle definitions, even if they reuse the same vertices.
        /// This is closely related to the rendering complexity of our model.
        /// </summary>
        public int TotalIndices { get; set; }

        /// <summary>
        /// Total number of triangles in the tree model
        /// 
        /// Triangles are the fundamental rendering primitive in 3D graphics.
        /// Every visible 3D surface is composed of triangles. This count equals
        /// TotalIndices / 3, since each triangle requires 3 indices to define it.
        /// </summary>
        public int TotalTriangles { get; set; }
    }
}
