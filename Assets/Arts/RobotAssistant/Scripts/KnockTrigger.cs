using com.HTC.Common;

using UnityEngine;

public class KnockTrigger : MonoBehaviour
{
    public AudioSource SFX;
    public AudioClip Knock;
    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "RobotCollider")
        {
            if (WhackAMole.Instance.GameState == WhackAMole.WhackAMoleGameState.Ready || WhackAMole.Instance.GameState == WhackAMole.WhackAMoleGameState.Start)
                WhackAMole.Instance.Knocking(transform.position);
            else if (RobotAssistantManager.robotAssistantManagerInstance.Knocking(transform.position))
            {
                SFX.PlayOneShot(Knock);
            }
        }

        if (other.GetComponent<PlanetsHandler>())
            other.GetComponent<PlanetsHandler>().Knocking(transform.position);
    }
}