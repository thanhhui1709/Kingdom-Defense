using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util 
{
    public static HashSet<GameObject> FindGameObjectInRange(HashSet<GameObject> existedTarget,Transform center, float range,string tag)
    {
        
        Collider[] hits = Physics.OverlapSphere(center.transform.position, range);
        foreach (var hit in hits)
        {
            if(hit.gameObject.CompareTag(tag) && hit.gameObject != center.gameObject)
            {
                existedTarget.Add(hit.gameObject);
            }
        }
        return existedTarget;
    }
    public static void RotateToward(Transform from, Vector3 to, float speed)
    {
        Vector3 direction = (to - from.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction,from.up);
        from.rotation = Quaternion.Lerp(from.rotation, lookRotation, Time.deltaTime * speed);
    }
    public static Vector3 MoveToward(Rigidbody from, Transform to, float speed)
    {
        Vector3 direction = (to.position - from.position).normalized;
        from.linearVelocity = direction * speed;
        //if(Vector3.Distance(from.position, to.position) < 0.1f)
        //{
        //    from.linearVelocity = Vector3.zero;
        //}
        return direction;
    }   
}
