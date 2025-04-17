using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGCustomDrawingPipeline.VertexTypes
{
    /// <summary>
    /// A custom vertex structure containing position, normal vector, and texture coordinates
    /// Renamed to avoid conflict with MonoGame's built-in VertexPositionNormalTexture
    /// 
    /// ===== BEGINNER'S GUIDE: VERTEX STRUCTURES =====
    /// 
    /// In 3D graphics, vertices are the fundamental building blocks of all models.
    /// A vertex is a point in 3D space that includes additional data beyond just position:
    /// 
    /// 1. Position - Where the point is located in 3D space (x, y, z coordinates)
    /// 2. Normal - A directional vector used for lighting calculations
    /// 3. Texture Coordinates - Values that map images onto the surface (u, v coordinates)
    /// 
    /// Vertex structures must be carefully designed to:
    /// - Include all necessary data for your rendering needs
    /// - Be memory-efficient (more data = more memory and bandwidth usage)
    /// - Follow a layout that the GPU can process efficiently
    /// 
    /// Custom vertex structures must implement IVertexType to tell the graphics system
    /// how to interpret the data when sending it to the GPU. The VertexDeclaration
    /// provides this information to the graphics pipeline.
    /// </summary>
    public struct CustomVertexPositionNormalTexture : IVertexType
    {
        /// <summary>
        /// The position of the vertex in 3D space (x, y, z)
        /// This defines where in the world the vertex is located
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// The normal vector perpendicular to the surface at this vertex
        /// Used for lighting calculations - determines how light reflects off the surface
        /// A normal should typically be normalized (have a length of 1.0)
        /// </summary>
        public Vector3 Normal;
        
        /// <summary>
        /// The texture coordinates (u, v) for mapping textures to this vertex
        /// In this project, we're using 1x1 textures, so these are mostly used for color mapping
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
        /// - The semantic usage (what the component represents: Position, Normal, etc.)
        /// </summary>
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            // Position comes first in memory, uses 12 bytes (3 floats x 4 bytes each)
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            
            // Normal comes next, offset by 12 bytes, uses 12 bytes (3 floats x 4 bytes each)
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            
            // Texture coordinate comes last, offset by 24 bytes, uses 8 bytes (2 floats x 4 bytes each)
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        /// <summary>
        /// Constructor for creating a new vertex with position, normal and texture coordinates
        /// </summary>
        /// <param name="position">The 3D position of the vertex</param>
        /// <param name="normal">The normal vector (should be normalized for best results)</param>
        /// <param name="textureCoordinate">The texture coordinates (u,v)</param>
        public CustomVertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
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
