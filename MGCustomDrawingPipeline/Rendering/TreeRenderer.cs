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
            
            // Select the appropriate rasterizer state based on wireframe toggle
            graphicsDevice.RasterizerState = state.UseWireframe 
                ? state.WireframeRasterizerState 
                : state.DoubleSidedRasterizerState;

            // Tell the GPU which vertices and indices to use for drawing
            graphicsDevice.SetVertexBuffer(state.VertexBuffer);
            graphicsDevice.Indices = state.IndexBuffer;

            // Position the tree slightly back from the camera for better viewing
            Matrix world = Matrix.CreateRotationX(state.RotationAngleX) * 
                          Matrix.CreateRotationY(state.RotationAngleY) * 
                          Matrix.CreateTranslation(0, 0, -0.5f);
            
            // Create a view matrix - this is like placing a camera in the world
            Matrix view = Matrix.CreateLookAt(
                state.CameraPosition,   // Camera position: 2 units away from origin
                Vector3.Zero,           // Looking at the origin (0,0,0)
                Vector3.Up);            // "Up" direction is +Y axis
            
            // Create a projection matrix - this adds perspective effect
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,                    // Field of view: 45 degrees 
                graphicsDevice.Viewport.AspectRatio,   // Match the window's aspect ratio
                0.1f,                                  // Near clipping plane (min render distance)
                100.0f);                               // Far clipping plane (max render distance)

            // Send the transformation matrices to the shader
            state.TriangleEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            state.TriangleEffect.Parameters["World"].SetValue(world);
            
            // Send lighting parameters to the shader
            state.TriangleEffect.Parameters["LightDirection"].SetValue(state.LightDirection);
            state.TriangleEffect.Parameters["SunlightDirection"].SetValue(state.SunlightDirection);
            state.TriangleEffect.Parameters["CameraPosition"].SetValue(state.CameraPosition);
            state.TriangleEffect.Parameters["AmbientLight"].SetValue(state.AmbientLight);
            state.TriangleEffect.Parameters["DiffuseLight"].SetValue(state.DiffuseLight);
            state.TriangleEffect.Parameters["SunlightColor"].SetValue(state.SunlightColor);
            state.TriangleEffect.Parameters["SunlightIntensity"].SetValue(state.SunlightIntensity);
            state.TriangleEffect.Parameters["SpecularLight"].SetValue(state.SpecularLight);
            state.TriangleEffect.Parameters["SpecularPower"].SetValue(state.SpecularPower);
            
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
