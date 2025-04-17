using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGCustomDrawingPipeline.Rendering
{
    /// <summary>
    /// Manages rendering targets for post-processing effects
    /// </summary>
    public class RenderTargetManager
    {
        /// <summary>
        /// Creates the render targets used for post-processing
        /// </summary>
        public static void CreateRenderTargets(GraphicsDevice graphicsDevice, GameState state)
        {
            // Get the current display size
            int width = graphicsDevice.PresentationParameters.BackBufferWidth;
            int height = graphicsDevice.PresentationParameters.BackBufferHeight;
            
            // Create the main scene render target with depth buffer for 3D rendering
            state.SceneRenderTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color, // Standard color format for the main scene
                DepthFormat.Depth24Stencil8); // Need depth for 3D rendering
                
            // Create render targets for the multi-pass bloom effect pipeline
            // These don't need depth buffers since they're used for 2D post-processing
            state.BloomExtractTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
                
            state.BloomHorizontalBlurTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
                
            state.BloomVerticalBlurTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
        }
    }
}
