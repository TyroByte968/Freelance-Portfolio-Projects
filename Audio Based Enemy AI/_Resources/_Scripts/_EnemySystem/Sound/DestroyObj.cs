using UnityEngine;

public class DestroyObj : MonoBehaviour
{
    public float delay = 2f;
    
    void Start()
    {
        Destroy(gameObject,delay);
    }

  
}
