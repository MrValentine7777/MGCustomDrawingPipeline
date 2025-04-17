using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using MGCustomDrawingPipeline.VertexTypes;

namespace MGCustomDrawingPipeline.Rendering
{
    /// <summary>
    /// Responsible for generating the tree 3D model
    /// </summary>
    public class TreeModelGenerator
    {
        /// <summary>
        /// Creates a simple 3D tree model with a trunk and branches
        /// </summary>
        public static void CreateTreeModel(GraphicsDevice graphicsDevice, GameState state)
        {
            // Define the tree components
            (MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture[] vertices, short[] indices) = GenerateTree();
            state.TotalVertices = vertices.Length;
            state.TotalIndices = indices.Length;
            state.TotalTriangles = indices.Length / 3;

            // Create vertex buffer
            state.VertexBuffer = new VertexBuffer(
                graphicsDevice,
                MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture.VertexDeclaration,
                state.TotalVertices,
                BufferUsage.WriteOnly);
            state.VertexBuffer.SetData(vertices);

            // Create index buffer
            state.IndexBuffer = new IndexBuffer(
                graphicsDevice,
                IndexElementSize.SixteenBits,
                state.TotalIndices,
                BufferUsage.WriteOnly);
            state.IndexBuffer.SetData(indices);
        }

        /// <summary>
        /// Generates vertices and indices for a simple tree
        /// </summary>
        private static (MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture[] vertices, short[] indices) GenerateTree()
        {
            // Create a list to hold all vertices and indices
            var verticesList = new List<MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture>();
            var indicesList = new List<short>();
            
            // For 1x1 textures, we can use any texture coordinate
            // The color will be the same regardless of the UV
            Vector2 texCoord = new Vector2(0.5f, 0.5f);
            
            // 1. Create the trunk (a simple rectangular prism)
            Vector3 trunkBase = new Vector3(0, -0.5f, 0);
            float trunkWidth = 0.1f;
            float trunkHeight = 0.5f;
            
            // Trunk vertices (bottom square)
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(-trunkWidth, 0, -trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(trunkWidth, 0, -trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(trunkWidth, 0, trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(-trunkWidth, 0, trunkWidth), texCoord));
            
            // Trunk vertices (top square)
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(-trunkWidth, trunkHeight, -trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(trunkWidth, trunkHeight, -trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(trunkWidth, trunkHeight, trunkWidth), texCoord));
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(trunkBase + new Vector3(-trunkWidth, trunkHeight, trunkWidth), texCoord));
            
            // Trunk indices (6 faces, 2 triangles per face = 12 triangles)
            // Bottom face
            AddQuad(indicesList, 0, 1, 2, 3);
            
            // Top face
            AddQuad(indicesList, 7, 6, 5, 4);
            
            // Side faces
            AddQuad(indicesList, 0, 4, 5, 1);
            AddQuad(indicesList, 1, 5, 6, 2);
            AddQuad(indicesList, 2, 6, 7, 3);
            AddQuad(indicesList, 3, 7, 4, 0);
            
            // 2. Create pyramid-like foliage (a simple cone approximation)
            int baseVertex = verticesList.Count;
            float leafRadius = 0.4f;
            float leafHeight = 0.7f;
            Vector3 leafBase = trunkBase + new Vector3(0, trunkHeight, 0);
            Vector3 leafTop = leafBase + new Vector3(0, leafHeight, 0);
            
            // Add the top vertex of the cone
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(leafTop, texCoord));
            
            // Add vertices in a circle for the base of the cone
            int leafSegments = 8;
            for (int i = 0; i < leafSegments; i++)
            {
                float angle = i * MathHelper.TwoPi / leafSegments;
                float x = (float)System.Math.Sin(angle) * leafRadius;
                float z = (float)System.Math.Cos(angle) * leafRadius;
                
                verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(leafBase + new Vector3(x, 0, z), texCoord));
            }
            
            // Add triangles connecting the top to each segment of the circle
            for (int i = 0; i < leafSegments; i++)
            {
                int next = (i + 1) % leafSegments;
                indicesList.Add((short)baseVertex); // Top vertex
                indicesList.Add((short)(baseVertex + 1 + i));
                indicesList.Add((short)(baseVertex + 1 + next));
            }
            
            // Add the bottom face of the cone (optional, as it's not usually visible)
            for (int i = 1; i < leafSegments - 1; i++)
            {
                indicesList.Add((short)(baseVertex + 1));
                indicesList.Add((short)(baseVertex + 1 + i + 1));
                indicesList.Add((short)(baseVertex + 1 + i));
            }
            
            // Create a second layer of foliage above the first
            baseVertex = verticesList.Count;
            leafBase = leafBase + new Vector3(0, leafHeight * 0.3f, 0);
            leafTop = leafBase + new Vector3(0, leafHeight * 0.7f, 0);
            leafRadius *= 0.7f;
            
            // Add the top vertex of the second cone
            verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(leafTop, texCoord));
            
            // Add vertices in a circle for the base of the second cone
            for (int i = 0; i < leafSegments; i++)
            {
                float angle = i * MathHelper.TwoPi / leafSegments;
                float x = (float)System.Math.Sin(angle) * leafRadius;
                float z = (float)System.Math.Cos(angle) * leafRadius;
                
                verticesList.Add(new MGCustomDrawingPipeline.VertexTypes.VertexPositionTexture(leafBase + new Vector3(x, 0, z), texCoord));
            }
            
            // Add triangles connecting the top to each segment of the circle
            for (int i = 0; i < leafSegments; i++)
            {
                int next = (i + 1) % leafSegments;
                indicesList.Add((short)baseVertex); // Top vertex
                indicesList.Add((short)(baseVertex + 1 + i));
                indicesList.Add((short)(baseVertex + 1 + next));
            }
            
            // Convert lists to arrays
            return (verticesList.ToArray(), indicesList.ToArray());
        }
        
        /// <summary>
        /// Helper method to add indices for a quad (two triangles)
        /// </summary>
        private static void AddQuad(List<short> indices, int a, int b, int c, int d)
        {
            // First triangle
            indices.Add((short)a);
            indices.Add((short)b);
            indices.Add((short)c);
            
            // Second triangle
            indices.Add((short)a);
            indices.Add((short)c);
            indices.Add((short)d);
        }
    }
}
