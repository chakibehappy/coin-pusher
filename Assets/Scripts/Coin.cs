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


    public void DropingDown(float force = 1f)
    {
        coinRb.isKinematic = false;
        coinRb.MovePosition(coinRb.position + (-Camera.main.transform.forward * coinDiameter * force));

        // TODO :
        // Make sure its really fall, we need to calculate the distance of the current coin to its fall point later
    }

    public void MoveForwardByForce(float force = 2f)
    {

        if (coinRb == null || Camera.main == null) return; // Safety check

        Vector3 forceDirection = -Camera.main.transform.forward * force;
        coinRb.AddForce(forceDirection, ForceMode.Impulse);
    }
}
