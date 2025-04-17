using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MGCustomDrawingPipeline.Utilities;

namespace MGCustomDrawingPipeline.TextureManagement
{
    /// <summary>
    /// Handles creation of color textures for game objects
    /// </summary>
    public static class ColorTextureCreator
    {
        /// <summary>
        /// Creates the 1x1 textures for the tree colors and assigns them to the game state
        /// </summary>
        public static void CreateTreeColorTextures(GraphicsDevice graphicsDevice, GameState state)
        {
            // Brown color for trunk
            state.TrunkTexture = TextureUtilities.Create1x1Texture(graphicsDevice, new Color(139, 69, 19));
            
            // Green color for leaves
            state.LeafTexture = TextureUtilities.Create1x1Texture(graphicsDevice, new Color(34, 139, 34));
        }
    }
}
