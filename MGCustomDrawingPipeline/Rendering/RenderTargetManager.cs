using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGCustomDrawingPipeline.Rendering
{
    /// <summary>
    /// Manages rendering targets for post-processing effects
    /// 
    /// ===== BEGINNER'S GUIDE: RENDER TARGETS =====
    /// 
    /// Render targets are special textures that we can draw to instead of drawing directly
    /// to the screen. Think of them like canvases that we can paint on and then manipulate.
    /// 
    /// In typical game rendering, everything is drawn directly to the screen (the back buffer).
    /// With render targets, we can:
    /// 
    /// 1. Render the scene to a texture instead of the screen
    /// 2. Apply effects or modifications to that texture
    /// 3. Use the modified texture in further rendering steps
    /// 4. Eventually display the final result on screen
    /// 
    /// This "multi-pass rendering" technique is essential for advanced visual effects
    /// like bloom, blur, shadow mapping, and many other post-processing effects.
    /// </summary>
    public class RenderTargetManager
    {   
        /// <summary>
        /// Creates the render targets used for post-processing
        /// 
        /// This method initializes four render targets:
        /// 1. SceneRenderTarget - Stores the complete 3D scene
        /// 2. BloomExtractTarget - Stores the extracted bright areas
        /// 3. BloomHorizontalBlurTarget - Stores the horizontally blurred image
        /// 4. BloomVerticalBlurTarget - Stores the fully blurred bloom image
        /// 
        /// Each render target is sized to match the current screen dimensions.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device for creating render targets</param>
        /// <param name="state">The game state to store the created render targets</param>
        public static void CreateRenderTargets(GraphicsDevice graphicsDevice, GameState state)
        {
            // Get the current display size from the graphics device
            // This ensures our render targets match the screen dimensions
            int width = graphicsDevice.PresentationParameters.BackBufferWidth;
            int height = graphicsDevice.PresentationParameters.BackBufferHeight;
            
            // ===== STEP 1: CREATE MAIN SCENE RENDER TARGET =====
            // This target will hold our complete 3D scene and needs a depth buffer
            // for correct rendering of overlapping 3D objects
            state.SceneRenderTarget = new RenderTarget2D(
                graphicsDevice,      // The graphics device to create the render target on
                width,               // Width matching the screen
                height,              // Height matching the screen
                false,               // No mipmaps needed for post-processing
                SurfaceFormat.Rgba64, // Higher precision color data for better quality
                DepthFormat.Depth24Stencil8); // Need depth buffer for 3D rendering
                
            // ===== STEP 2: CREATE BLOOM EXTRACTION TARGET =====
            // This target will store the bright areas extracted from the scene
            // It doesn't need a depth buffer since we're just doing 2D image processing
            state.BloomExtractTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Rgba64, // Higher precision color data for better quality effects
                DepthFormat.None);    // No depth buffer needed for 2D post-processing
                
            // ===== STEP 3: CREATE HORIZONTAL BLUR TARGET =====
            // This target will store the intermediate result after horizontal blur
            state.BloomHorizontalBlurTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Rgba64, // Higher precision color data
                DepthFormat.None);    // No depth buffer needed
                
            // ===== STEP 4: CREATE VERTICAL BLUR TARGET =====
            // This target will store the final bloom result after vertical blur
            state.BloomVerticalBlurTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Rgba64, // Higher precision color data
                DepthFormat.None);    // No depth buffer needed
                
            // Now all render targets are created and stored in the game state
            // They're ready to be used in our multi-pass rendering pipeline
        }
    }
}
