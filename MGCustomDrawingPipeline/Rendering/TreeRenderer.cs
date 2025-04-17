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
        /// 
        /// This method performs the following steps:
        /// 1. Sets up rendering states (depth testing, wireframe/solid mode)
        /// 2. Sets the vertex and index buffers for the GPU to use
        /// 3. Creates transformation matrices (world, view, projection)
        /// 4. Sets shader parameters for model transformations and lighting
        /// 5. Renders the trunk and foliage with appropriate textures
        /// </summary>
        /// <param name="graphicsDevice">The graphics device used for rendering</param>
        /// <param name="state">The game state containing all rendering resources</param>
        public void DrawTree(GraphicsDevice graphicsDevice, GameState state)
        {
            //===== BEGINNER'S GUIDE: 3D RENDERING PIPELINE =====//
            
            // Step 1: Enable depth testing to correctly handle overlapping triangles
            // The depth buffer (Z-buffer) stores the distance of each pixel from the camera
            // This ensures farther objects are occluded by nearer ones
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            // Step 2: Select the appropriate rasterizer state based on wireframe toggle
            // The rasterizer converts 3D triangles into 2D pixels on the screen
            graphicsDevice.RasterizerState = state.UseWireframe 
                ? state.WireframeRasterizerState  // Show only triangle edges
                : state.DoubleSidedRasterizerState;  // Show filled triangles

            // Step 3: Tell the GPU which vertices and indices to use for drawing
            // The vertex buffer contains positions, normals, and texture coordinates
            // The index buffer defines which vertices form triangles
            graphicsDevice.SetVertexBuffer(state.VertexBuffer);
            graphicsDevice.Indices = state.IndexBuffer;

            //===== BEGINNER'S GUIDE: 3D TRANSFORMATIONS =====//
            
            // Step 4: Create transformation matrices
            // In 3D graphics, we use matrices to transform objects from their own coordinate 
            // system (model space) to the screen (screen space)
            
            // The World matrix positions and orientates our object in the 3D world
            // Here we apply rotation and translation to the tree
            Matrix world = Matrix.CreateRotationX(state.RotationAngleX) * 
                           Matrix.CreateRotationY(state.RotationAngleY) * 
                           Matrix.CreateTranslation(0, 0, -0.5f);
            
            // The View matrix represents the camera's position and orientation
            // It transforms from world space to camera space (what the camera sees)
            Matrix view = Matrix.CreateLookAt(
                state.CameraPosition,  // Camera position: 2 units away from origin
                Vector3.Zero,          // Looking at the origin (0,0,0)
                Vector3.Up);           // "Up" direction is +Y axis
            
            // The Projection matrix adds perspective and defines the view frustum
            // It transforms from camera space to clip space
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,                    // Field of view: 45 degrees 
                graphicsDevice.Viewport.AspectRatio,   // Match the window's aspect ratio
                0.1f,                                  // Near clipping plane (min render distance)
                100.0f);                               // Far clipping plane (max render distance)

            // Step 5: Send matrices and lighting parameters to the shader
            // These control how the model is transformed and lit
            
            // WorldViewProjection combines all three transforms into one matrix
            // This transforms directly from model space to clip space
            state.TriangleEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            
            // World matrix is needed separately for lighting calculations
            state.TriangleEffect.Parameters["World"].SetValue(world);
            
            //===== BEGINNER'S GUIDE: LIGHTING MODEL =====//
            
            // Send lighting parameters to the shader
            // These determine how the model is illuminated
            
            // Light directions (normalized vectors pointing from the light to the scene)
            state.TriangleEffect.Parameters["LightDirection"].SetValue(state.LightDirection);
            state.TriangleEffect.Parameters["SunlightDirection"].SetValue(state.SunlightDirection);
            
            // Camera position (needed for specular reflections)
            state.TriangleEffect.Parameters["CameraPosition"].SetValue(state.CameraPosition);
            
            // Light colors and intensities
            // Ambient light: constant light present everywhere (simulates indirect light)
            state.TriangleEffect.Parameters["AmbientLight"].SetValue(state.AmbientLight);
            
            // Diffuse light: direct illumination that varies with surface angle
            state.TriangleEffect.Parameters["DiffuseLight"].SetValue(state.DiffuseLight);
            
            // Sunlight color and intensity
            state.TriangleEffect.Parameters["SunlightColor"].SetValue(state.SunlightColor);
            state.TriangleEffect.Parameters["SunlightIntensity"].SetValue(state.SunlightIntensity);
            
            // Specular light: bright highlights on reflective surfaces
            state.TriangleEffect.Parameters["SpecularLight"].SetValue(state.SpecularLight);
            state.TriangleEffect.Parameters["SpecularPower"].SetValue(state.SpecularPower);
            
            //===== BEGINNER'S GUIDE: RENDERING THE MODEL IN PARTS =====//
            
            // Step 6: Draw the tree in parts (trunk and foliage)
            // This allows us to use different textures for different parts
            
            // First draw the trunk with brown texture
            state.TriangleEffect.Parameters["ModelTexture"].SetValue(state.TrunkTexture);
            
            foreach (EffectPass pass in state.TriangleEffect.CurrentTechnique.Passes)
            {
                // Apply the current pass settings to the graphics device
                pass.Apply();
                
                // Draw the trunk portion (first 12 triangles - 6 faces with 2 triangles each)
                graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,  // Draw triangles
                    0,       // Base vertex offset
                    0,       // Start index
                    12       // Number of triangles for the trunk
                );
            }
            
            // Then draw the foliage with green texture
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
