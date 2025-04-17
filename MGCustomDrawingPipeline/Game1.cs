using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using MGCustomDrawingPipeline.VertexTypes;

namespace MGCustomDrawingPipeline
{
    /// <summary>
    /// Custom Drawing Pipeline Demo - MonoGame Tutorial Application
    /// 
    /// This application demonstrates how to create a basic 3D rendering pipeline in MonoGame
    /// by drawing a rotating tree model using custom vertex and index buffers along
    /// with a custom shader effect. It also implements a color-targeted bloom post-processing
    /// effect that makes the tree's foliage glow.
    /// </summary>
    public class Game1 : Game
    {
        /// <summary>
        /// Contains all game state fields
        /// </summary>
        private GameState _state;

        /// <summary>
        /// Constructor - initializes the game
        /// </summary>
        public Game1()
        {
            _state = new GameState();
            
            // Create the graphics manager - this is required for any MonoGame project
            _state.Graphics = new GraphicsDeviceManager(this);
            
            // Set the graphics profile to HiDef for higher quality rendering and more advanced shader support
            _state.Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            
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
            _state.Graphics.PreferredBackBufferWidth = 800;  // Width in pixels
            _state.Graphics.PreferredBackBufferHeight = 600; // Height in pixels
            
            // Enable anti-aliasing for smoother edges (HiDef profile supports this)
            _state.Graphics.PreferMultiSampling = true;
            
            // Use 24-bit depth buffer with 8-bit stencil for better precision in 3D rendering
            _state.Graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            
            _state.Graphics.ApplyChanges();                  // Apply the new settings
            
            //===== BEGINNER NOTE: Rasterizer State =====//
            // The rasterizer controls how triangles are converted to pixels
            // By default, triangles facing away from the camera are not drawn (culled)
            // We disable culling here to see our tree from both sides
            _state.DoubleSidedRasterizerState = new RasterizerState
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
            // Create a new SpriteBatch for drawing 2D graphics
            _state.SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Create render targets for post-processing
            CreateRenderTargets();
            
            // Create the 1x1 pixel color textures
            CreateColorTextures();

            // Generate tree vertices and indices 
            CreateTreeModel();

            // Load the tree rendering shader
            _state.TriangleEffect = Content.Load<Effect>("TriangleShader");
            
            // Load the bloom post-processing shader
            _state.BloomEffect = Content.Load<Effect>("BloomShader");
        }
        
        /// <summary>
        /// Creates a 1x1 pixel texture with the specified color
        /// </summary>
        private Texture2D Create1x1Texture(Color color)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new[] { color });
            return texture;
        }
        
        /// <summary>
        /// Creates the 1x1 textures for the tree colors
        /// </summary>
        private void CreateColorTextures()
        {
            // Brown color for trunk
            _state.TrunkTexture = Create1x1Texture(new Color(139, 69, 19));
            
            // Green color for leaves
            _state.LeafTexture = Create1x1Texture(new Color(34, 139, 34));
        }
        
        /// <summary>
        /// Creates the render targets used for post-processing
        /// </summary>
        private void CreateRenderTargets()
        {
            // Get the current display size
            int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = GraphicsDevice.PresentationParameters.BackBufferHeight;
            
            // Create the main scene render target with depth buffer for 3D rendering
            _state.SceneRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color, // Standard color format for the main scene
                DepthFormat.Depth24Stencil8); // Need depth for 3D rendering
                
            // Create render targets for the multi-pass bloom effect pipeline
            // These don't need depth buffers since they're used for 2D post-processing
            _state.BloomExtractTarget = new RenderTarget2D(
                GraphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
                
            _state.BloomHorizontalBlurTarget = new RenderTarget2D(
                GraphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
                
            _state.BloomVerticalBlurTarget = new RenderTarget2D(
                GraphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
        }

        /// <summary>
        /// Creates a simple 3D tree model with a trunk and branches
        /// </summary>
        private void CreateTreeModel()
        {
            // Define the tree components
            (MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture[] vertices, short[] indices) = GenerateTree();
            _state.TotalVertices = vertices.Length;
            _state.TotalIndices = indices.Length;
            _state.TotalTriangles = indices.Length / 3;

            // Create vertex buffer
            _state.VertexBuffer = new VertexBuffer(
                GraphicsDevice,
                MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture.VertexDeclaration,
                _state.TotalVertices,
                BufferUsage.WriteOnly);
            _state.VertexBuffer.SetData(vertices);

            // Create index buffer
            _state.IndexBuffer = new IndexBuffer(
                GraphicsDevice,
                IndexElementSize.SixteenBits,
                _state.TotalIndices,
                BufferUsage.WriteOnly);
            _state.IndexBuffer.SetData(indices);
        }

        /// <summary>
        /// Generates vertices and indices for a simple tree
        /// </summary>
        private (MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture[] vertices, short[] indices) GenerateTree()
        {
            // Create a list to hold all vertices and indices
            var verticesList = new List<MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture>();
            var indicesList = new List<short>();
            
            // For 1x1 textures, we can use any texture coordinate
            // The color will be the same regardless of the UV
            Vector2 texCoord = new Vector2(0.5f, 0.5f);
            
            // 1. Create the trunk (a simple rectangular prism)
            Vector3 trunkBase = new Vector3(0, -0.5f, 0);
            float trunkWidth = 0.1f;
            float trunkHeight = 0.5f;
            
            // Trunk vertices (bottom square)
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(-trunkWidth, 0, -trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(trunkWidth, 0, -trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(trunkWidth, 0, trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(-trunkWidth, 0, trunkWidth), texCoord));
            
            // Trunk vertices (top square)
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(-trunkWidth, trunkHeight, -trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(trunkWidth, trunkHeight, -trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(trunkWidth, trunkHeight, trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(-trunkWidth, trunkHeight, trunkWidth), texCoord));
            
            // Trunk indices (6 faces, 2 triangles per face = 12 triangles)
            // Bottom face
            AddQuad(indicesList, 0, 1, 2, 3);
            
            // Top face
            AddQuad(indicesList, 7, 6, 5, 4);
            
            // Side faces
            AddQuad(indicesList, 0, 4, 5, 1);
            AddQuad(indicesList, 1, 5, 6, 2);
            AddQuad(indicesList, 2, 6, 7, 3);
            AddQuad(indicesList, 3, 7, 4, 0);
            
            // 2. Create pyramid-like foliage (a simple cone approximation)
            int baseVertex = verticesList.Count;
            float leafRadius = 0.4f;
            float leafHeight = 0.7f;
            Vector3 leafBase = trunkBase + new Vector3(0, trunkHeight, 0);
            Vector3 leafTop = leafBase + new Vector3(0, leafHeight, 0);
            
            // Add the top vertex of the cone
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(leafTop, texCoord));
            
            // Add vertices in a circle for the base of the cone
            int leafSegments = 8;
            for (int i = 0; i < leafSegments; i++)
            {
                float angle = i * MathHelper.TwoPi / leafSegments;
                float x = (float)System.Math.Sin(angle) * leafRadius;
                float z = (float)System.Math.Cos(angle) * leafRadius;
                
                verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(leafBase + new Vector3(x, 0, z), texCoord));
            }
            
            // Add triangles connecting the top to each segment of the circle
            for (int i = 0; i < leafSegments; i++)
            {
                int next = (i + 1) % leafSegments;
                indicesList.Add((short)baseVertex); // Top vertex
                indicesList.Add((short)(baseVertex + 1 + i));
                indicesList.Add((short)(baseVertex + 1 + next));
            }
            
            // Add the bottom face of the cone (optional, as it's not usually visible)
            for (int i = 1; i < leafSegments - 1; i++)
            {
                indicesList.Add((short)(baseVertex + 1));
                indicesList.Add((short)(baseVertex + 1 + i + 1));
                indicesList.Add((short)(baseVertex + 1 + i));
            }
            
            // Create a second layer of foliage above the first
            baseVertex = verticesList.Count;
            leafBase = leafBase + new Vector3(0, leafHeight * 0.3f, 0);
            leafTop = leafBase + new Vector3(0, leafHeight * 0.7f, 0);
            leafRadius *= 0.7f;
            
            // Add the top vertex of the second cone
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(leafTop, texCoord));
            
            // Add vertices in a circle for the base of the second cone
            for (int i = 0; i < leafSegments; i++)
            {
                float angle = i * MathHelper.TwoPi / leafSegments;
                float x = (float)System.Math.Sin(angle) * leafRadius;
                float z = (float)System.Math.Cos(angle) * leafRadius;
                
                verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(leafBase + new Vector3(x, 0, z), texCoord));
            }
            
            // Add triangles connecting the top to each segment of the circle
            for (int i = 0; i < leafSegments; i++)
            {
                int next = (i + 1) % leafSegments;
                indicesList.Add((short)baseVertex); // Top vertex
                indicesList.Add((short)(baseVertex + 1 + i));
                indicesList.Add((short)(baseVertex + 1 + next));
            }
            
            // Convert lists to arrays
            return (verticesList.ToArray(), indicesList.ToArray());
        }
        
        /// <summary>
        /// Helper method to add indices for a quad (two triangles)
        /// </summary>
        private void AddQuad(List<short> indices, int a, int b, int c, int d)
        {
            // First triangle
            indices.Add((short)a);
            indices.Add((short)b);
            indices.Add((short)c);
            
            // Second triangle
            indices.Add((short)a);
            indices.Add((short)c);
            indices.Add((short)d);
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
            if (keyboardState.IsKeyDown(Keys.P) && !_state.PreviousKeyboardState.IsKeyDown(Keys.P))
            {
                _state.UsePostProcessing = !_state.UsePostProcessing;
            }
            
            // Store current keyboard state for next frame
            _state.PreviousKeyboardState = keyboardState;
            
            //===== BEGINNER NOTE: Time-Based Animation =====//
            // To make animations smooth regardless of frame rate, we adjust
            // changes based on how much time has passed since the last frame
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update rotation angles at different speeds for each axis
            // This creates a more interesting animation than rotating around
            // just one axis would
            _state.RotationAngleX += 0.2f * elapsed;  // Rotate around X axis (slower)
            _state.RotationAngleY += 0.3f * elapsed;  // Rotate around Y axis (faster)
            
            //===== BEGINNER NOTE: Angle Management =====//
            // Keep angles between 0 and 2π (full circle in radians)
            // This isn't strictly necessary but prevents the values from
            // growing too large over time which could cause precision issues
            _state.RotationAngleX %= MathHelper.TwoPi;  // 2π ≈ 6.28 radians = 360 degrees
            _state.RotationAngleY %= MathHelper.TwoPi;
            _state.RotationAngleZ %= MathHelper.TwoPi;

            // Always call the base class Update method
            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game content to the screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void Draw(GameTime gameTime)
        {
            if (_state.UsePostProcessing)
            {
                // Apply the multi-pass bloom post-processing to the scene
                DrawSceneToRenderTarget();
                
                // Present the final post-processed scene to the screen
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);
                
                _state.SpriteBatch.Begin();
                _state.SpriteBatch.Draw(_state.SceneRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
                _state.SpriteBatch.End();
            }
            else
            {
                // Draw directly to the screen without post-processing for comparison
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.CornflowerBlue);
                DrawTree();
            }

            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Draws the tree scene with bloom post-processing using a multi-pass approach
        /// </summary>
        private void DrawSceneToRenderTarget()
        {
            // STEP 1: Render the tree scene to the main render target
            GraphicsDevice.SetRenderTarget(_state.SceneRenderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawTree();
            
            // STEP 2: Extract the blue colors from the scene into a separate render target
            GraphicsDevice.SetRenderTarget(_state.BloomExtractTarget);
            GraphicsDevice.Clear(Color.Black);
            
            // Configure the shader parameters for blue color extraction
            _state.BloomEffect.Parameters["InputTexture"].SetValue(_state.SceneRenderTarget);
            _state.BloomEffect.Parameters["BloomThreshold"].SetValue(_state.BloomThreshold);
            _state.BloomEffect.Parameters["TargetColor"].SetValue(_state.TargetBlueColor);
            _state.BloomEffect.Parameters["ColorSensitivity"].SetValue(_state.ColorSensitivity);
            _state.BloomEffect.Parameters["ScreenSize"].SetValue(new Vector2(
                GraphicsDevice.Viewport.Width, 
                GraphicsDevice.Viewport.Height));
            
            // Apply the blue bloom extraction shader technique
            _state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            _state.BloomEffect.CurrentTechnique = _state.BloomEffect.Techniques["BlueBloomExtract"];
            _state.BloomEffect.CurrentTechnique.Passes[0].Apply();
            _state.SpriteBatch.Draw(_state.SceneRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
            _state.SpriteBatch.End();
            
            // STEP 3: Apply horizontal Gaussian blur to the extracted blue colors
            GraphicsDevice.SetRenderTarget(_state.BloomHorizontalBlurTarget);
            GraphicsDevice.Clear(Color.Black);
            
            // Configure shader parameters for horizontal blur pass
            _state.BloomEffect.Parameters["InputTexture"].SetValue(_state.BloomExtractTarget);
            _state.BloomEffect.Parameters["BlurAmount"].SetValue(_state.BloomBlurAmount);
            _state.BloomEffect.Parameters["BlurDirection"].SetValue(new Vector2(1, 0)); // Horizontal direction
            
            // Apply the horizontal Gaussian blur
            _state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            _state.BloomEffect.CurrentTechnique = _state.BloomEffect.Techniques["GaussianBlur"];
            _state.BloomEffect.CurrentTechnique.Passes[0].Apply();
            _state.SpriteBatch.Draw(_state.BloomExtractTarget, GraphicsDevice.Viewport.Bounds, Color.White);
            _state.SpriteBatch.End();
            
            // STEP 4: Apply vertical Gaussian blur to complete the bloom blur effect
            GraphicsDevice.SetRenderTarget(_state.BloomVerticalBlurTarget);
            GraphicsDevice.Clear(Color.Black);
            
            // Configure shader parameters for vertical blur pass
            _state.BloomEffect.Parameters["InputTexture"].SetValue(_state.BloomHorizontalBlurTarget);
            _state.BloomEffect.Parameters["BlurDirection"].SetValue(new Vector2(0, 1)); // Vertical direction
            
            // Apply the vertical Gaussian blur
            _state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            _state.BloomEffect.CurrentTechnique = _state.BloomEffect.Techniques["GaussianBlur"];
            _state.BloomEffect.CurrentTechnique.Passes[0].Apply();
            _state.SpriteBatch.Draw(_state.BloomHorizontalBlurTarget, GraphicsDevice.Viewport.Bounds, Color.White);
            _state.SpriteBatch.End();
            
            // STEP 5: Combine the original scene with the blurred bloom effect
            GraphicsDevice.SetRenderTarget(_state.SceneRenderTarget);
            
            // Configure shader parameters for the final composition
            _state.BloomEffect.Parameters["BaseTexture"].SetValue(_state.SceneRenderTarget);
            _state.BloomEffect.Parameters["BloomTexture"].SetValue(_state.BloomVerticalBlurTarget);
            _state.BloomEffect.Parameters["BloomIntensity"].SetValue(_state.BloomIntensity);
            
            // Apply the bloom combine technique to produce the final image
            _state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            _state.BloomEffect.CurrentTechnique = _state.BloomEffect.Techniques["BloomCombine"];
            _state.BloomEffect.CurrentTechnique.Passes[0].Apply();
            _state.SpriteBatch.Draw(_state.SceneRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
            _state.SpriteBatch.End();
        }
        
        /// <summary>
        /// Draws the rotating tree using textures for coloring
        /// </summary>
        private void DrawTree()
        {
            // Enable depth testing to correctly handle overlapping triangles
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = _state.DoubleSidedRasterizerState;

            // Tell the GPU which vertices and indices to use for drawing
            GraphicsDevice.SetVertexBuffer(_state.VertexBuffer);
            GraphicsDevice.Indices = _state.IndexBuffer;

            // Position the tree slightly back from the camera for better viewing
            Matrix world = Matrix.CreateRotationX(_state.RotationAngleX) * 
                          Matrix.CreateRotationY(_state.RotationAngleY) * 
                          Matrix.CreateTranslation(0, 0, -0.5f);
            
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
            _state.TriangleEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            
            // First draw the trunk
            _state.TriangleEffect.Parameters["ModelTexture"].SetValue(_state.TrunkTexture);
            
            foreach (EffectPass pass in _state.TriangleEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                
                // Draw the trunk portion (first 12 triangles - 6 faces with 2 triangles each)
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,       // Base vertex offset
                    0,       // Start index
                    12       // Number of triangles for the trunk
                );
            }
            
            // Then draw the foliage
            _state.TriangleEffect.Parameters["ModelTexture"].SetValue(_state.LeafTexture);
            
            foreach (EffectPass pass in _state.TriangleEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                
                // Draw the foliage portion (remaining triangles)
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,            // Base vertex offset
                    12 * 3,       // Start index (after trunk triangles)
                    _state.TotalTriangles - 12  // Number of triangles for foliage
                );
            }
        }

        /// <summary>
        /// Clean up resources when the game is shutting down
        /// </summary>
        protected override void UnloadContent()
        {
            // Dispose GPU resources to prevent memory leaks
            _state.VertexBuffer?.Dispose();
            _state.IndexBuffer?.Dispose();
            _state.DoubleSidedRasterizerState?.Dispose();
            
            // Dispose textures
            _state.TrunkTexture?.Dispose();
            _state.LeafTexture?.Dispose();
            
            // Dispose all render targets used in the post-processing pipeline
            _state.SceneRenderTarget?.Dispose();
            _state.BloomExtractTarget?.Dispose();
            _state.BloomHorizontalBlurTarget?.Dispose();
            _state.BloomVerticalBlurTarget?.Dispose();
            
            base.UnloadContent();
        }
    }
}
