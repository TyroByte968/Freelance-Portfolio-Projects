using UnityEngine;
using UnityEngine.Events;
public class SoundTrigger : MonoBehaviour
{
    //a simple script that sets the player's footsteps to a higher valye on trigger enter
    public UnityEvent OnEnterEvent;
    public UnityEvent OnExitEvent;

    private void OnTriggerEnter(Collider other) 
    {
        if(other.tag == "Player")
        {
            OnEnterEvent.Invoke();
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if(other.tag == "Player")
        {
            OnExitEvent.Invoke();
        }
    }

}
