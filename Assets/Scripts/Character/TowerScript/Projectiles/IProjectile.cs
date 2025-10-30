using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface  IProjectile 
{
    public void Launch(Transform launchPoint, List<GameObject> target,float damage);
}
