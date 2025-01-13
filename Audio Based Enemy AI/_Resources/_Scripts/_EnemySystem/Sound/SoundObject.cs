using UnityEngine;

public class SoundObject : MonoBehaviour
{
    public float SoundValue;
    public float radiusScaleValue = 20f;
    public float DestroyDelay = 10f;
    public Color GizmoColor;


    void Start()
    {
        //set the collider radius
        SetRadius();
        Destroy(gameObject,DestroyDelay);
    }

    // This function is called when the script is loaded or a value is changed in the Inspector
    // Updates the collider gizmo
    void OnValidate()
    {
        SetRadius();
    }

    private void SetRadius()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider != null)
        {
            collider.radius = SoundValue*radiusScaleValue;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.tag == "Enemy")
        {
            Debug.Log("Enemy heard the sound!");
            Enemy DetectedEnemy = other.GetComponent<Enemy>();

            //if sound value is less than or equal 0.6, set the enemy to alert mode   
            if(SoundValue <= 0.6f)
            {
                if(DetectedEnemy.enemyState != EnemyState.Chasing && DetectedEnemy.enemyState != EnemyState.Alert)
                {
                    DetectedEnemy.ListenBehavior(this.gameObject,SoundValue);
                    DetectedEnemy.EnemyWasAlert = true;
                }
            }
            //if sound value is greater than 0.6, set the enemy to flee mode 
            else if(SoundValue > 0.6f)
            {
                DetectedEnemy.enemyState = EnemyState.Fleeing;
            }
            

        }    
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(transform.position,SoundValue*radiusScaleValue);    
    }


}
