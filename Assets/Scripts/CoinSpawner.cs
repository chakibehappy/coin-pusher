using UnityEngine;
using System.Collections.Generic;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab; // Assign your coin prefab
    public int maxCoins = 20; // Max number of coins
    public float stackHeight = 0.2f; // Height of each stacked coin
    public PhysicsMaterial slipperyMaterial;
    public PhysicsMaterial staticMaterial;
    public Transform minX, maxX, minZ, maxZ;

    private Dictionary<Vector2, int> coinStacks = new Dictionary<Vector2, int>();
    private float platformTopY;
    private float coinDiameter;

    public Transform platformParent;
    public float platformAngle = -5;


    public float stackOffsetRange = 0.01f;

    [SerializeField] private MainGame game;

    void Start()
    {

        // Get platform's top Y position
        Collider platformCollider = GetComponent<Collider>();
        platformTopY = platformCollider ? platformCollider.bounds.max.y : transform.position.y;

        // Get coin diameter dynamically
        GameObject tempCoin = Instantiate(coinPrefab);
        Collider coinCollider = tempCoin.GetComponent<Collider>();
        coinDiameter = coinCollider.bounds.size.x; // Use X size as diameter
        stackHeight = coinCollider.bounds.size.y;
        Destroy(tempCoin); // Cleanup

        SpawnCoins();
    }

    void SpawnCoins()
    {
        game.coinsOnPlatform = new();

        platformParent.transform.rotation = Quaternion.identity;
        int coinCount = 0;

        while (coinCount < maxCoins)
        {
            // Snap positions to grid based on coin size
            float randomX = Mathf.Round(Random.Range(minX.position.x, maxX.position.x) / coinDiameter) * coinDiameter;
            float randomZ = Mathf.Round(Random.Range(minZ.position.z, maxZ.position.z) / coinDiameter) * coinDiameter;
            Vector2 key = new Vector2(randomX, randomZ);

            // Get stack height for this position
            int stackLevel = coinStacks.ContainsKey(key) ? coinStacks[key] : 0;
            float yPosition = platformTopY + (stackLevel * stackHeight);

            // Apply random slight offset for a natural stack (only for 2nd coin and above)
            float offsetX = (stackLevel > 0) ? Random.Range(-stackOffsetRange, stackOffsetRange) : 0;
            float offsetZ = (stackLevel > 0) ? Random.Range(-stackOffsetRange, stackOffsetRange) : 0;

            // Spawn the coin
            GameObject coin = Instantiate(coinPrefab,
                new Vector3(randomX + offsetX, yPosition, randomZ + offsetZ),
                Quaternion.Euler(90, 0, 0));

            

            bool is2ndFrontCoin = (randomZ <= minZ.position.z + (coinDiameter * 2));
            if (is2ndFrontCoin)
            {
                //coin.GetComponent<Renderer>().material.color = Color.green;
            }
            
            // set 1st and 2nd front coins to be kinematic
            bool isFrontCoin = (randomZ <= minZ.position.z + (coinDiameter));
            if (isFrontCoin)
            {
                //coin.GetComponent<Renderer>().material.color = Color.red;
            }

            coin.GetComponent<Rigidbody>().isKinematic = isFrontCoin || is2ndFrontCoin;

            // Apply physics materials
            Collider coinCol = coin.GetComponent<Collider>();
            if (coinCol)
            {
                coinCol.material = (stackLevel == 0) ? slipperyMaterial : staticMaterial;
                //coinCol.material = staticMaterial;
            }
            coin.transform.SetParent(platformParent);
            coin.GetComponent<Coin>().stackLevel = stackLevel;
            game.coinsOnPlatform.Add(coin);

            // Update stack level
            coinStacks[key] = stackLevel + 1;

            coinCount++;
        }

        // Apply platform angle
        platformParent.transform.rotation = Quaternion.Euler(platformAngle, 0, 0);
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Coin coin = collision.gameObject.GetComponent<Coin>();
            if (coin.isSpawnedByPlayer)
            {
                coin.isSpawnedByPlayer = false;
                if (game.coinsOnPlatform.Contains(collision.gameObject))
                {
                    game.coinsOnPlatform.Add(collision.gameObject);
                }
            }
        }
    }

    public bool IsOnFrontAreaCollider(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }
        return obj.transform.position.z <= minZ.position.z + (coinDiameter);
    }
}
