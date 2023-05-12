using UnityEngine;

public class BubbleEvent : MonoBehaviour
{
    private void OnParticleCollision(GameObject other)
    {
        if (other.transform.name.Equals("RobotCollider"))
        {
            BubbleLauncher.Instance.OnBubbleTouched();
        }
    }
}