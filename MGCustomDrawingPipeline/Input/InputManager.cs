using Microsoft.Xna.Framework.Input;

namespace MGCustomDrawingPipeline.Input
{
    /// <summary>
    /// Handles user input for the game
    /// 
    /// ===== BEGINNER'S GUIDE: INPUT MANAGEMENT =====
    /// 
    /// Input management is a critical part of any interactive application.
    /// In games, we typically need to handle various input sources and states:
    /// 
    /// 1. Keyboard Input - Detecting key presses, releases, and holds
    /// 2. Mouse Input - Tracking position, clicks, and scrolling
    /// 3. Gamepad Input - Reading controller buttons, triggers, and sticks
    /// 
    /// Input can be processed in different ways:
    /// 
    /// - Polling: Checking the current state of input devices each frame (used here)
    /// - Event-Based: Responding to input events when they occur
    /// - Command Pattern: Translating input into game commands that can be queued or remapped
    /// 
    /// This simple InputManager demonstrates polling with state tracking, which allows us
    /// to detect new key presses by comparing current and previous input states.
    /// </summary>
    public class InputManager
    {
        /// <summary>
        /// Processes keyboard input for the current frame
        /// </summary>
        /// <param name="state">The game state to update based on input</param>
        public static void ProcessInput(GameState state)
        {
            //===== BEGINNER'S GUIDE: KEY PRESS DETECTION =====//
            
            // Get the current state of the keyboard
            // This returns a snapshot of which keys are currently pressed
            KeyboardState keyboardState = Keyboard.GetState();
            
            // Toggle post-processing with P key
            // We only toggle when the key is first pressed (not held down)
            // This is done by checking if the key is down now but wasn't down previously
            if (keyboardState.IsKeyDown(Keys.P) && !state.PreviousKeyboardState.IsKeyDown(Keys.P))
            {
                state.UsePostProcessing = !state.UsePostProcessing;
            }
            
            // Toggle wireframe mode with W key
            // Same technique: check for a key transition from up to down
            if (keyboardState.IsKeyDown(Keys.W) && !state.PreviousKeyboardState.IsKeyDown(Keys.W))
            {
                state.UseWireframe = !state.UseWireframe;
            }
            
            // Store current keyboard state for next frame
            // This allows us to detect key presses rather than key holds
            state.PreviousKeyboardState = keyboardState;
        }
    }
}
