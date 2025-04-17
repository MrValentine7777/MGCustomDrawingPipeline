using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MGCustomDrawingPipeline.Utilities;

namespace MGCustomDrawingPipeline.TextureManagement
{
    /// <summary>
    /// Handles creation of color textures for game objects
    /// 
    /// ===== BEGINNER'S GUIDE: SIMPLE TEXTURING =====
    /// 
    /// Textures in 3D graphics are images applied to the surface of objects.
    /// While textures are often detailed images, they can also be simple colors.
    /// 
    /// This class demonstrates a common optimization technique: using tiny 1x1 pixel
    /// textures as a simple way to apply solid colors to 3D models. The advantages are:
    /// 
    /// 1. Consistency - The same shader can handle both textured and colored objects
    /// 2. Efficiency - No need for separate rendering paths for textured vs. colored objects
    /// 3. Flexibility - Easy to change colors at runtime without modifying shaders
    /// 
    /// Instead of using a specialized "color material" system, we simply create
    /// solid-color textures on the fly and use our standard texturing pipeline.
    /// </summary>
    public static class ColorTextureCreator
    {
        /// <summary>
        /// Creates the 1x1 textures for the tree colors and assigns them to the game state
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to create textures with</param>
        /// <param name="state">The game state to store the created textures</param>
        public static void CreateTreeColorTextures(GraphicsDevice graphicsDevice, GameState state)
        {
            //===== BEGINNER'S GUIDE: COLOR DEFINITION =====//
            
            // Brown color for trunk (RGB: 139, 69, 19)
            // This creates a medium-dark brown similar to tree bark
            state.TrunkTexture = TextureUtilities.Create1x1Texture(graphicsDevice, new Color(139, 69, 19));
            
            // Green color for leaves (RGB: 34, 139, 34)
            // This creates a forest green color for the foliage
            state.LeafTexture = TextureUtilities.Create1x1Texture(graphicsDevice, new Color(34, 139, 34));
            
            // In a more advanced application, we could:
            // - Load actual texture images from files
            // - Create procedural textures with patterns
            // - Generate mipmaps for better quality at different distances
            // - Use texture arrays for more complex materials
        }
    }
}
