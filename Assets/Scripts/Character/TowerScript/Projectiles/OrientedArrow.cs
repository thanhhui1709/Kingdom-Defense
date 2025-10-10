using UnityEngine;

public class OrientedArrow : MonoBehaviour
{
    private GameObject target;
    private Rigidbody rb;
    [SerializeField] private float speed = 10f;
    void Awake()
    {
        rb=GetComponent<Rigidbody>();
        if(rb==null) Debug.LogError("Rigidbody component is missing from the projectile.");

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null) return;

       Vector3 dir= Util.MoveToward(rb, target.transform, speed);
       Quaternion targetRotation= Quaternion.LookRotation(dir*180, transform.up)*Quaternion.Euler(90,0,0);
       rb.MoveRotation(targetRotation);
    }
    public void SetTarget(GameObject targetGO)
    {
       target= targetGO;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == target)
        {
            Destroy(gameObject);
            Debug.Log("Hitted Target Object");
        }
    }
}
