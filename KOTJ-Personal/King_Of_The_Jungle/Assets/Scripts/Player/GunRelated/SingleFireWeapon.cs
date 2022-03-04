using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

//All single fire weapons will use this class
public class SingleFireWeapon : Gun
{
    GameObject ProjectileSpawn;

    public override void Use(float direction)
    {
        Shoot(direction);
    }

    void Shoot(float direction)
    {
        //If left direction, flip projectile
        if(direction == -1)
            ProjectileSpawn = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", Projectile.name), ShootPoint.position, ShootPoint.rotation * Quaternion.Euler(0f,180f,0f));
        else
            ProjectileSpawn = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", Projectile.name), ShootPoint.position, ShootPoint.rotation);

        //Instantiate and fire projectile depending on forces passed thorugh unity.
        ProjectileSpawn.GetComponent<Rigidbody2D>().AddForce(ProjectileSpawn.transform.right * ProjectileSpeed);

        //Assigns GunInfo damage to the projectile so that it can be applied to player on colision. Check Projectile class
        ProjectileSpawn.GetComponent<Projectile>().damage=((GunInfo)itemInfo).damage;
    }
}
