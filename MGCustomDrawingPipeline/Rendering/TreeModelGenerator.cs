using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using MGCustomDrawingPipeline.VertexTypes;

namespace MGCustomDrawingPipeline.Rendering
{
    /// <summary>
    /// Responsible for generating the 3D tree model geometry
    /// </summary>
    public class TreeModelGenerator
    {
        /// <summary>
        /// Creates a simple 3D tree model with a trunk and branches
        /// </summary>
        /// <param name="graphicsDevice">The graphics device for creating GPU resources</param>
        /// <param name="state">The game state to store the created buffers</param>
        public static void CreateTreeModel(GraphicsDevice graphicsDevice, GameState state)
        {
            // Generate the tree geometry (vertices and indices)
            (CustomVertexPositionNormalTexture[] vertices, short[] indices) = GenerateTree();
            
            // Store count information for debugging and drawing
            state.TotalVertices = vertices.Length;
            state.TotalIndices = indices.Length;
            state.TotalTriangles = indices.Length / 3;

            // Create a vertex buffer to store our vertices on the GPU
            // The GPU can access this data directly during rendering
            state.VertexBuffer = new VertexBuffer(
                graphicsDevice,
                CustomVertexPositionNormalTexture.VertexDeclaration,
                state.TotalVertices,
                BufferUsage.WriteOnly);
            
            // Upload the vertex data to the GPU
            state.VertexBuffer.SetData(vertices);

            // Create an index buffer to tell the GPU which vertices form triangles
            // This saves memory by reusing vertices in multiple triangles
            state.IndexBuffer = new IndexBuffer(
                graphicsDevice,
                IndexElementSize.SixteenBits, // 16-bit indices support up to 65,536 vertices
                state.TotalIndices,
                BufferUsage.WriteOnly);
            
            // Upload the index data to the GPU
            state.IndexBuffer.SetData(indices);
        }

        /// <summary>
        /// Generates vertices and indices for a simple tree with normals for shading
        /// 
        /// This method creates a procedural tree model with:
        /// 1. A rectangular trunk at the bottom
        /// 2. Two pyramid-like foliage sections above the trunk
        /// </summary>
        /// <returns>Arrays of vertices and indices that define the tree geometry</returns>
        private static (CustomVertexPositionNormalTexture[] vertices, short[] indices) GenerateTree()
        {
            // Create lists to hold all vertices and indices
            // Lists are used because we'll be adding vertices/indices in sections
            var verticesList = new List<CustomVertexPositionNormalTexture>();
            var indicesList = new List<short>();
            
            // For 1x1 textures, we can use any texture coordinate
            // The color will be the same regardless of the UV coordinates
            Vector2 texCoord = new Vector2(0.5f, 0.5f);
            
            //===== BEGINNER'S GUIDE: CREATING 3D GEOMETRY =====//
            
            // 1. CREATE THE TREE TRUNK
            // We'll create a rectangular prism (box) for the trunk
            Vector3 trunkBase = new Vector3(0, -0.5f, 0); // Bottom center of trunk
            float trunkWidth = 0.1f;                      // Width/depth of trunk
            float trunkHeight = 0.5f;                     // Height of trunk
            
            // In 3D graphics, we build shapes out of triangles
            // For a box, we need 8 vertices (corners) and 12 triangles (2 per face)
            
            // These vertices define the 8 corners of our trunk box
            // Each vertex includes a position, normal direction, and texture coordinate
            
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
            
            // Now we define the faces of the trunk by creating triangles
            // Each face of the box consists of 2 triangles (6 indices)
            
            // For correct lighting, we create separate vertices for each face
            // so that normals point in the right direction for each face
            
            // Bottom face (normal facing down)
            AddQuadWithNormals(verticesList, indicesList, 0, 1, 2, 3, Vector3.Down);
            
            // Top face (normal facing up)
            AddQuadWithNormals(verticesList, indicesList, 7, 6, 5, 4, Vector3.Up);
            
            // Side faces (normals facing outward)
            AddQuadWithNormals(verticesList, indicesList, 0, 4, 5, 1, Vector3.Backward);
            AddQuadWithNormals(verticesList, indicesList, 1, 5, 6, 2, Vector3.Right);
            AddQuadWithNormals(verticesList, indicesList, 2, 6, 7, 3, Vector3.Forward);
            AddQuadWithNormals(verticesList, indicesList, 3, 7, 4, 0, Vector3.Left);
            
            // 2. CREATE THE TREE FOLIAGE (LOWER SECTION)
            // We'll use a simple cone approximation for the leaves
            
            // Get the current vertex count to use as an offset for indices
            int baseVertex = verticesList.Count;
            float leafRadius = 0.4f;       // Base radius of the cone
            float leafHeight = 0.7f;       // Height of the cone
            
            // Position the leaf cone on top of the trunk
            Vector3 leafBase = trunkBase + new Vector3(0, trunkHeight, 0);
            Vector3 leafTop = leafBase + new Vector3(0, leafHeight, 0);
            
            // Add the top vertex of the cone
            verticesList.Add(new CustomVertexPositionNormalTexture(leafTop, Vector3.Up, texCoord));
            
            // Add vertices in a circle for the base of the cone
            // More segments create a smoother cone
            int leafSegments = 8;
            for (int i = 0; i < leafSegments; i++)
            {
                // Calculate position around the circle using trigonometry
                float angle = i * MathHelper.TwoPi / leafSegments;
                float x = (float)System.Math.Sin(angle) * leafRadius;
                float z = (float)System.Math.Cos(angle) * leafRadius;
                
                // Calculate normal for each vertex pointing outward and upward
                Vector3 normal = Vector3.Normalize(new Vector3(x, 0.5f, z));
                
                verticesList.Add(new CustomVertexPositionNormalTexture(leafBase + new Vector3(x, 0, z), normal, texCoord));
            }
            
            // Add triangles connecting the top to each segment of the circle
            // Each triangle consists of the top vertex and two adjacent base vertices
            for (int i = 0; i < leafSegments; i++)
            {
                int next = (i + 1) % leafSegments;
                
                // Calculate face normal for this triangle
                Vector3 v0 = verticesList[baseVertex].Position;
                Vector3 v1 = verticesList[baseVertex + 1 + i].Position;
                Vector3 v2 = verticesList[baseVertex + 1 + next].Position;
                
                // Compute edges from the top vertex to the base vertices
                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;
                
                // The normal is perpendicular to these two edges (cross product)
                Vector3 normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
                
                // Define indices for the triangle
                int topIndex = baseVertex;
                int index1 = baseVertex + 1 + i;
                int index2 = baseVertex + 1 + next;
                
                // Add indices in counterclockwise order
                indicesList.Add((short)topIndex);
                indicesList.Add((short)index1);
                indicesList.Add((short)index2);
            }
            
            // Add the bottom face of the cone (optional, as it's not usually visible)
            for (int i = 1; i < leafSegments - 1; i++)
            {
                // Calculate face normal (pointing down)
                Vector3 normal = Vector3.Down;
                
                // Create triangles to fill the bottom circle
                indicesList.Add((short)(baseVertex + 1));
                indicesList.Add((short)(baseVertex + 1 + i + 1));
                indicesList.Add((short)(baseVertex + 1 + i));
            }
            
            // 3. CREATE SECOND LAYER OF FOLIAGE (UPPER SECTION)
            // Similar to the first layer but smaller and higher up
            baseVertex = verticesList.Count;
            leafBase = leafBase + new Vector3(0, leafHeight * 0.3f, 0);
            leafTop = leafBase + new Vector3(0, leafHeight * 0.7f, 0);
            leafRadius *= 0.7f; // Make this section narrower
            
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
            
            // Convert lists to arrays for better performance when uploading to GPU
            return (verticesList.ToArray(), indicesList.ToArray());
        }
        
        /// <summary>
        /// Helper method to add indices for a quad (two triangles) with proper normals
        /// </summary>
        /// <param name="vertices">The list of vertices to add to</param>
        /// <param name="indices">The list of indices to add to</param>
        /// <param name="a">Index of first vertex</param>
        /// <param name="b">Index of second vertex</param>
        /// <param name="c">Index of third vertex</param>
        /// <param name="d">Index of fourth vertex</param>
        /// <param name="normal">Normal vector for this face</param>
        private static void AddQuadWithNormals(List<CustomVertexPositionNormalTexture> vertices, List<short> indices, 
                                              int a, int b, int c, int d, Vector3 normal)
        {
            // Calculate start index for new vertices
            int baseIndex = vertices.Count;
            
            // Create new vertices with the correct normal for this face
            // This is important for proper lighting
            Vector2 texCoord = new Vector2(0.5f, 0.5f);
            vertices.Add(new CustomVertexPositionNormalTexture(vertices[a].Position, normal, texCoord));
            vertices.Add(new CustomVertexPositionNormalTexture(vertices[b].Position, normal, texCoord));
            vertices.Add(new CustomVertexPositionNormalTexture(vertices[c].Position, normal, texCoord));
            vertices.Add(new CustomVertexPositionNormalTexture(vertices[d].Position, normal, texCoord));
            
            // First triangle (vertices 0,1,2)
            indices.Add((short)(baseIndex + 0));
            indices.Add((short)(baseIndex + 1));
            indices.Add((short)(baseIndex + 2));
            
            // Second triangle (vertices 0,2,3) - completes the quad
            indices.Add((short)(baseIndex + 0));
            indices.Add((short)(baseIndex + 2));
            indices.Add((short)(baseIndex + 3));
        }
        
        /// <summary>
        /// Helper method to add indices for a quad (two triangles)
        /// </summary>
        /// <param name="indices">The list of indices to add to</param>
        /// <param name="a">Index of first vertex</param>
        /// <param name="b">Index of second vertex</param>
        /// <param name="c">Index of third vertex</param>
        /// <param name="d">Index of fourth vertex</param>
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
