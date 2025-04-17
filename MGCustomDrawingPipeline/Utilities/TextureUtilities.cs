using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGCustomDrawingPipeline.Utilities
{
    /// <summary>
    /// Utility methods for working with textures
    /// 
    /// ===== BEGINNER'S GUIDE: TEXTURE CREATION =====
    /// 
    /// Textures can come from various sources in game development:
    /// 
    /// 1. Loaded from files (PNG, JPG, etc.) - Most common for complex textures
    /// 2. Created at runtime - Useful for procedural effects or simple colors
    /// 3. Rendered during gameplay - Used for things like security cameras, mirrors, etc.
    /// 
    /// This utility class demonstrates the second approach: runtime creation.
    /// Creating textures at runtime allows for dynamic content that can respond
    /// to game conditions, user preferences, or procedural generation.
    /// 
    /// While a 1x1 texture is extremely simple, the same technique can be extended
    /// to create more complex procedural textures like gradients, noise patterns,
    /// or even complete procedural materials.
    /// </summary>
    public static class TextureUtilities
    {
        /// <summary>
        /// Creates a 1x1 pixel texture with the specified color
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to create the texture with</param>
        /// <param name="color">The color to use for the texture</param>
        /// <returns>A new 1x1 texture of the specified color</returns>
        public static Texture2D Create1x1Texture(GraphicsDevice graphicsDevice, Color color)
        {
            //===== BEGINNER'S GUIDE: TEXTURE DATA MANIPULATION =====//
            
            // Create a new 1x1 texture (width=1, height=1)
            // This allocates the texture resource on the GPU
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            
            // Create a color array to hold our pixel data
            // For a 1x1 texture, this is just a single color value
            // For larger textures, this array would hold multiple color values
            Color[] colorData = new[] { color };
            
            // Upload the color data to the GPU texture
            // This transfers our color from CPU memory to GPU texture memory
            texture.SetData(colorData);
            
            return texture;
        }
    }
}
