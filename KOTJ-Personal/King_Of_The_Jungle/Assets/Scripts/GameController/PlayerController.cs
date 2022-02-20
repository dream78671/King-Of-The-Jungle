using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator anim;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] int maxJumps;
    private int jumpCount;
    private bool grounded;
    PhotonView PV;

    [SerializeField] private float timeLeft;
    private bool canMove = true; 


    private void Awake() //Called Everytime script is loaded
    {
        body = GetComponent<Rigidbody2D>(); //Gets component from game object from inspector tab
        anim = GetComponent<Animator>();
        PV = GetComponent<PhotonView>(); 
        jumpCount = maxJumps;
        Debug.Log("Spawned");
    }

    private void Update() //Runs every frame
    {
        //If view isn't mine, do nothing
        if (!PV.IsMine)
            Destroy(body); //Should delete body that isn't your own - stops buginess

        if (!canMove)
        {
            return;
        }

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

        //If character is moving, turn run animation on, else turn it off
        anim.SetBool("Run", horizontalInput != 0);
        anim.SetBool("Grounded", grounded);
    }




    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, jumpPower);
        anim.SetTrigger("Jump");
        jumpCount -= 1;
        grounded = false;
    }

    //When player collides with Ground, reset number of jumps
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            grounded = true;
        jumpCount = maxJumps;
    }

    //Player can attack if these criteria are met - Player is grounded
    public bool canAttack()
    {
        return grounded == true;
    }
}
