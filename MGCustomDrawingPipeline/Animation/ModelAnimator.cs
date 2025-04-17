using Microsoft.Xna.Framework;

namespace MGCustomDrawingPipeline.Animation
{
    /// <summary>
    /// Handles animation of 3D models in the game
    /// 
    /// ===== BEGINNER'S GUIDE: SIMPLE 3D ANIMATION =====
    /// 
    /// 3D animation comes in many forms, from simple rotations to complex skeletal animations.
    /// This class demonstrates the most basic form - transformational animation:
    /// 
    /// 1. Transformational Animation - Changing position, rotation, or scale over time
    ///    - Simple to implement, just update transformation values each frame
    ///    - Limited to whole-object movement (can't animate parts independently)
    ///    - Perfect for simple objects like our rotating tree
    /// 
    /// More advanced animation techniques (not shown here) include:
    /// 
    /// 2. Skeletal Animation - Uses a hierarchy of "bones" to deform a mesh
    ///    - Allows complex character animation (walking, jumping, etc.)
    ///    - Requires rigged models with bone weights assigned to vertices
    ///    - Much more complex to implement but very flexible
    /// 
    /// 3. Vertex Animation - Directly animating vertex positions
    ///    - Good for specific effects like cloth, water, or facial expressions
    ///    - Typically requires storing multiple versions of the geometry
    ///    - High memory usage but can achieve effects not possible with skeletal animation
    /// 
    /// For our simple tree model, basic rotation animation provides a visually interesting
    /// result without the complexity of more advanced techniques.
    /// </summary>
    public class ModelAnimator
    {
        /// <summary>
        /// Updates the rotation angles for model animation
        /// </summary>
        /// <param name="state">The game state containing animation parameters</param>
        /// <param name="elapsed">Time elapsed since the last update (in seconds)</param>
        public static void UpdateAnimation(GameState state, float elapsed)
        {
            //===== BEGINNER'S GUIDE: TIME-BASED ANIMATION =====//
            
            // Update rotation angles at different speeds for each axis
            // We multiply by elapsed time to ensure smooth animation regardless of frame rate
            // This is called "frame rate independent" animation
            state.RotationAngleX += 0.5f * elapsed;  // Rotate around X axis (slower)
            state.RotationAngleY += 0.7f * elapsed;  // Rotate around Y axis (faster)
            
            // Keep angles between 0 and 2π (full circle in radians)
            // This isn't strictly necessary but prevents the values from
            // growing too large over time which could cause precision issues
            state.RotationAngleX %= MathHelper.TwoPi;  // 2π ≈ 6.28 radians = 360 degrees
            state.RotationAngleY %= MathHelper.TwoPi;
            state.RotationAngleZ %= MathHelper.TwoPi;
        }
    }
}
