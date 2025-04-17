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
        }

        /// <summary>
        /// Updates the game state each frame - handles input, physics, AI, etc.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void Update(GameTime gameTime)
        {
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

        /// <summary>
        /// Draws the game content to the screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen to blue before drawing anything new
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //===== BEGINNER NOTE: 3D Rendering Setup =====//
            // Before drawing 3D objects, we need to configure how the graphics
            // pipeline will process our geometry
            
            // Apply our double-sided rendering settings so triangle is visible from both sides
            GraphicsDevice.RasterizerState = _doubleSidedRasterizerState;

            // Tell the GPU which vertices and indices to use for drawing
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;

            //===== BEGINNER NOTE: 3D Transformation Matrices =====//
            // To position and transform 3D objects, we use matrices:
            // 1. World matrix - positions objects in the 3D world
            // 2. View matrix - positions the camera in the world
            // 3. Projection matrix - handles perspective (things get smaller in distance)
            
            // Create a world matrix that combines rotations around all three axes
            Matrix world = Matrix.CreateRotationX(_rotationAngleX) * 
                          Matrix.CreateRotationY(_rotationAngleY) * 
                          Matrix.CreateRotationZ(_rotationAngleZ);
            
            // Create a view matrix - this is like placing a camera in the world
            // Parameters: camera position, target to look at, up direction
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

            //===== BEGINNER NOTE: Passing Data to Shaders =====//
            // Shaders need information from our C# code to work correctly
            // Here we pass the combined transformation matrix to the shader
            // This matrix will be used to position our triangle vertices
            _triangleEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);

            //===== BEGINNER NOTE: Drawing with Shaders =====//
            // Shaders can have multiple "passes" - ways of rendering the same object
            // We need to loop through each pass and apply it before drawing
            foreach (EffectPass pass in _triangleEffect.CurrentTechnique.Passes)
            {
                // Apply this shader pass
                pass.Apply();

                // Draw the triangle using our vertex and index data
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,  // Drawing triangles (not lines or points)
                    0,       // Base vertex offset (first vertex to use)
                    0,       // Start index (first index to use)
                    1        // Number of triangles to draw
                );
            }

            // Always call the base class Draw method
            base.Draw(gameTime);
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
            
            // Always call the base class UnloadContent method
            base.UnloadContent();
        }
    }
}
