using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGCustomDrawingPipeline.VertexTypes
{
    /// <summary>
    /// A vertex structure containing position and texture coordinates
    /// 
    /// ===== BEGINNER'S GUIDE: SIMPLIFIED VERTEX STRUCTURES =====
    /// 
    /// Not all 3D graphics require the same level of detail. When lighting calculations
    /// aren't needed, we can use simpler vertex structures like this one that omit normal data.
    /// 
    /// Simpler vertex structures offer several advantages:
    /// 1. Reduced Memory Usage - Less data per vertex means lower memory consumption
    /// 2. Better Performance - Less data to process and transfer to the GPU
    /// 3. Simpler Shaders - Vertex and pixel shaders can be less complex
    /// 
    /// Common use cases for position+texture vertices include:
    /// - UI elements and 2D sprites rendered in 3D space
    /// - Skyboxes that don't require lighting
    /// - Particle effects where lighting isn't calculated per-vertex
    /// - Post-processing full-screen quads (like our bloom effect)
    /// 
    /// When designing your application, choose the simplest vertex structure that meets
    /// your visual requirements to maximize performance.
    /// </summary>
    public struct VertexPositionTexture : IVertexType
    {
        /// <summary>
        /// The position of the vertex in 3D space (x, y, z)
        /// This defines where in the world the vertex is located
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// The texture coordinates (u, v) for mapping textures to this vertex
        /// These coordinates determine which part of a texture is applied to this vertex
        /// Texture coordinates typically range from 0.0 to 1.0
        /// </summary>
        public Vector2 TextureCoordinate;
        
        /// <summary>
        /// Defines the memory layout of this vertex structure for the GPU
        /// This tells the graphics hardware how to interpret the vertex data in memory
        /// 
        /// A vertex declaration consists of VertexElement objects that define:
        /// - The offset in bytes where each component starts
        /// - The format of each component (Vector2, Vector3, etc.)
        /// - The semantic usage (what the component represents: Position, TextureCoordinate, etc.)
        /// </summary>
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            // Position comes first in memory, uses 12 bytes (3 floats x 4 bytes each)
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            
            // Texture coordinate comes next, offset by 12 bytes, uses 8 bytes (2 floats x 4 bytes each)
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        /// <summary>
        /// Constructor for creating a new vertex with position and texture coordinates
        /// </summary>
        /// <param name="position">The 3D position of the vertex</param>
        /// <param name="textureCoordinate">The texture coordinates (u,v)</param>
        public VertexPositionTexture(Vector3 position, Vector2 textureCoordinate)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
        }

        /// <summary>
        /// Implementation of IVertexType interface.
        /// This property allows the MonoGame graphics system to determine the 
        /// vertex declaration when rendering.
        /// The GPU needs to know how the vertex data is structured in memory to interpret it correctly.
        /// </summary>
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
