using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;

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

    [SerializeField] private Transform spinWheelUI;
    [SerializeField] private Transform spinWheelObj;
    [SerializeField] private SpinWheel spinWheel;

    public int minSpinwheelCount = 5;
    public int maxSpinwheelCount = 10;
    int currentSpinwheelCount = 0;
    int currentBetCount;

    public GameObject redGemPrefab;

    public Transform coinFallingPoint;

    private void Start()
    {
        currentBetCount = 0;
        spinWheelUI.gameObject.SetActive(false);
        AssignLaunchButton();
    }

    IEnumerator ShowSpinWheelIE()
    {
        spinWheelObj.localScale = Vector3.zero;
        spinWheelUI.gameObject.SetActive(true);

        spinWheelUI.GetComponent<Image>().DOFade(200f/255f, 0.5f);
        spinWheelObj.DOScale(Vector3.one, 0.5f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.5f);

        int result = Random.Range(0, spinWheel.segmentLabels.Length);

        yield return StartCoroutine(spinWheel.StartSpinIE(result));

        //Debug.Log(spinWheel.segmentLabels[result]);

        spinWheelObj.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear);
        spinWheelUI.GetComponent<Image>().DOFade(0, 0.5f);
        yield return new WaitForSeconds(0.5f);

        spinWheelUI.gameObject.SetActive(false);

        if(spinWheel.segmentLabels[result] == "Red Diamond")
        {

            Vector3 pos = new Vector3(
                Random.Range(coinSpawner.minX.position.x, coinSpawner.maxX.position.x), 
                maxX.position.y, 
                Random.Range(coinSpawner.minZ.position.z, coinSpawner.maxZ.position.z));
            GameObject redGem = Instantiate(redGemPrefab, pos, Quaternion.Euler(Random.Range(0, 360), 0, 0));
        }
        else if (spinWheel.segmentLabels[result] == "Black Hole")
        {

        }

        EnableInsertCoin(true);
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
        EnableInsertCoin(false);
        StartCoroutine(InsertCoinIE(coinCount));
    }

    IEnumerator InsertCoinIE(int coinCount)
    {
        if(currentSpinwheelCount == 0)
        {
            currentSpinwheelCount = Random.Range(minSpinwheelCount, maxSpinwheelCount);
        }
        currentBetCount++;

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
            coin.DropingDown(coinFallingPoint.position, 0.75f);

            yield return new WaitForSeconds(delayPerCoin);
        }

        EnableInsertCoin(currentBetCount < currentSpinwheelCount);

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

        if(currentBetCount >= currentSpinwheelCount)
        {
            currentBetCount = currentSpinwheelCount = 0;
            StartCoroutine(ShowSpinWheelIE());
        }
    }

    void EnableInsertCoin(bool enable = true)
    {
        CanPressLaunch = enable;
        launchButton1.gameObject.SetActive(enable);
        launchButton2.gameObject.SetActive(enable);
    }
}
