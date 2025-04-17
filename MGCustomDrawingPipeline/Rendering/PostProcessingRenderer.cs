using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGCustomDrawingPipeline.Rendering
{
    /// <summary>
    /// Handles post-processing rendering effects like bloom
    /// </summary>
    public class PostProcessingRenderer
    {
        /// <summary>
        /// Draws the tree scene with bloom post-processing using a multi-pass approach
        /// </summary>
        public static void DrawSceneToRenderTarget(GraphicsDevice graphicsDevice, GameState state, TreeRenderer treeRenderer)
        {
            // Use the wireframe setting from state for consistent rendering in both standard and post-processed modes
            // (No actual code changes needed here since this method calls TreeRenderer.DrawTree which already uses the wireframe setting)

            // STEP 1: Render the tree scene to the main render target
            graphicsDevice.SetRenderTarget(state.SceneRenderTarget);
            graphicsDevice.Clear(Color.CornflowerBlue);
            treeRenderer.DrawTree(graphicsDevice, state);
            
            // STEP 2: Extract the sunlight-affected areas from the scene
            graphicsDevice.SetRenderTarget(state.BloomExtractTarget);
            graphicsDevice.Clear(Color.Black);
            
            // Configure the shader parameters for sunlight extraction
            state.BloomEffect.Parameters["InputTexture"].SetValue(state.SceneRenderTarget);
            state.BloomEffect.Parameters["BloomThreshold"].SetValue(state.BloomThreshold * 0.5f); // Lower threshold for sunlight bloom
            state.BloomEffect.Parameters["TargetColor"].SetValue(state.SunlightColor); // Use the sunlight color for extraction
            state.BloomEffect.Parameters["ColorSensitivity"].SetValue(state.ColorSensitivity * 1.2f); // Increase sensitivity for sunlight
            state.BloomEffect.Parameters["ScreenSize"].SetValue(new Vector2(
                graphicsDevice.Viewport.Width, 
                graphicsDevice.Viewport.Height));
            
            // Apply the sunlight bloom extraction shader technique
            state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            state.BloomEffect.CurrentTechnique = state.BloomEffect.Techniques["SunlightBloomExtract"];
            state.BloomEffect.CurrentTechnique.Passes[0].Apply();
            state.SpriteBatch.Draw(state.SceneRenderTarget, graphicsDevice.Viewport.Bounds, Color.White);
            state.SpriteBatch.End();
            
            // STEP 3: Apply horizontal Gaussian blur to the extracted sunlight colors
            graphicsDevice.SetRenderTarget(state.BloomHorizontalBlurTarget);
            graphicsDevice.Clear(Color.Black);
            
            // Configure shader parameters for horizontal blur pass
            state.BloomEffect.Parameters["InputTexture"].SetValue(state.BloomExtractTarget);
            state.BloomEffect.Parameters["BlurAmount"].SetValue(state.BloomBlurAmount * 1.2f); // Slightly stronger blur for sunlight glow
            state.BloomEffect.Parameters["BlurDirection"].SetValue(new Vector2(1, 0)); // Horizontal direction
            
            // Apply the horizontal Gaussian blur
            state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            state.BloomEffect.CurrentTechnique = state.BloomEffect.Techniques["GaussianBlur"];
            state.BloomEffect.CurrentTechnique.Passes[0].Apply();
            state.SpriteBatch.Draw(state.BloomExtractTarget, graphicsDevice.Viewport.Bounds, Color.White);
            state.SpriteBatch.End();
            
            // STEP 4: Apply vertical Gaussian blur to complete the bloom blur effect
            graphicsDevice.SetRenderTarget(state.BloomVerticalBlurTarget);
            graphicsDevice.Clear(Color.Black);
            
            // Configure shader parameters for vertical blur pass
            state.BloomEffect.Parameters["InputTexture"].SetValue(state.BloomHorizontalBlurTarget);
            state.BloomEffect.Parameters["BlurDirection"].SetValue(new Vector2(0, 1)); // Vertical direction
            
            // Apply the vertical Gaussian blur
            state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            state.BloomEffect.CurrentTechnique = state.BloomEffect.Techniques["GaussianBlur"];
            state.BloomEffect.CurrentTechnique.Passes[0].Apply();
            state.SpriteBatch.Draw(state.BloomHorizontalBlurTarget, graphicsDevice.Viewport.Bounds, Color.White);
            state.SpriteBatch.End();
            
            // STEP 5: Combine the original scene with the blurred bloom effect
            graphicsDevice.SetRenderTarget(state.SceneRenderTarget);
            
            // Configure shader parameters for the final composition
            state.BloomEffect.Parameters["BaseTexture"].SetValue(state.SceneRenderTarget);
            state.BloomEffect.Parameters["BloomTexture"].SetValue(state.BloomVerticalBlurTarget);
            state.BloomEffect.Parameters["BloomIntensity"].SetValue(state.BloomIntensity * 1.3f); // Enhance sunlight bloom intensity
            
            // Apply the bloom combine technique to produce the final image
            state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            state.BloomEffect.CurrentTechnique = state.BloomEffect.Techniques["BloomCombine"];
            state.BloomEffect.CurrentTechnique.Passes[0].Apply();
            state.SpriteBatch.Draw(state.SceneRenderTarget, graphicsDevice.Viewport.Bounds, Color.White);
            state.SpriteBatch.End();
        }
    }
}
