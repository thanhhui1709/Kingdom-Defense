using UnityEngine;

public class OrientedArrow : MonoBehaviour
{
    private GameObject target;
    [SerializeField] private float speed = 10f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            Util.MoveToward(transform, target.transform, speed);
            Util.RotateToward(transform, target.transform, 180f);
        }
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
