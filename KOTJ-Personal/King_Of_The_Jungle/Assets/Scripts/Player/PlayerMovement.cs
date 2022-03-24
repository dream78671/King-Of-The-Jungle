using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator anim;
    private PhotonView PV;
    private PlayerController Player; 

    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] int maxJumps;
    private int jumpCount;

    private bool grounded = true;
    private bool running = false;

    //void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(running);
    //        stream.SendNext(grounded);
    //    }
    //    else if (stream.IsReading)
    //    {
    //        running = (bool)stream.ReceiveNext();
    //        grounded = (bool)stream.ReceiveNext();
    //    }
    //}

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>(); //Gets component from game object from inspector tab
        anim = GetComponent<Animator>();
        PV = GetComponent<PhotonView>();
        Player = GetComponent<PlayerController>();
        body.freezeRotation = true;
        jumpCount = maxJumps;
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        if (!Player.canMove)
        {
            anim.GetComponent<Animator>().enabled = false;
            return;
        }

        anim.GetComponent<Animator>().enabled = true;

        float horizontalInput = Input.GetAxis("Horizontal"); //Store horizontal Input (-1, 0 ,1)

        body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

        //Flip player when changing direction
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        //Jump - GetKeyDown used to only register the initial click, not holding the space bar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount > 0)
                Jump();
        }

        running = horizontalInput != 0;

        //If character is moving, turn run animation on, else turn it off
        anim.SetBool("Run", running);
        anim.SetBool("Grounded", grounded);
    }

    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, jumpPower);
        anim.SetBool("Jump", !grounded);
        jumpCount -= 1;
        grounded = false;

    }

    //When player collides with Ground, reset number of jumps
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            anim.SetBool("Jump", !grounded);
            grounded = true;
        }
        jumpCount = maxJumps;
    }
}
