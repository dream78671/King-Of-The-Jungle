using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage; //Damage passed from Weapon Class

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Damage sent when object instantiated
        collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage);

        Destroy(gameObject);

        

    }
}
