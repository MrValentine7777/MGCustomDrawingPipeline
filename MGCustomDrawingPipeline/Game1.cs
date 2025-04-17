using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGCustomDrawingPipeline
{
    /// <summary>
    /// Custom Drawing Pipeline Demo - MonoGame Tutorial Application
    /// 
    /// This application demonstrates how to create a basic 3D rendering pipeline in MonoGame
    /// by drawing a rotating colored triangle using custom vertex and index buffers along
    /// with a custom shader effect.
    /// </summary>
    public class Game1 : Game
    {
        #region Fields
        /// <summary>
        /// Manages the graphics device and provides methods for setting display modes
        /// </summary>
        private GraphicsDeviceManager _graphics;
        
        /// <summary>
        /// Helper for drawing sprites (not used in this demo, but commonly included)
        /// </summary>
        private SpriteBatch _spriteBatch;
        
        //===== 3D Rendering Components =====//
        
        /// <summary>
        /// Stores triangle vertex data (positions and colors) in GPU memory
        /// </summary>
        private VertexBuffer _vertexBuffer;
        
        /// <summary>
        /// Stores the order in which vertices should be drawn to form triangles
        /// </summary>
        private IndexBuffer _indexBuffer;
        
        /// <summary>
        /// Shader effect that handles the rendering of our triangle
        /// </summary>
        private Effect _triangleEffect;
        
        /// <summary>
        /// Defines the layout of our vertex data (not used in this demo, but declared for completeness)
        /// </summary>
        private VertexDeclaration _vertexDeclaration;
        
        //===== Post-Processing Components =====//
        
        /// <summary>
        /// Render target for the main scene
        /// </summary>
        private RenderTarget2D _sceneRenderTarget;
        
        /// <summary>
        /// Shader effect for the bloom post-processing
        /// </summary>
        private Effect _bloomEffect;
        
        /// <summary>
        /// Bloom intensity parameter
        /// </summary>
        private float _bloomIntensity = 1.5f;
        
        /// <summary>
        /// Bloom threshold parameter
        /// </summary>
        private float _bloomThreshold = 0.3f;
        
        /// <summary>
        /// Bloom blur amount parameter
        /// </summary>
        private float _bloomBlurAmount = 4.0f;
        
        //===== Animation Properties =====//
        
        /// <summary>
        /// Current rotation angle around X axis (in radians)
        /// </summary>
        private float _rotationAngleX = 0f;
        
        /// <summary>
        /// Current rotation angle around Y axis (in radians)
        /// </summary>
        private float _rotationAngleY = 0f;
        
        /// <summary>
        /// Current rotation angle around Z axis (in radians)
        /// </summary>
        private float _rotationAngleZ = 0f;
        
        /// <summary>
        /// Determines how triangles are drawn (front/back face rendering)
        /// </summary>
        private RasterizerState _doubleSidedRasterizerState;
        
        /// <summary>
        /// Toggle between normal rendering and post-processing
        /// </summary>
        private bool _usePostProcessing = false;
        #endregion

        /// <summary>
        /// Constructor - initializes the game
        /// </summary>
        public Game1()
        {
            // Create the graphics manager - this is required for any MonoGame project
            _graphics = new GraphicsDeviceManager(this);
            
            // Set the graphics profile to HiDef for higher quality rendering and more advanced shader support
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            
            // Set the folder where game content (like shaders) will be loaded from
            Content.RootDirectory = "Content";
            
            // Show the mouse cursor when it's over our game window
            IsMouseVisible = true;
        }

        /// <summary>
        /// Initialize game settings and objects before the main game loop begins
        /// </summary>
        protected override void Initialize()
        {
            //===== BEGINNER NOTE: Display Setup =====//
            // Here we configure the game window's size. This determines how large
            // our rendering area will be. Larger values give more detailed visuals
            // but may reduce performance on lower-end machines.
            _graphics.PreferredBackBufferWidth = 800;  // Width in pixels
            _graphics.PreferredBackBufferHeight = 600; // Height in pixels
            
            // Enable anti-aliasing for smoother edges (HiDef profile supports this)
            _graphics.PreferMultiSampling = true;
            
            // Use 24-bit depth buffer with 8-bit stencil for better precision in 3D rendering
            _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            
            _graphics.ApplyChanges();                  // Apply the new settings
            
            //===== BEGINNER NOTE: Rasterizer State =====//
            // The rasterizer controls how triangles are converted to pixels
            // By default, triangles facing away from the camera are not drawn (culled)
            // We disable culling here to see our triangle from both sides
            _doubleSidedRasterizerState = new RasterizerState
            {
                // When CullMode is None, both front and back faces are drawn
                CullMode = CullMode.None,  
                
                // FillMode.Solid means triangles are filled with color
                // (Alternative is FillMode.WireFrame to see only the edges)
                FillMode = FillMode.Solid  
            };

            // Always call the base class Initialize method
            base.Initialize();
        }

        /// <summary>
        /// Loads all content needed by the game (textures, models, sounds, etc.)
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch for drawing 2D graphics (not used in this demo)
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create render targets for post-processing
            CreateRenderTargets();

            //===== BEGINNER NOTE: What are Vertices? =====//
            // Vertices are points in 3D space that define the corners of 3D shapes.
            // Each vertex can have additional properties like color, texture coordinates, etc.
            // Here we create three vertices to form a triangle:
            //   - Each Vector3 represents (x, y, z) coordinates in 3D space
            //   - x and y range from -1 to 1 (center of screen is 0,0)
            //   - z is depth (positive is away from the camera)
            //   - Each vertex also has a color assigned to it
            var vertices = new VertexPositionColor[3]
            {
                // Bottom-left vertex: red color
                new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.1f), Color.Red),
                
                // Top vertex: green color  
                new VertexPositionColor(new Vector3(0, 0.5f, -0.1f), Color.Green),
                
                // Bottom-right vertex: blue color
                new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.1f), Color.Blue)
            };

            //===== BEGINNER NOTE: Vertex Buffers =====//
            // A vertex buffer holds vertex data in GPU memory for fast rendering
            // This is much more efficient than sending vertex data for each frame
            _vertexBuffer = new VertexBuffer(
                GraphicsDevice,              // The graphics device to use
                typeof(VertexPositionColor), // The type of vertices we're storing
                vertices.Length,             // How many vertices to allocate space for
                BufferUsage.WriteOnly);      // Optimization hint (we won't read this data back)
            
            // Copy our vertex data to the GPU memory
            _vertexBuffer.SetData(vertices);

            //===== BEGINNER NOTE: Indices =====//
            // Indices determine the order in which vertices are drawn to form triangles
            // For a single triangle, this is simple (0,1,2), but for complex models
            // using indices lets vertices be reused for multiple triangles
            var indices = new short[] { 0, 1, 2 };  // Draw vertices in this order

            // Create an index buffer to hold these indices in GPU memory
            _indexBuffer = new IndexBuffer(
                GraphicsDevice,              // The graphics device to use
                IndexElementSize.SixteenBits, // Size of each index (16-bit = short)
                indices.Length,              // How many indices to store
                BufferUsage.WriteOnly);      // Optimization hint
            
            // Copy our index data to the GPU memory
            _indexBuffer.SetData(indices);

            //===== BEGINNER NOTE: Shaders =====//
            // Shaders are small programs that run on the GPU and control rendering
            // They determine how vertices are transformed and how pixels are colored
            // Our shader is defined in the TriangleShader.fx file and compiled
            // during the content build process
            _triangleEffect = Content.Load<Effect>("TriangleShader");
            
            // Load the bloom post-processing shader
            _bloomEffect = Content.Load<Effect>("BloomShader");
        }
        
        /// <summary>
        /// Creates the render targets used for post-processing
        /// </summary>
        private void CreateRenderTargets()
        {
            // Get the current display size
            int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = GraphicsDevice.PresentationParameters.BackBufferHeight;
            
            // Create the main scene render target - using Color format for now
            // We'll switch to HDR format once basic rendering is working
            _sceneRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color, // Use standard color format first
                DepthFormat.Depth24Stencil8);
        }

        /// <summary>
        /// Updates the game state each frame - handles input, physics, AI, etc.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void Update(GameTime gameTime)
        {
            // Check for keyboard input
            KeyboardState keyboardState = Keyboard.GetState();
            
            // Toggle post-processing with P key
            if (keyboardState.IsKeyDown(Keys.P) && !_previousKeyboardState.IsKeyDown(Keys.P))
            {
                _usePostProcessing = !_usePostProcessing;
            }
            
            // Store current keyboard state for next frame
            _previousKeyboardState = keyboardState;
            
            //===== BEGINNER NOTE: Time-Based Animation =====//
            // To make animations smooth regardless of frame rate, we adjust
            // changes based on how much time has passed since the last frame
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update rotation angles at different speeds for each axis
            // This creates a more interesting animation than rotating around
            // just one axis would
            _rotationAngleX += 1.0f * elapsed;  // Rotate around X axis 
            _rotationAngleY += 1.5f * elapsed;  // Rotate around Y axis (faster)
            _rotationAngleZ += 0.7f * elapsed;  // Rotate around Z axis (slower)
            
            //===== BEGINNER NOTE: Angle Management =====//
            // Keep angles between 0 and 2π (full circle in radians)
            // This isn't strictly necessary but prevents the values from
            // growing too large over time which could cause precision issues
            _rotationAngleX %= MathHelper.TwoPi;  // 2π ≈ 6.28 radians = 360 degrees
            _rotationAngleY %= MathHelper.TwoPi;
            _rotationAngleZ %= MathHelper.TwoPi;

            // Always call the base class Update method
            base.Update(gameTime);
        }
        private KeyboardState _previousKeyboardState;

        /// <summary>
        /// Draws the game content to the screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void Draw(GameTime gameTime)
        {
            if (_usePostProcessing)
            {
                // Draw to render target first
                DrawSceneToRenderTarget();
                
                // Then draw the render target to the screen
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);
                
                // Just draw the render target directly to the screen for now
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                _spriteBatch.Draw(_sceneRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
                _spriteBatch.End();
            }
            else
            {
                // Draw directly to the screen
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.CornflowerBlue);
                DrawTriangle();
            }

            // Always call the base class Draw method
            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Draws the triangle scene to the scene render target
        /// </summary>
        private void DrawSceneToRenderTarget()
        {
            // Set the render target to draw to our scene texture
            GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            
            // Clear the render target with dark blue color
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Draw the triangle
            DrawTriangle();
        }
        
        /// <summary>
        /// Draws the rotating triangle
        /// </summary>
        private void DrawTriangle()
        {
            // Apply our double-sided rendering settings so triangle is visible from both sides
            GraphicsDevice.RasterizerState = _doubleSidedRasterizerState;

            // Tell the GPU which vertices and indices to use for drawing
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;

            // Create a world matrix that combines rotations around all three axes
            Matrix world = Matrix.CreateRotationX(_rotationAngleX) * 
                          Matrix.CreateRotationY(_rotationAngleY) * 
                          Matrix.CreateRotationZ(_rotationAngleZ);
            
            // Create a view matrix - this is like placing a camera in the world
            Matrix view = Matrix.CreateLookAt(
                new Vector3(0, 0, 2),  // Camera position: 2 units away from origin
                Vector3.Zero,          // Looking at the origin (0,0,0)
                Vector3.Up);           // "Up" direction is +Y axis
            
            // Create a projection matrix - this adds perspective effect
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,                    // Field of view: 45 degrees 
                GraphicsDevice.Viewport.AspectRatio,   // Match the window's aspect ratio
                0.1f,                                  // Near clipping plane (min render distance)
                100.0f);                               // Far clipping plane (max render distance)

            // Send the transformation matrix to the shader
            _triangleEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);

            // Draw the triangle using our shader
            foreach (EffectPass pass in _triangleEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,       // Base vertex offset
                    0,       // Start index
                    1        // Number of triangles
                );
            }
        }

        /// <summary>
        /// Clean up resources when the game is shutting down
        /// </summary>
        protected override void UnloadContent()
        {
            //===== BEGINNER NOTE: Resource Management =====//
            // Properly disposing resources prevents memory leaks
            // The ?. operator is the null conditional operator - it only calls
            // Dispose() if the object is not null
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _doubleSidedRasterizerState?.Dispose();
            
            // Dispose the render target
            _sceneRenderTarget?.Dispose();
            
            // Always call the base class UnloadContent method
            base.UnloadContent();
        }
    }
}
