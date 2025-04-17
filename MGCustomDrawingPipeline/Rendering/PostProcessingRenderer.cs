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
            //===== BEGINNER'S GUIDE: POST-PROCESSING EFFECTS =====//
            
            // First, check if our shader is properly loaded
            // Post-processing effects require shader programs to work
            if (state.BloomEffect == null)
            {
                System.Diagnostics.Debug.WriteLine("BloomShader not loaded properly");
                return;
            }

            // A shader might contain multiple "techniques" which are like different recipes
            // for rendering effects. We need to verify that our shader has all the techniques
            // we'll need for our bloom effect.
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
                //===== STEP 1: RENDER THE SCENE =====//
                // First, we need to draw our 3D scene to a special texture called a render target
                // Think of it like taking a photo of our scene that we can then manipulate
                graphicsDevice.SetRenderTarget(state.SceneRenderTarget);
                graphicsDevice.Clear(Color.CornflowerBlue);
                treeRenderer.DrawTree(graphicsDevice, state);
                
                //===== STEP 2: EXTRACT BRIGHT AREAS =====//
                // Now we identify which parts of the scene should glow
                // We're targeting areas that match our sunlight color
                graphicsDevice.SetRenderTarget(state.BloomExtractTarget);
                graphicsDevice.Clear(Color.Black);
                
                // Configure the shader parameters for sunlight extraction
                // These tell the shader what to look for when deciding what should glow
                var bloomEffect = state.BloomEffect;
                try
                {
                    // Input texture is the scene we just rendered
                    bloomEffect.Parameters["InputTexture"].SetValue(state.SceneRenderTarget);
                    
                    // Threshold determines how bright a pixel needs to be to glow
                    // We use a lower threshold for sunlight to capture more of its glow
                    bloomEffect.Parameters["BloomThreshold"].SetValue(state.BloomThreshold * 0.5f);
                    
                    // Target color we're looking for (warm sunlight tone)
                    // The shader will make pixels close to this color glow more
                    bloomEffect.Parameters["TargetColor"].SetValue(state.SunlightColor);
                    
                    // Color sensitivity controls how strict the color matching is
                    // Higher sensitivity means more colors close to our target will glow
                    bloomEffect.Parameters["ColorSensitivity"].SetValue(state.ColorSensitivity * 1.2f);
                    
                    // Pass screen dimensions so the shader can correctly sample texels
                    bloomEffect.Parameters["ScreenSize"].SetValue(screenSize);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting bloom extraction parameters: {ex.Message}");
                    return;
                }
                
                // Apply the extraction shader to create an image containing only
                // the bright parts that match our sunlight color
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["SunlightBloomExtract"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.SceneRenderTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                //===== STEP 3: HORIZONTAL BLUR =====//
                // To create a convincing glow, we need to blur the extracted bright areas
                // We do this in two passes for better quality and performance
                // First pass blurs horizontally (left-right)
                graphicsDevice.SetRenderTarget(state.BloomHorizontalBlurTarget);
                graphicsDevice.Clear(Color.Black);
                
                try
                {
                    // Set input to the extracted bright areas
                    bloomEffect.Parameters["InputTexture"].SetValue(state.BloomExtractTarget);
                    
                    // Set blur amount - controls how wide the blur spreads
                    bloomEffect.Parameters["BlurAmount"].SetValue(state.BloomBlurAmount * 1.2f);
                    
                    // Set blur direction to horizontal (1,0)
                    // This vector tells the shader which direction to sample pixels
                    bloomEffect.Parameters["BlurDirection"].SetValue(new Vector2(1, 0));
                    
                    // Pass screen dimensions for proper sampling
                    bloomEffect.Parameters["ScreenSize"].SetValue(screenSize);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting horizontal blur parameters: {ex.Message}");
                    return;
                }
                
                // Apply the horizontal blur shader
                // This creates a horizontally blurred version of our extracted bright areas
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["GaussianBlur"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.BloomExtractTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                //===== STEP 4: VERTICAL BLUR =====//
                // Second pass blurs vertically (up-down)
                // When combined with the horizontal blur, this creates a smooth radial glow
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
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting vertical blur parameters: {ex.Message}");
                    return;
                }
                
                // Apply the vertical blur shader
                state.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                bloomEffect.CurrentTechnique = bloomEffect.Techniques["GaussianBlur"];
                bloomEffect.CurrentTechnique.Passes[0].Apply();
                state.SpriteBatch.Draw(state.BloomHorizontalBlurTarget, graphicsDevice.Viewport.Bounds, Color.White);
                state.SpriteBatch.End();
                
                //===== STEP 5: COMBINE ORIGINAL SCENE WITH BLOOM =====//
                // Finally, we combine the original scene with the blurred glow effect
                // This creates the final image with the glow effect applied
                graphicsDevice.SetRenderTarget(state.SceneRenderTarget);
                
                try
                {
                    // Set the original scene as the base image
                    bloomEffect.Parameters["BaseTexture"].SetValue(state.SceneRenderTarget);
                    
                    // Set the blurred glow as the bloom texture to be added on top
                    bloomEffect.Parameters["BloomTexture"].SetValue(state.BloomVerticalBlurTarget);
                    
                    // Set bloom intensity - controls how strong the glow effect is
                    bloomEffect.Parameters["BloomIntensity"].SetValue(state.BloomIntensity * 1.3f);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting bloom combine parameters: {ex.Message}");
                    return;
                }
                
                // Apply the combination shader to produce the final image
                // This adds the glow effect on top of our original scene
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
