using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class CoinDestroyer : MonoBehaviour
{
    public GameObject multiplierTextPrefab;
    public Transform startPosY, endPosY;
    public Transform canvas;

    public int totalCoinFall = 0;
    bool isCheckingCoinFall = false;
    public float fallSessionDelay = 2f;

    private void OnTriggerEnter(Collider other)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(other.transform.position);
        Vector3 spawnPos = new Vector3(screenPos.x, startPosY.position.y, 0);

        if (other.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            ShowMultiplierText(spawnPos, "0.5x", Color.white);
            
            totalCoinFall++;
            if (!isCheckingCoinFall)
            {
                StartCoroutine(CheckCoinFallIE());
            }
        }

        if (other.CompareTag("Gem"))
        {
            Destroy(other.gameObject);
            ShowMultiplierText(spawnPos, "25x", Color.green);
        }
    }

    IEnumerator CheckCoinFallIE()
    {
        isCheckingCoinFall = true;
        yield return new WaitForSeconds(fallSessionDelay);
        Debug.Log("Coin Fall : " + totalCoinFall);
        totalCoinFall = 0;
        isCheckingCoinFall=false;
    }

    void ShowMultiplierText(Vector3 spawnPos, string text, Color color)
    {
        GameObject textObj = Instantiate(multiplierTextPrefab, spawnPos, Quaternion.identity);
        textObj.GetComponent<TextMeshProUGUI>().text = text;
        textObj.GetComponent<TextMeshProUGUI>().color = color;
        textObj.transform.SetParent(canvas);

        textObj.transform.DOMoveY(endPosY.position.y, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            textObj.transform.DOPunchScale(Vector3.one, 1, 1).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(textObj);
            });
        });
    }
}


