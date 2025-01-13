using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f; // Speed of the player
    public float gravity = 10.0f;
    private CharacterController cc;

    void Start()
    {
        // Get the Rigidbody component from the GameObject this script is attached to
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Get input from the keyboard
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calculate the movement direction
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        if (!cc.isGrounded)
        {
            movement.y -= gravity * Time.deltaTime;
        }

        // Apply the movement to the Rigidbody
        cc.Move(movement.normalized * speed * Time.deltaTime);

        
    }
}