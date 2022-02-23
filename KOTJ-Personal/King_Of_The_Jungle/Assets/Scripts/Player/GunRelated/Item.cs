using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemInfo itemInfo;
    public GameObject itemGameObject;
    public GameObject Projectile;
    public Transform ShootPoint; 
    public float ProjectileSpeed;


    public abstract void Use(float direction);
}
