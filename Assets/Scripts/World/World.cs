using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public int chunkSize = 16;
    public int chunkHeight = 256;
    public int worldSize = 16;
    private Chunk[,] chunks;
    public GameObject spherePrefab;

    private void Start()
    {
        chunks = new Chunk[worldSize, worldSize];

        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                Vector3 chunkPosition = new Vector3(x * chunkSize, 0, z * chunkSize);
                GameObject newChunkObject = new GameObject($"Chunk_{x}_{z}");
                newChunkObject.transform.position = chunkPosition;
              
                Chunk newChunk = newChunkObject.AddComponent<Chunk>();
                newChunk.material = material;
                newChunk.Initialize(chunkSize, chunkPosition);
                chunks[x, z] = newChunk;
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            HandleBlockClick();
        }
    }

    private void HandleBlockClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 hitPoint = hit.point + new Vector3(0.5f, 0.5f, 0.5f);

            Vector3 spawnPoint = hitPoint + new Vector3(0, 10f, 0);

            GameObject newSphere = Instantiate(spherePrefab, spawnPoint, Quaternion.identity);

            if (hit.normal == Vector3.up)
            {
                hitPoint.y -= 0.5f;
            }
            else if (hit.normal == Vector3.down)
            {
                hitPoint.y += 0.5f;
            }
            else if (hit.normal == Vector3.forward)
            {
                hitPoint.z -= 0.5f;
            }
            else if (hit.normal == Vector3.back)
            {
                hitPoint.z += 0.5f;
            }
            else if (hit.normal == Vector3.left)
            {
                hitPoint.x += 0.5f;
            }
            else if (hit.normal == Vector3.right)
            {
                hitPoint.x -= 0.5f;
            }

            Debug.Log($"World clicked at: {hitPoint}");

            int chunkX = Mathf.FloorToInt(hitPoint.x / chunkSize);
            int chunkZ = Mathf.FloorToInt(hitPoint.z / chunkSize);

            if (chunkX >= 0 && chunkX < worldSize && chunkZ >= 0 && chunkZ < worldSize)
            {
                Chunk clickedChunk = chunks[chunkX, chunkZ];

                Vector3 localBlockPos = new Vector3(hitPoint.x % chunkSize, hitPoint.y, hitPoint.z % chunkSize);
                clickedChunk.TryRemoveBlock(localBlockPos);
            }
        }
    }
}
