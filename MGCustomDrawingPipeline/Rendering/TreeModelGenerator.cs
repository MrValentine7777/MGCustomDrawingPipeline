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
            (CustomVertexPositionNormalTexture[] vertices, short[] indices) = GenerateTree();
            state.TotalVertices = vertices.Length;
            state.TotalIndices = indices.Length;
            state.TotalTriangles = indices.Length / 3;

            // Create vertex buffer
            state.VertexBuffer = new VertexBuffer(
                graphicsDevice,
                CustomVertexPositionNormalTexture.VertexDeclaration,
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
        /// Generates vertices and indices for a simple tree with normals for shading
        /// </summary>
        private static (CustomVertexPositionNormalTexture[] vertices, short[] indices) GenerateTree()
        {
            // Create a list to hold all vertices and indices
            var verticesList = new List<CustomVertexPositionNormalTexture>();
            var indicesList = new List<short>();
            
            // For 1x1 textures, we can use any texture coordinate
            // The color will be the same regardless of the UV
            Vector2 texCoord = new Vector2(0.5f, 0.5f);
            
            // 1. Create the trunk (a simple rectangular prism)
            Vector3 trunkBase = new Vector3(0, -0.5f, 0);
            float trunkWidth = 0.1f;
            float trunkHeight = 0.5f;
            
            // Trunk vertices (bottom square) with outward-facing normals
            verticesList.Add(new CustomVertexPositionNormalTexture(trunkBase + new Vector3(-trunkWidth, 0, -trunkWidth), Vector3.Normalize(new Vector3(-1, -1, -1)), texCoord));
            verticesList.Add(new CustomVertexPositionNormalTexture(trunkBase + new Vector3(trunkWidth, 0, -trunkWidth), Vector3.Normalize(new Vector3(1, -1, -1)), texCoord));
            verticesList.Add(new CustomVertexPositionNormalTexture(trunkBase + new Vector3(trunkWidth, 0, trunkWidth), Vector3.Normalize(new Vector3(1, -1, 1)), texCoord));
            verticesList.Add(new CustomVertexPositionNormalTexture(trunkBase + new Vector3(-trunkWidth, 0, trunkWidth), Vector3.Normalize(new Vector3(-1, -1, 1)), texCoord));
            
            // Trunk vertices (top square) with outward-facing normals
            verticesList.Add(new CustomVertexPositionNormalTexture(trunkBase + new Vector3(-trunkWidth, trunkHeight, -trunkWidth), Vector3.Normalize(new Vector3(-1, 1, -1)), texCoord));
            verticesList.Add(new CustomVertexPositionNormalTexture(trunkBase + new Vector3(trunkWidth, trunkHeight, -trunkWidth), Vector3.Normalize(new Vector3(1, 1, -1)), texCoord));
            verticesList.Add(new CustomVertexPositionNormalTexture(trunkBase + new Vector3(trunkWidth, trunkHeight, trunkWidth), Vector3.Normalize(new Vector3(1, 1, 1)), texCoord));
            verticesList.Add(new CustomVertexPositionNormalTexture(trunkBase + new Vector3(-trunkWidth, trunkHeight, trunkWidth), Vector3.Normalize(new Vector3(-1, 1, 1)), texCoord));
            
            // Trunk indices (6 faces, 2 triangles per face = 12 triangles)
            // Bottom face (normal facing down)
            AddQuadWithNormals(verticesList, indicesList, 0, 1, 2, 3, Vector3.Down);
            
            // Top face (normal facing up)
            AddQuadWithNormals(verticesList, indicesList, 7, 6, 5, 4, Vector3.Up);
            
            // Side faces
            AddQuadWithNormals(verticesList, indicesList, 0, 4, 5, 1, Vector3.Backward);
            AddQuadWithNormals(verticesList, indicesList, 1, 5, 6, 2, Vector3.Right);
            AddQuadWithNormals(verticesList, indicesList, 2, 6, 7, 3, Vector3.Forward);
            AddQuadWithNormals(verticesList, indicesList, 3, 7, 4, 0, Vector3.Left);
            
            // 2. Create pyramid-like foliage (a simple cone approximation)
            int baseVertex = verticesList.Count;
            float leafRadius = 0.4f;
            float leafHeight = 0.7f;
            Vector3 leafBase = trunkBase + new Vector3(0, trunkHeight, 0);
            Vector3 leafTop = leafBase + new Vector3(0, leafHeight, 0);
            
            // Add the top vertex of the cone
            verticesList.Add(new CustomVertexPositionNormalTexture(leafTop, Vector3.Up, texCoord));
            
            // Add vertices in a circle for the base of the cone
            int leafSegments = 8;
            for (int i = 0; i < leafSegments; i++)
            {
                float angle = i * MathHelper.TwoPi / leafSegments;
                float x = (float)System.Math.Sin(angle) * leafRadius;
                float z = (float)System.Math.Cos(angle) * leafRadius;
                
                // Calculate normal for each vertex pointing outward and upward
                Vector3 normal = Vector3.Normalize(new Vector3(x, 0.5f, z));
                
                verticesList.Add(new CustomVertexPositionNormalTexture(leafBase + new Vector3(x, 0, z), normal, texCoord));
            }
            
            // Add triangles connecting the top to each segment of the circle
            for (int i = 0; i < leafSegments; i++)
            {
                int next = (i + 1) % leafSegments;
                
                // Calculate face normal for this triangle
                Vector3 v0 = verticesList[baseVertex].Position;
                Vector3 v1 = verticesList[baseVertex + 1 + i].Position;
                Vector3 v2 = verticesList[baseVertex + 1 + next].Position;
                
                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
                
                int topIndex = baseVertex;
                int index1 = baseVertex + 1 + i;
                int index2 = baseVertex + 1 + next;
                
                indicesList.Add((short)topIndex);
                indicesList.Add((short)index1);
                indicesList.Add((short)index2);
            }
            
            // Add the bottom face of the cone (optional, as it's not usually visible)
            for (int i = 1; i < leafSegments - 1; i++)
            {
                // Calculate face normal (pointing down)
                Vector3 normal = Vector3.Down;
                
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
            verticesList.Add(new CustomVertexPositionNormalTexture(leafTop, Vector3.Up, texCoord));
            
            // Add vertices in a circle for the base of the second cone
            for (int i = 0; i < leafSegments; i++)
            {
                float angle = i * MathHelper.TwoPi / leafSegments;
                float x = (float)System.Math.Sin(angle) * leafRadius;
                float z = (float)System.Math.Cos(angle) * leafRadius;
                
                // Calculate normal for each vertex pointing outward and upward
                Vector3 normal = Vector3.Normalize(new Vector3(x, 0.5f, z));
                
                verticesList.Add(new CustomVertexPositionNormalTexture(leafBase + new Vector3(x, 0, z), normal, texCoord));
            }
            
            // Add triangles connecting the top to each segment of the circle
            for (int i = 0; i < leafSegments; i++)
            {
                int next = (i + 1) % leafSegments;
                
                // Calculate face normal for this triangle
                Vector3 v0 = verticesList[baseVertex].Position;
                Vector3 v1 = verticesList[baseVertex + 1 + i].Position;
                Vector3 v2 = verticesList[baseVertex + 1 + next].Position;
                
                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
                
                int topIndex = baseVertex;
                int index1 = baseVertex + 1 + i;
                int index2 = baseVertex + 1 + next;
                
                indicesList.Add((short)topIndex);
                indicesList.Add((short)index1);
                indicesList.Add((short)index2);
            }
            
            // Convert lists to arrays
            return (verticesList.ToArray(), indicesList.ToArray());
        }
        
        /// <summary>
        /// Helper method to add indices for a quad (two triangles) with proper normals
        /// </summary>
        private static void AddQuadWithNormals(List<CustomVertexPositionNormalTexture> vertices, List<short> indices, 
                                              int a, int b, int c, int d, Vector3 normal)
        {
            // Calculate start index for new vertices
            int baseIndex = vertices.Count;
            
            // Create new vertices with the correct normal for this face
            Vector2 texCoord = new Vector2(0.5f, 0.5f);
            vertices.Add(new CustomVertexPositionNormalTexture(vertices[a].Position, normal, texCoord));
            vertices.Add(new CustomVertexPositionNormalTexture(vertices[b].Position, normal, texCoord));
            vertices.Add(new CustomVertexPositionNormalTexture(vertices[c].Position, normal, texCoord));
            vertices.Add(new CustomVertexPositionNormalTexture(vertices[d].Position, normal, texCoord));
            
            // First triangle
            indices.Add((short)(baseIndex + 0));
            indices.Add((short)(baseIndex + 1));
            indices.Add((short)(baseIndex + 2));
            
            // Second triangle
            indices.Add((short)(baseIndex + 0));
            indices.Add((short)(baseIndex + 2));
            indices.Add((short)(baseIndex + 3));
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
