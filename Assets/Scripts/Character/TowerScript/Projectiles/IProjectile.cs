using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface  IProjectile 
{
    public void Launch(List<GameObject> target,float damage);
}
