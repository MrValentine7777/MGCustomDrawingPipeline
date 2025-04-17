using Microsoft.Xna.Framework.Input;

namespace MGCustomDrawingPipeline.Input
{
    /// <summary>
    /// Handles user input for the game
    /// </summary>
    public class InputManager
    {
        /// <summary>
        /// Processes keyboard input for the current frame
        /// </summary>
        public static void ProcessInput(GameState state)
        {
            // Check for keyboard input
            KeyboardState keyboardState = Keyboard.GetState();
            
            // Toggle post-processing with P key
            if (keyboardState.IsKeyDown(Keys.P) && !state.PreviousKeyboardState.IsKeyDown(Keys.P))
            {
                state.UsePostProcessing = !state.UsePostProcessing;
            }
            
            // Store current keyboard state for next frame
            state.PreviousKeyboardState = keyboardState;
        }
    }
}
