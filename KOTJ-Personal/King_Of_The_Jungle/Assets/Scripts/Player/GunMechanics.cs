using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.IO;
using Photon.Realtime;

public class GunMechanics : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject ItemHolder;
    [SerializeField] private Transform Gun;
    [SerializeField] private Transform ShootPoint;
    [SerializeField] Item[] items;

    private int itemIndex = -1;
    private int previousItemIndex = -1; 

    private PlayerController LocalPlayer;

    private float ProjectileSpeed = 1000; 
    Vector2 direction;
    int dirMultiplier; 

    PhotonView PV;

    void Start()
    {
        PV = GetComponent<PhotonView>();
        LocalPlayer = GetComponent<PlayerController>(); 
    }


    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine || !LocalPlayer.canShoot)
        {
            ItemHolder.SetActive(false);
            return; 
        }

        if (!ItemHolder.activeInHierarchy)
            ItemHolder.SetActive(true);

        //Switch to next/previous gun
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (itemIndex == -1)
                itemIndex = 0;
            else if (itemIndex + 1 > items.Length - 1)
                itemIndex = 0;
            else
                itemIndex += 1;

            EquipItem(itemIndex);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (itemIndex == -1)
                itemIndex = 0;
            else if (itemIndex - 1 < 0)
                itemIndex = items.Length - 1;
            else
                itemIndex -= 1;

            EquipItem(itemIndex);
        }

        //MousePos - Relative to whole screen, Direction - Relative to Player
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = mousePos - (Vector2)Gun.position;

        if(CheckValidDirection())
            FaceMouse();

        if (Input.GetMouseButtonDown(0) && LocalPlayer.canShoot && itemIndex != -1)
        {
            items[itemIndex].Use(dirMultiplier);
            LocalPlayer.canShoot = false;
            if (itemIndex != 0)
            {
                LocalPlayer.canMove = false; 
            }
        }
    }

    //Face Mouse according to local scale. Stops gun from doing '360'. 
    void FaceMouse()
    {
        if (transform.localScale == Vector3.one)
        {
            dirMultiplier = 1;
            Gun.transform.right = dirMultiplier * direction;
        }
        else
        {
            dirMultiplier = -1;
            Gun.transform.right = dirMultiplier * direction;
        }
            
    }

    //Checks if gun is pointed in the same direction as player
    bool CheckValidDirection()
    {
        //if player is looking right and mouse is pointed right of player - true else false
        if (direction.x >= 0 && transform.localScale == Vector3.one)
            return true;
        else if (direction.x <= 0 && transform.localScale == new Vector3(-1, 1, 1))
            return true;
        else
            return false;
    }

    //Equip player weapon
    void EquipItem(int _index)
    {
        //If the player is trying to switch to the weapon already equipped return
        if (_index == previousItemIndex)
            return;

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);

        //If the previous item is showing, hide the gameObject
        if (previousItemIndex != -1)
            items[previousItemIndex].itemGameObject.SetActive(false);

        previousItemIndex = itemIndex;
    }

}
