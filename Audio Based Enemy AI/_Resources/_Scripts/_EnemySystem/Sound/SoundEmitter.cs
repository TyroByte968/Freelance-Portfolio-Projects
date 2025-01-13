using UnityEngine;
using Homebrew;

public enum SurfaceAudioType
{
    LowSound,
    MediumSound,
    HighSound
};

public class SoundEmitter : MonoBehaviour
{
    [Foldout("Raycast Settings",true)]
    public float strideTime = 0.5f; // Time in seconds between strides
    public float RaycastDistance;

    [Foldout("Sound Emission Settings",true)]
    public SurfaceAudioType CurrentSurface;
    public GameObject LowSoundObject;
    public GameObject MediumSoundObject;
    public GameObject HighSoundObject;
    private float strideTimer;
    private CharacterController playerController;

    void Start()
    {
        playerController = GetComponent<CharacterController>();
        strideTimer = strideTime;
    }

    void Update()
    {
        // Check if the player is moving
        if (playerController.velocity.magnitude > 0.1f)
        {
            strideTimer -= Time.deltaTime;
            if (strideTimer <= 0)
            {
                PerformRaycast();
                strideTimer = strideTime;
            }
        }
    }

    RaycastHit hit;
    private void PerformRaycast()
    {
        if (Physics.Raycast(transform.position, Vector3.down*RaycastDistance, out hit))
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.red);
            InstaniateSoundObject((int)CurrentSurface,hit.transform);
        }
    }

    public void InstaniateSoundObject(int i,Transform spawnpos)
    {
        if(i == 0)
        {
            //Debug.Log("Spawn low sound object");
            Instantiate(LowSoundObject,spawnpos.position,Quaternion.identity);
        }
        else if(i == 1)
        {
            //Debug.Log("Spawn medium sound object");
            Instantiate(MediumSoundObject,spawnpos.position,Quaternion.identity);
        }
        else if(i == 2)
        {
            //Debug.Log("Spawn high sound object");
            Instantiate(HighSoundObject,spawnpos.position,Quaternion.identity);
        }
    }

    public void SetCurrentSurface(int i)
    {
        CurrentSurface = (SurfaceAudioType)i;
        Debug.Log("Set the surface type to: " + CurrentSurface);
    }


}

