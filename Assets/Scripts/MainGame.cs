using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using NUnit.Framework.Interfaces;

public class MainGame : MonoBehaviour
{
    public GameObject coinPrefab;

    public GameObject launchButton1, launchButton2;

    public Transform minX, maxX, minZ, maxZ;
    public float delayPerCoin = 0.05f;

    public List<GameObject> coinsOnPlatform = new();

    [SerializeField] private CoinSpawner coinSpawner;

    public List<GameObject> frontRowCoins = new();

    bool CanPressLaunch = true;


    private void Start()
    {
        AssignLaunchButton();
    }

    void AssignLaunchButton()
    {
        EventTrigger eventTrigger1 = launchButton1.AddComponent<EventTrigger>();
        EventTrigger.Entry entry1 = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entry1.callback.AddListener((data) => { InsertCoins(5); });
        eventTrigger1.triggers.Add(entry1);

        EventTrigger eventTrigger2 = launchButton2.AddComponent<EventTrigger>();
        EventTrigger.Entry entry2 = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entry2.callback.AddListener((data) => { InsertCoins(10); });
        eventTrigger2.triggers.Add(entry2);
    }

    void InsertCoins(int coinCount)
    {
        if (!CanPressLaunch)
            return;
        CanPressLaunch = false;
        launchButton1.gameObject.SetActive(false);
        launchButton2.gameObject.SetActive(false);
        StartCoroutine(InsertCoinIE(coinCount));
    }

    IEnumerator InsertCoinIE(int coinCount)
    {
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(minX.position.x, maxX.position.x), maxX.position.y, Random.Range(minZ.position.z, maxZ.position.z));
            GameObject coinObj = Instantiate(coinPrefab, pos, Quaternion.Euler(Random.Range(0, 360), 0, 0));
            coinObj.GetComponent<Coin>().isSpawnedByPlayer = true;
            yield return new WaitForSeconds(delayPerCoin);
        }

        // waiting for all coins let say down on platform
        yield return new WaitForSeconds(1f);
        frontRowCoins = frontRowCoins.Where(coin => coin != null).ToList();

        // TODO : also order by front to back later
        frontRowCoins = frontRowCoins.OrderByDescending(coin => coin.transform.position.y).ToList();

        List<GameObject> coinsToRemove = new();

        int randomCount = Random.Range(1, 10);
        Debug.Log("Target Coin to Drop : " + randomCount);

        //Debug.Log(frontRowCoins.Count);

        for (int i = 0; i < randomCount; i++)
        {
            Coin coin = frontRowCoins[i].GetComponent<Coin>();
            coinsToRemove.Add(frontRowCoins[i]);
            coin.DropingDown(0.75f);

            yield return new WaitForSeconds(delayPerCoin);
        }

        CanPressLaunch = true;
        launchButton1.gameObject.SetActive(true);
        launchButton2.gameObject.SetActive(true);

        // Remove marked coins after the loop
        foreach (var coin in coinsToRemove)
        {
            frontRowCoins.Remove(coin);
            coinsOnPlatform.Remove(coin);
        }

        //Debug.Log(frontRowCoins.Count);

        // Push all coin to forward by one diameter power & check the front row and set to kinematic
        for (int i = 0; i < coinsOnPlatform.Count; i++)
        {
            if(coinsOnPlatform[i] != null)
            {
                bool isFrontCoin = coinSpawner.IsOnFrontAreaCollider(coinsOnPlatform[i]);
                coinsOnPlatform[i].GetComponent<Rigidbody>().isKinematic = isFrontCoin;
                if (!isFrontCoin)
                {
                    Coin coin = coinsOnPlatform[i].GetComponent<Coin>();
                    
                    if(coin.stackLevel <= 2 && !coin.isSpawnedByPlayer)
                    {
                        coin.MoveForwardByForce(2.5f);
                    }
                }
            }
        }
    }
}
