using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage; //Damage passed from Weapon Class
    public float lifeTimeSeconds;
    private PhotonView PV; 

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Damage sent when object instantiated
        collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage);
        PhotonNetwork.Destroy(gameObject);
    }
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        Destroy(gameObject, lifeTimeSeconds);
    }

}
