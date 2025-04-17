using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGCustomDrawingPipeline.VertexTypes
{
    /// <summary>
    /// A custom vertex structure containing position, normal vector, and texture coordinates
    /// Renamed to avoid conflict with MonoGame's built-in VertexPositionNormalTexture
    /// 
    /// This vertex structure is used for 3D models that require lighting calculations.
    /// The normal vector is essential for determining how light interacts with the surface.
    /// </summary>
    public struct CustomVertexPositionNormalTexture : IVertexType
    {
        /// <summary>
        /// The position of the vertex in 3D space (x, y, z)
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// The normal vector perpendicular to the surface at this vertex
        /// Used for lighting calculations - determines how light reflects off the surface
        /// </summary>
        public Vector3 Normal;
        
        /// <summary>
        /// The texture coordinates (u, v) for mapping textures to this vertex
        /// In this project, we're using 1x1 textures, so these are mostly used for color
        /// </summary>
        public Vector2 TextureCoordinate;
        
        /// <summary>
        /// Defines the memory layout of this vertex structure for the GPU
        /// This tells the graphics hardware how to interpret the vertex data
        /// </summary>
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            // Position comes first in memory, uses 12 bytes (3 floats x 4 bytes each)
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            
            // Normal comes next, offset by 12 bytes, uses 12 bytes
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            
            // Texture coordinate comes last, offset by 24 bytes, uses 8 bytes
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        /// <summary>
        /// Constructor for creating a new vertex with position, normal and texture coordinates
        /// </summary>
        /// <param name="position">The 3D position of the vertex</param>
        /// <param name="normal">The normal vector (should be normalized)</param>
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
        /// </summary>
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
