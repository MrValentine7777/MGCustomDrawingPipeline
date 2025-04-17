using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MGCustomDrawingPipeline.Utilities;
using MGCustomDrawingPipeline.TextureManagement;
using MGCustomDrawingPipeline.Rendering;
using MGCustomDrawingPipeline.Input;
using MGCustomDrawingPipeline.Animation;
using System;

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
        /// Handles rendering of the tree model
        /// </summary>
        private TreeRenderer _treeRenderer;

        /// <summary>
        /// Constructor - initializes the game
        /// </summary>
        public Game1()
        {
            _state = new GameState();
            _treeRenderer = new TreeRenderer();
            
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

            // Create a wireframe rasterizer state for showing the model structure
            _state.WireframeRasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,  // Show both front and back faces
                FillMode = FillMode.WireFrame  // Only draw the edges of triangles
            };

            // Always call the base class Initialize method
            base.Initialize();
        }

        /// <summary>
        /// Loads all content needed by the game (textures, models, sounds, etc.)
        /// </summary>
        protected override void LoadContent()
        {
            try
            {
                // Create a new SpriteBatch for drawing 2D graphics
                _state.SpriteBatch = new SpriteBatch(GraphicsDevice);

                // Create render targets for post-processing
                RenderTargetManager.CreateRenderTargets(GraphicsDevice, _state);
                
                // Create the 1x1 pixel color textures
                ColorTextureCreator.CreateTreeColorTextures(GraphicsDevice, _state);

                // Generate tree vertices and indices 
                TreeModelGenerator.CreateTreeModel(GraphicsDevice, _state);

                // Load the tree rendering shader
                _state.TriangleEffect = Content.Load<Effect>("TriangleShader");
                if (_state.TriangleEffect == null)
                    throw new InvalidOperationException("Failed to load TriangleShader effect");
                
                // Load and validate the bloom post-processing shader
                _state.BloomEffect = Content.Load<Effect>("BloomShader");
                if (_state.BloomEffect == null)
                    throw new InvalidOperationException("Failed to load BloomShader effect");
                    
                // Verify that required techniques exist
                try
                {
                    var sunlightTechnique = _state.BloomEffect.Techniques["SunlightBloomExtract"];
                    var blurTechnique = _state.BloomEffect.Techniques["GaussianBlur"];
                    var combineTechnique = _state.BloomEffect.Techniques["BloomCombine"];
                    System.Diagnostics.Debug.WriteLine("All shader techniques verified successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Shader technique verification failed: {ex.Message}");
                }
                
                // Log success message
                System.Diagnostics.Debug.WriteLine("All shaders and content loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading content: {ex.Message}");
                // Consider showing a user-friendly error message
            }
        }

        /// <summary>
        /// Updates the game state each frame - handles input, physics, AI, etc.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void Update(GameTime gameTime)
        {
            // Process keyboard input
            InputManager.ProcessInput(_state);
            
            // Update animation based on elapsed time
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ModelAnimator.UpdateAnimation(_state, elapsed);

            // Always call the base class Update method
            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game content to the screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void Draw(GameTime gameTime)
        {
            try
            {
                if (_state.UsePostProcessing)
                {
                    // set the render target to the scene render target
                    GraphicsDevice.SetRenderTarget(_state.SceneRenderTarget);
                    GraphicsDevice.Clear(Color.Transparent);
                    // Apply the multi-pass bloom post-processing to the scene
                    PostProcessingRenderer.DrawSceneToRenderTarget(GraphicsDevice, _state, _treeRenderer);
                    
                    // Present the final post-processed scene to the screen
                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.Transparent);
                    
                    _state.SpriteBatch.Begin();
                    _state.SpriteBatch.Draw(_state.SceneRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
                    _state.SpriteBatch.End();
                }
                else
                {
                    // Draw directly to the screen without post-processing for comparison
                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.Transparent);
                    _treeRenderer.DrawTree(GraphicsDevice, _state);
                }
            }
            catch (Exception ex)
            {
                // Log the error - in a real app you'd want to use a proper logging system
                System.Diagnostics.Debug.WriteLine($"Rendering error: {ex.Message}");
                
                // Fall back to basic rendering if post-processing fails
                if (_state.UsePostProcessing)
                {
                    _state.UsePostProcessing = false;
                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    _treeRenderer.DrawTree(GraphicsDevice, _state);
                }
            }

            base.Draw(gameTime);
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
            _state.WireframeRasterizerState?.Dispose();
            
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

