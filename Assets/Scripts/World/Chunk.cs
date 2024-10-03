using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkSize = 16;
    public int chunkHeight = 256;
    public BlockType[,,] blocks;
    public Material material;
    public Vector3 chunkPos;


    public void Initialize(int size, Vector3 chunkPos)
    {
        chunkSize = size;
        this.chunkPos = chunkPos;
        blocks = new BlockType[chunkSize, chunkHeight, chunkSize];
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        meshRenderer.material = material;
        InitializeChunk();
        GenerateMesh();
    }

    private void InitializeChunk()
    {
        float terrainScale = 5f;
        float heightScale = f; // Scale for height variation
        float featureScale = 0.3f; // Scale for feature randomness

        // Noise layers for terrain generation
        float[,] heightMap = new float[chunkSize, chunkSize];
        float[,] featureMap = new float[chunkSize, chunkSize];

        // Generate height map and feature map
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                // Use two layers of Perlin noise for more complexity
                float baseHeight = Mathf.PerlinNoise((x + chunkPos.x) * terrainScale, (z + chunkPos.z) * terrainScale) * heightScale;
                float featureNoise = Mathf.PerlinNoise((x + chunkPos.x) * featureScale, (z + chunkPos.z) * featureScale);

                heightMap[x, z] = baseHeight;
                featureMap[x, z] = featureNoise; // Random features like trees or rocks
            }
        }

        // Generate blocks based on height map
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int maxHeight = Mathf.FloorToInt(heightMap[x, z]);

                for (int y = 0; y < chunkHeight; y++)
                {
                    if (y < maxHeight)
                    {
                        // Determine block type with smooth transitions
                        if (y < maxHeight - 1)
                        {
                            blocks[x, y, z] = BlockType.Stone; // Stone below ground level
                        }
                        else if (y == maxHeight)
                        {
                            blocks[x, y, z] = BlockType.Grass; // Grass on the surface
                        }
                    }
                    else
                    {
                        blocks[x, y, z] = BlockType.Empty; // Air above ground
                    }
                }

                // Add features based on the feature map
                if (featureMap[x, z] > 0.8f)
                {
                    PlaceTree(x, maxHeight, z); // Randomly place a tree
                }
                else if (featureMap[x, z] > 0.6f)
                {
                    PlaceRock(x, maxHeight, z); // Randomly place a rock
                }
            }
        }
    }

    // Methods to place trees and rocks (you will need to implement these)
    private void PlaceTree(int x, int height, int z)
    {
        // Implementation for placing a tree
        blocks[x, height + 1, z] = BlockType.Stone; // Example for tree trunk
        blocks[x, height + 2, z] = BlockType.Grass; // Example for leaves
    }

    private void PlaceRock(int x, int height, int z)
    {
        // Implementation for placing a rock
        blocks[x, height + 1, z] = BlockType.Stone; // Example for rock
    }



    public void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if (blocks[x, y, z] == BlockType.Empty) continue;

                    Vector3 pos = new Vector3(x, y, z);
                    if (IsFaceVisible(x, y, z, Vector3.forward)) AddFace(vertices, triangles, uv, pos, Vector3.forward, blocks[x, y, z]);
                    if (IsFaceVisible(x, y, z, Vector3.back)) AddFace(vertices, triangles, uv, pos, Vector3.back, blocks[x, y, z]);
                    if (IsFaceVisible(x, y, z, Vector3.up)) AddFace(vertices, triangles, uv, pos, Vector3.up, blocks[x, y, z]);
                    if (IsFaceVisible(x, y, z, Vector3.down)) AddFace(vertices, triangles, uv, pos, Vector3.down, blocks[x, y, z]);
                    if (IsFaceVisible(x, y, z, Vector3.left)) AddFace(vertices, triangles, uv, pos, Vector3.left, blocks[x, y, z]);
                    if (IsFaceVisible(x, y, z, Vector3.right)) AddFace(vertices, triangles, uv, pos, Vector3.right, blocks[x, y, z]);
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = mesh; 
    }

    private bool IsFaceVisible(int x, int y, int z, Vector3 direction)
    {
        Vector3 neighborPos = new Vector3(x, y, z) + direction;
        int nx = Mathf.FloorToInt(neighborPos.x);
        int ny = Mathf.FloorToInt(neighborPos.y);
        int nz = Mathf.FloorToInt(neighborPos.z);

        if (nx < 0 || ny < 0 || nz < 0 || nx >= chunkSize || ny >= chunkSize || nz >= chunkSize) return true;
        return blocks[nx, ny, nz] == BlockType.Empty;
    }

    private void AddFace(List<Vector3> vertices, List<int> triangles, List<Vector2> uv, Vector3 position, Vector3 direction, BlockType blockType)
    {
        int startIndex = vertices.Count;

        Vector3[] faceVertices = new Vector3[4];

        if (direction == Vector3.forward)
        {
            faceVertices[0] = position + new Vector3(-0.5f, -0.5f, 0.5f);
            faceVertices[1] = position + new Vector3(0.5f, -0.5f, 0.5f);
            faceVertices[2] = position + new Vector3(0.5f, 0.5f, 0.5f);
            faceVertices[3] = position + new Vector3(-0.5f, 0.5f, 0.5f);
        }
        else if (direction == Vector3.back)
        {
            faceVertices[0] = position + new Vector3(0.5f, -0.5f, -0.5f);
            faceVertices[1] = position + new Vector3(-0.5f, -0.5f, -0.5f);
            faceVertices[2] = position + new Vector3(-0.5f, 0.5f, -0.5f);
            faceVertices[3] = position + new Vector3(0.5f, 0.5f, -0.5f);
        }
        else if (direction == Vector3.up)
        {
            faceVertices[0] = position + new Vector3(-0.5f, 0.5f, 0.5f);
            faceVertices[1] = position + new Vector3(0.5f, 0.5f, 0.5f);
            faceVertices[2] = position + new Vector3(0.5f, 0.5f, -0.5f);
            faceVertices[3] = position + new Vector3(-0.5f, 0.5f, -0.5f);
        }
        else if (direction == Vector3.down)
        {
            faceVertices[0] = position + new Vector3(-0.5f, -0.5f, -0.5f);
            faceVertices[1] = position + new Vector3(0.5f, -0.5f, -0.5f);
            faceVertices[2] = position + new Vector3(0.5f, -0.5f, 0.5f);
            faceVertices[3] = position + new Vector3(-0.5f, -0.5f, 0.5f);
        }
        else if (direction == Vector3.left)
        {
            faceVertices[0] = position + new Vector3(-0.5f, -0.5f, -0.5f);
            faceVertices[1] = position + new Vector3(-0.5f, -0.5f, 0.5f);
            faceVertices[2] = position + new Vector3(-0.5f, 0.5f, 0.5f);
            faceVertices[3] = position + new Vector3(-0.5f, 0.5f, -0.5f);
        }
        else if (direction == Vector3.right)
        {
            faceVertices[0] = position + new Vector3(0.5f, -0.5f, 0.5f);
            faceVertices[1] = position + new Vector3(0.5f, -0.5f, -0.5f);
            faceVertices[2] = position + new Vector3(0.5f, 0.5f, -0.5f);
            faceVertices[3] = position + new Vector3(0.5f, 0.5f, 0.5f);
        }

        vertices.AddRange(faceVertices);

        triangles.Add(startIndex);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 3);

        Vector2[] faceUVs = GetBlockUVs(blockType, direction);

        uv.AddRange(faceUVs);
    }

    private Vector2[] GetBlockUVs(BlockType blockType, Vector3 direction)
    {
        float tileSize = 0.5f;
        float uvShrinkAmount = 0.01f; 

        Vector2[] uvs = new Vector2[4];
        Vector2 tileOffset = Vector2.zero;

        if (blockType == BlockType.Grass)
        {
            if (direction == Vector3.up)
            {
                tileOffset = new Vector2(0, 0.5f);
            }
            else if (direction == Vector3.down)
            {
                tileOffset = new Vector2(0, 0);
            }
            else
            {
                tileOffset = new Vector2(0.5f, 0.5f);
            }
        }
        else if (blockType == BlockType.Stone)
        {
            tileOffset = new Vector2(0, 0);
        }

        uvs[0] = new Vector2(tileOffset.x + uvShrinkAmount, tileOffset.y + uvShrinkAmount);
        uvs[1] = new Vector2(tileOffset.x + tileSize - uvShrinkAmount, tileOffset.y + uvShrinkAmount); 
        uvs[2] = new Vector2(tileOffset.x + tileSize - uvShrinkAmount, tileOffset.y + tileSize - uvShrinkAmount); 
        uvs[3] = new Vector2(tileOffset.x + uvShrinkAmount, tileOffset.y + tileSize - uvShrinkAmount);

        return uvs;
    }


    public void TryRemoveBlock(Vector3 localPos)
    {
        Vector3Int blockPos = Vector3Int.FloorToInt(localPos);

        Debug.Log($"Attempting to remove block at local position: {blockPos}");
        blocks[blockPos.x, blockPos.y, blockPos.z] = BlockType.Empty;
        GenerateMesh();
    }

}

public enum BlockType
{
    Empty,
    Grass,
    Stone
}
