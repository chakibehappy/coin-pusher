using UnityEngine;
using DG.Tweening;
using System.Collections;

public class SpinWheel : MonoBehaviour
{
    public string[] segmentLabels;
    public float spinDuration = 5f;
    public int minFullSpin = 5;
    public int maxFullSpin = 10;
    public int bounceBackSpinLimit = 6;
    public float overlapDegree = 2;
    public float bounceMultiplier = 0.05f;
    public float bounceDelayMultiplier = 0.05f;
    private RectTransform wheelTransform;

    public int desiredIndex = -1;   // -1 for random spin, otherwise set to specific segment index
    public bool spinClockwise;

    void Start()
    {
        wheelTransform = GetComponent<RectTransform>();
    }

    public IEnumerator StartSpinIE(int result = -1)
    {
        if(result >= 0)
        {
            desiredIndex = result;
        }

        int extraSpins = Random.Range(minFullSpin, maxFullSpin); 
        float targetRotation = GetFinalRotationForSegment(desiredIndex);

        float finalRotation = !spinClockwise
            ? (targetRotation + extraSpins * 360)
            : (targetRotation - extraSpins * 360);

        float additionalSpinDuration = extraSpins > bounceBackSpinLimit ? bounceDelayMultiplier * extraSpins : 0;
        wheelTransform.DORotate(new Vector3(0, 0, finalRotation), spinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                if(extraSpins > bounceBackSpinLimit)
                {
                    float bounceBack = (bounceMultiplier * extraSpins); 
                    wheelTransform.DORotate(new Vector3(0, 0, finalRotation - bounceBack), additionalSpinDuration)
                        .SetEase(Ease.InOutSine);
                }
            });

        yield return new WaitForSeconds(spinDuration + additionalSpinDuration);
    }

    float GetFinalRotationForSegment(int index)
    {
        if (index < 0)
            return Random.Range(0, 360);

        int segments = segmentLabels.Length;
        float segmentSize = 360f / segments;
        return Random.Range((index * segmentSize) + overlapDegree, ((index+1) * segmentSize) - overlapDegree);
    }
}
