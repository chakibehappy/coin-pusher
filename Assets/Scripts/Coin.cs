using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    Rigidbody coinRb;
    float coinDiameter;
    public bool isSpawnedByPlayer = false;
    public int stackLevel = 0;

    private void Start()
    {
        coinRb = GetComponent<Rigidbody>();
        coinDiameter = GetComponent<Collider>().bounds.size.x;
    }


    public void DropingDown(Vector3 coinFallingPoint, float force = 1f)
    {
        coinRb.isKinematic = false;

        // Direction towards the falling point
        Vector3 fallDirection = (coinFallingPoint - coinRb.position).normalized;

        // Apply movement towards the falling point
        StartCoroutine(FallToPoint(coinFallingPoint, fallDirection, force));
    }

    private IEnumerator FallToPoint(Vector3 targetPoint, Vector3 direction, float force)
    {
        float threshold = 0.05f; // Stopping distance

        while (Vector3.Distance(coinRb.position, new Vector3(coinRb.position.x, coinRb.position.y, targetPoint.z)) > threshold)
        {
            // Move the coin smoothly towards the target
            coinRb.MovePosition(Vector3.MoveTowards(coinRb.position, new Vector3(coinRb.position.x, coinRb.position.y, targetPoint.z), Time.deltaTime * force));

            yield return null;
        }

        // Ensure the coin lands exactly at the target
        coinRb.MovePosition(new Vector3(coinRb.position.x, coinRb.position.y, targetPoint.z));
    }

    public void MoveForwardByForce(float force = 2f)
    {
        if (coinRb == null || Camera.main == null) return; 
        Vector3 forceDirection = -Camera.main.transform.forward * force;
        coinRb.AddForce(forceDirection, ForceMode.Impulse);
    }
}
