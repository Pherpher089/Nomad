using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : FpsItem
{
    public GameObject bulletImpactPrefab;
    public abstract override void Use();
}
