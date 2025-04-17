using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGCustomDrawingPipeline.Rendering
{
    /// <summary>
    /// Handles rendering of the 3D tree model
    /// </summary>
    public class TreeRenderer
    {
        /// <summary>
        /// Draws the rotating tree using textures for coloring
        /// </summary>
        public void DrawTree(GraphicsDevice graphicsDevice, GameState state)
        {
            // Enable depth testing to correctly handle overlapping triangles
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = state.DoubleSidedRasterizerState;

            // Tell the GPU which vertices and indices to use for drawing
            graphicsDevice.SetVertexBuffer(state.VertexBuffer);
            graphicsDevice.Indices = state.IndexBuffer;

            // Position the tree slightly back from the camera for better viewing
            Matrix world = Matrix.CreateRotationX(state.RotationAngleX) * 
                          Matrix.CreateRotationY(state.RotationAngleY) * 
                          Matrix.CreateTranslation(0, 0, -0.5f);
            
            // Create a view matrix - this is like placing a camera in the world
            Matrix view = Matrix.CreateLookAt(
                new Vector3(0, 0, 2),  // Camera position: 2 units away from origin
                Vector3.Zero,          // Looking at the origin (0,0,0)
                Vector3.Up);           // "Up" direction is +Y axis
            
            // Create a projection matrix - this adds perspective effect
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,                    // Field of view: 45 degrees 
                graphicsDevice.Viewport.AspectRatio,   // Match the window's aspect ratio
                0.1f,                                  // Near clipping plane (min render distance)
                100.0f);                               // Far clipping plane (max render distance)

            // Send the transformation matrix to the shader
            state.TriangleEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            
            // First draw the trunk
            state.TriangleEffect.Parameters["ModelTexture"].SetValue(state.TrunkTexture);
            
            foreach (EffectPass pass in state.TriangleEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                
                // Draw the trunk portion (first 12 triangles - 6 faces with 2 triangles each)
                graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,       // Base vertex offset
                    0,       // Start index
                    12       // Number of triangles for the trunk
                );
            }
            
            // Then draw the foliage
            state.TriangleEffect.Parameters["ModelTexture"].SetValue(state.LeafTexture);
            
            foreach (EffectPass pass in state.TriangleEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                
                // Draw the foliage portion (remaining triangles)
                graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,            // Base vertex offset
                    12 * 3,       // Start index (after trunk triangles)
                    state.TotalTriangles - 12  // Number of triangles for foliage
                );
            }
        }
    }
}
