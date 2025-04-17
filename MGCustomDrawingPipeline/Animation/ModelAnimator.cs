using Microsoft.Xna.Framework;

namespace MGCustomDrawingPipeline.Animation
{
    /// <summary>
    /// Handles animation of 3D models in the game
    /// </summary>
    public class ModelAnimator
    {
        /// <summary>
        /// Updates the rotation angles for model animation
        /// </summary>
        public static void UpdateAnimation(GameState state, float elapsed)
        {
            // Update rotation angles at different speeds for each axis
            // This creates a more interesting animation than rotating around
            // just one axis would
            state.RotationAngleX += 0.2f * elapsed;  // Rotate around X axis (slower)
            state.RotationAngleY += 0.3f * elapsed;  // Rotate around Y axis (faster)
            
            // Keep angles between 0 and 2π (full circle in radians)
            // This isn't strictly necessary but prevents the values from
            // growing too large over time which could cause precision issues
            state.RotationAngleX %= MathHelper.TwoPi;  // 2π ≈ 6.28 radians = 360 degrees
            state.RotationAngleY %= MathHelper.TwoPi;
            state.RotationAngleZ %= MathHelper.TwoPi;
        }
    }
}
