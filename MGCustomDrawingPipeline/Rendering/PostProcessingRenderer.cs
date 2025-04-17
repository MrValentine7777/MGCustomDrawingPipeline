using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
            // Ensure shader is loaded properly
            if (state.BloomEffect == null)
            {
                System.Diagnostics.Debug.WriteLine("BloomShader not loaded properly");
                return;
            }

            // Check if techniques exist by trying to access them directly instead of using Contains
            try
            {
                // Just verify that these techniques exist by trying to access them
                var sunlightTechnique = state.BloomEffect.Techniques["SunlightBloomExtract"];
                var blurTechnique = state.BloomEffect.Techniques["GaussianBlur"];
                var combineTechnique = state.BloomEffect.Techniques["BloomCombine"];
                
                System.Diagnostics.Debug.WriteLine("Bloom shader techniques verified");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bloom shader missing required techniques: {ex.Message}");
                return;
            }

            // Add ScreenSize parameter if it wasn't being set
            Vector2 screenSize = new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            try
            {
                // STEP 1: Render the tree scene to the main render target
                graphicsDevice.SetRenderTarget(state.SceneRenderTarget);
                graphicsDevice.Clear(Color.CornflowerBlue);
                treeRenderer.DrawTree(graphicsDevice, state);
                
                // STEP 2: Extract the sunlight-affected areas from the scene
                graphicsDevice.SetRenderTarget(state.BloomExtractTarget);
                graphicsDevice.Clear(Color.Black);
                
                // Configure the shader parameters for sunlight extraction
                // Add safety check for parameters
                var bloomEffect = state.BloomEffect;
                try
                {
                    bloomEffect.Parameters["InputTexture"].SetValue(state.SceneRenderTarget);
                    bloomEffect.Parameters["BloomThreshold"].SetValue(state.BloomThreshold * 0.5f);
                    bloomEffect.Parameters["TargetColor"].SetValue(state.SunlightColor);
                    bloomEffect.Parameters["ColorSensitivity"].SetValue(state.ColorSensitivity * 1.2f);
                    bloomEffect.Parameters["ScreenSize"].SetValue(screenSize);
                    
                    System.Diagnostics.Debug.WriteLine("Bloom extraction parameters set successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting bloom extraction parameters: {ex.Message}");
                }
                
                // Apply the sunlight bloom extraction shader technique
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["SunlightBloomExtract"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.SceneRenderTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                // STEP 3: Apply horizontal Gaussian blur
                graphicsDevice.SetRenderTarget(state.BloomHorizontalBlurTarget);
                graphicsDevice.Clear(Color.Black);
                
                try
                {
                    bloomEffect.Parameters["InputTexture"].SetValue(state.BloomExtractTarget);
                    bloomEffect.Parameters["BlurAmount"].SetValue(state.BloomBlurAmount * 1.2f);
                    bloomEffect.Parameters["BlurDirection"].SetValue(new Vector2(1, 0));
                    bloomEffect.Parameters["ScreenSize"].SetValue(screenSize);
                    
                    System.Diagnostics.Debug.WriteLine("Horizontal blur parameters set successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting horizontal blur parameters: {ex.Message}");
                }
                
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["GaussianBlur"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.BloomExtractTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                // STEP 4: Apply vertical Gaussian blur
                graphicsDevice.SetRenderTarget(state.BloomVerticalBlurTarget);
                graphicsDevice.Clear(Color.Black);
                
                try
                {
                    bloomEffect.Parameters["InputTexture"].SetValue(state.BloomHorizontalBlurTarget);
                    bloomEffect.Parameters["BlurDirection"].SetValue(new Vector2(0, 1));
                    bloomEffect.Parameters["ScreenSize"].SetValue(screenSize);
                    
                    System.Diagnostics.Debug.WriteLine("Vertical blur parameters set successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting vertical blur parameters: {ex.Message}");
                }
                
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["GaussianBlur"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.BloomHorizontalBlurTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                // STEP 5: Combine the original scene with the blurred bloom effect
                graphicsDevice.SetRenderTarget(state.SceneRenderTarget);
                
                try
                {
                    bloomEffect.Parameters["BaseTexture"].SetValue(state.SceneRenderTarget);
                    bloomEffect.Parameters["BloomTexture"].SetValue(state.BloomVerticalBlurTarget);
                    bloomEffect.Parameters["BloomIntensity"].SetValue(state.BloomIntensity * 1.3f);
                    
                    System.Diagnostics.Debug.WriteLine("Bloom combine parameters set successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting bloom combine parameters: {ex.Message}");
                }
                
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["BloomCombine"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.SceneRenderTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                System.Diagnostics.Debug.WriteLine("Bloom post-processing completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during bloom post-processing: {ex.Message}");
            }
        }
    }
}
