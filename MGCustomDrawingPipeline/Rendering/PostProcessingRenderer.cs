using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MGCustomDrawingPipeline.Rendering
{
    /// <summary>
    /// Handles post-processing rendering effects like bloom.
    /// 
    /// Post-processing refers to image effects applied after the main 3D scene is rendered.
    /// These effects modify the final image to create visual enhancements like glow,
    /// color grading, blur, or other artistic effects.
    /// </summary>
    public class PostProcessingRenderer
    {
        /// <summary>
        /// Draws the tree scene with bloom post-processing using a multi-pass approach.
        /// 
        /// This method implements a complete bloom post-processing pipeline:
        /// 1. Renders the tree to the main scene render target
        /// 2. Extracts bright areas affected by sunlight to a separate target
        /// 3. Applies horizontal Gaussian blur to the extracted areas
        /// 4. Applies vertical Gaussian blur to create a smooth glow
        /// 5. Combines the original scene with the blurred glow
        /// 
        /// Bloom is a visual effect that simulates how very bright objects appear to glow
        /// in real life due to the limitations of cameras and the human eye. It enhances 
        /// the perception of brightness by adding a glow around light sources or bright objects.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use for rendering</param>
        /// <param name="state">The current game state containing rendering parameters</param>
        /// <param name="treeRenderer">The renderer that draws the tree model</param>
        public static void DrawSceneToRenderTarget(GraphicsDevice graphicsDevice, GameState state, TreeRenderer treeRenderer)
        {
            // Ensure shader is loaded properly
            if (state.BloomEffect == null)
            {
                System.Diagnostics.Debug.WriteLine("BloomShader not loaded properly");
                return;
            }

            // Check if techniques exist by trying to access them directly
            // Shader techniques are different rendering methods defined in the shader
            // Each technique serves a specific purpose in our multi-pass bloom effect
            try
            {
                // Verify that required techniques exist in the shader
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

            // Get screen dimensions for proper sampling in the shader
            // This ensures the shader knows the exact size of the texture it's working with
            // which is critical for correct pixel sampling and blur operations
            Vector2 screenSize = new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            try
            {
                // STEP 1: Render the tree scene to the main render target
                // This creates our base image that we'll apply effects to
                graphicsDevice.SetRenderTarget(state.SceneRenderTarget);
                graphicsDevice.Clear(Color.CornflowerBlue);
                treeRenderer.DrawTree(graphicsDevice, state);
                
                // STEP 2: Extract the sunlight-affected areas from the scene
                // This isolates the areas that should glow based on color and brightness
                // The extraction shader identifies pixels that match our target sunlight color
                graphicsDevice.SetRenderTarget(state.BloomExtractTarget);
                graphicsDevice.Clear(Color.Black);
                
                // Configure the shader parameters for sunlight extraction
                // Each parameter controls how the bloom effect is applied
                var bloomEffect = state.BloomEffect;
                try
                {
                    // Set the input texture to our main scene
                    bloomEffect.Parameters["InputTexture"].SetValue(state.SceneRenderTarget);
                    
                    // Set the bloom threshold - pixels dimmer than this won't bloom
                    // We use a lower threshold for sunlight to capture more of its glow
                    bloomEffect.Parameters["BloomThreshold"].SetValue(state.BloomThreshold * 0.5f);
                    
                    // Set the target color we're looking for (warm sunlight tone)
                    bloomEffect.Parameters["TargetColor"].SetValue(state.SunlightColor);
                    
                    // Set color sensitivity - controls how strictly we match the target color
                    // Higher sensitivity means more colors around our target will glow
                    bloomEffect.Parameters["ColorSensitivity"].SetValue(state.ColorSensitivity * 1.2f);
                    
                    // Pass the screen dimensions to the shader for correct sampling
                    bloomEffect.Parameters["ScreenSize"].SetValue(screenSize);
                    
                    System.Diagnostics.Debug.WriteLine("Bloom extraction parameters set successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting bloom extraction parameters: {ex.Message}");
                }
                
                // Apply the sunlight bloom extraction shader technique
                // This draws our scene using the extraction shader to isolate bright areas
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["SunlightBloomExtract"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.SceneRenderTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                // STEP 3: Apply horizontal Gaussian blur
                // This is the first pass of our two-pass blur algorithm
                // Gaussian blur creates a soft, natural-looking glow effect
                graphicsDevice.SetRenderTarget(state.BloomHorizontalBlurTarget);
                graphicsDevice.Clear(Color.Black);
                
                try
                {
                    // Set input to the extracted bright areas
                    bloomEffect.Parameters["InputTexture"].SetValue(state.BloomExtractTarget);
                    
                    // Set blur amount - controls how wide the blur spreads
                    bloomEffect.Parameters["BlurAmount"].SetValue(state.BloomBlurAmount * 1.2f);
                    
                    // Set blur direction to horizontal (1,0)
                    bloomEffect.Parameters["BlurDirection"].SetValue(new Vector2(1, 0));
                    
                    // Pass screen dimensions for proper sampling
                    bloomEffect.Parameters["ScreenSize"].SetValue(screenSize);
                    
                    System.Diagnostics.Debug.WriteLine("Horizontal blur parameters set successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting horizontal blur parameters: {ex.Message}");
                }
                
                // Apply the horizontal blur shader
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["GaussianBlur"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.BloomExtractTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                // STEP 4: Apply vertical Gaussian blur
                // Second pass of blur, operating on result of horizontal blur
                // Two-pass blur is more efficient than a single-pass 2D blur
                graphicsDevice.SetRenderTarget(state.BloomVerticalBlurTarget);
                graphicsDevice.Clear(Color.Black);
                
                try
                {
                    // Set input to the horizontally blurred image
                    bloomEffect.Parameters["InputTexture"].SetValue(state.BloomHorizontalBlurTarget);
                    
                    // Set blur direction to vertical (0,1)
                    bloomEffect.Parameters["BlurDirection"].SetValue(new Vector2(0, 1));
                    
                    // Pass screen dimensions for proper sampling
                    bloomEffect.Parameters["ScreenSize"].SetValue(screenSize);
                    
                    System.Diagnostics.Debug.WriteLine("Vertical blur parameters set successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting vertical blur parameters: {ex.Message}");
                }
                
                // Apply the vertical blur shader
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["GaussianBlur"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.BloomHorizontalBlurTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                // STEP 5: Combine the original scene with the blurred bloom effect
                // This creates the final image with the glow effect applied
                graphicsDevice.SetRenderTarget(state.SceneRenderTarget);
                
                try
                {
                    // Set the original scene as the base texture
                    bloomEffect.Parameters["BaseTexture"].SetValue(state.SceneRenderTarget);
                    
                    // Set the blurred glow as the bloom texture to be added
                    bloomEffect.Parameters["BloomTexture"].SetValue(state.BloomVerticalBlurTarget);
                    
                    // Set bloom intensity - controls how strong the glow effect is
                    bloomEffect.Parameters["BloomIntensity"].SetValue(state.BloomIntensity * 1.3f);
                    
                    System.Diagnostics.Debug.WriteLine("Bloom combine parameters set successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting bloom combine parameters: {ex.Message}");
                }
                
                // Apply the combination shader to produce final image
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
