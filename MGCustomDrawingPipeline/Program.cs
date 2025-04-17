/// <summary>
/// Application Entry Point
/// 
/// This is the main entry point for the application. It creates a new instance
/// of our Game1 class and runs it. This is the standard pattern for MonoGame
/// applications.
/// 
/// When the application starts:
/// 1. A new Game1 instance is created
/// 2. The Run() method starts the game loop, which will continue until the game is closed
/// 3. The game loop repeatedly calls Update() and Draw() methods in the Game1 class
/// </summary>
using var game = new MGCustomDrawingPipeline.Game1();
game.Run();
