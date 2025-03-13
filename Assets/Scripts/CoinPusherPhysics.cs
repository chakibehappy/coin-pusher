using System.Collections.Generic;
using UnityEngine;

public class CoinPusherPhysics : MonoBehaviour
{
    public float speed = 2f;
    public Transform startPos, endPos;
    Rigidbody rb;
    bool movingToEnd = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 targetPos = movingToEnd ? endPos.position : startPos.position;

        // Move using Rigidbody
        rb.MovePosition(Vector3.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime));

        // Check if reached the target
        if (Vector3.Distance(rb.position, targetPos) < 0.01f)
        {
            movingToEnd = !movingToEnd; // Reverse direction
        }
    }

    void ApplyForceToChildren()
    {
        foreach (Transform child in transform)
        {
            Coin coin = child.GetComponent<Coin>();
            if (coin != null)
            {
                Debug.Log("Apply Force");
                coin.transform.SetParent(null);
                coin.MoveForwardByForce();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Coin coin = collision.gameObject.GetComponent<Coin>();
            if (coin.isSpawnedByPlayer)
            {
                Rigidbody coinRb = collision.gameObject.GetComponent<Rigidbody>();
                if (coinRb != null)
                {
                    //coinRb.isKinematic = true;  // Stop physics simulation
                    collision.transform.SetParent(transform);
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Rigidbody coinRb = collision.gameObject.GetComponent<Rigidbody>();
            if (coinRb != null)
            {
                //coinRb.isKinematic = false;  // Resume physics when falling
                collision.transform.SetParent(null);
            }
        }
    }
}
