using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util 
{
    public static List<GameObject> FindGameObjectInRange(Transform center, float range,string tag)
    {
        List<GameObject> gameObjectsInRange = new List<GameObject>();
        Collider[] hits = Physics.OverlapSphere(center.transform.position, range);
        foreach (var hit in hits)
        {
            if(hit.gameObject.CompareTag(tag) && hit.gameObject != center.gameObject)
            {
                gameObjectsInRange.Add(hit.gameObject);
            }
                
           
        }
        gameObjectsInRange.Sort((a, b) => Vector3.Distance(center.position, a.transform.position).CompareTo(Vector3.Distance(center.position, b.transform.position)));

        return gameObjectsInRange;
    }
    public static void RotateToward(Transform from, Transform to, float speed)
    {
        Vector3 direction = (to.position - from.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        from.rotation = Quaternion.Lerp(from.rotation, lookRotation, Time.deltaTime * speed);
    }
    public static void MoveToward(Transform from, Transform to, float speed)
    {
        Vector3 direction = (to.position - from.position).normalized;
        from.Translate(direction * speed * Time.deltaTime);
    }   
}
