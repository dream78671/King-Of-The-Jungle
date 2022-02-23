using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.IO;
using Photon.Realtime;

public class Shoot : MonoBehaviourPunCallbacks
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

        //Check numbers to see if gun switched
        for(int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
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

    
    void EquipItem(int _index)
    {
        if (_index == previousItemIndex)
            return;

        itemIndex = _index; 

        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
            items[previousItemIndex].itemGameObject.SetActive(false);

        previousItemIndex = itemIndex;

    }

}
