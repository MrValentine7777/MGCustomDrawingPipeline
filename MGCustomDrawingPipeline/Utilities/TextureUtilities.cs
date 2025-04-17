using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGCustomDrawingPipeline.Utilities
{
    /// <summary>
    /// Utility methods for working with textures
    /// </summary>
    public static class TextureUtilities
    {
        /// <summary>
        /// Creates a 1x1 pixel texture with the specified color
        /// </summary>
        public static Texture2D Create1x1Texture(GraphicsDevice graphicsDevice, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { color });
            return texture;
        }
    }
}
