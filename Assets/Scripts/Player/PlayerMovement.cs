using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public CharacterController controller;

    private Vector3 velocity;

    public Transform groundCheck;
    public float groundDistance = .4f;
    public LayerMask groundMaskRock;
    public LayerMask groundMaskDirt;
    public LayerMask groundMask;
    public bool isGroundedRock;
    public bool isGroundedDirt;
    public bool isGroundedOther;

    public bool isGrounded
    {
        get { return isGroundedRock || isGroundedDirt || isGroundedOther; }
    }

    public bool canJump = false;
    public bool canMove = false;

    private Vector3[] oldPos;
    public int posesCount;

    public float walkAudioSpeed = .4f;

    public GameObject AudioPlayerPrefab;
    public AudioClip[] SFXWalkGround;
    public AudioClip[] SFXWalkRock;

    private void Start()
    {
        oldPos = new Vector3[posesCount];

        //put this back on whn adding sfx to the game
        //InvokeRepeating("WalkSound", 0f, walkAudioSpeed);
    }

    private void Update()
    {
        isGroundedRock = Physics.CheckSphere(groundCheck.position, groundDistance, groundMaskRock);
        isGroundedDirt = Physics.CheckSphere(groundCheck.position, groundDistance, groundMaskDirt);
        isGroundedOther = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (canMove)
        {
            Vector3 move = transform.right * x + transform.forward * z;

            controller.Move(move * speed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        AddPos(transform.position);
    }

    public Vector3 VectorSpeed()
    {
        Vector3 temp = Vector3.zero;
        for (int i = 0; i < posesCount; i++)
        {
            temp += oldPos[i];
        }
        temp /= posesCount;
        temp -= transform.position;
        return temp * (1f / (posesCount * Time.fixedDeltaTime));
    }

    public float FloatSpeed()
    {
        Vector3 temp = VectorSpeed();
        return Mathf.Sqrt(Mathf.Pow(temp.x, 2) + Mathf.Pow(temp.y, 2) + Mathf.Pow(temp.z, 2));
    }

    public bool isMoving()
    {
        Vector3 temp = VectorSpeed();
        return Mathf.Sqrt(Mathf.Pow(temp.x, 2) + Mathf.Pow(temp.z, 2)) > .1f;
    }

    private void AddPos(Vector3 pos)
    {
        for (int i = posesCount - 1; i >= 1; i--)
        {
            oldPos[i] = oldPos[i - 1];
        }
        oldPos[0] = pos;
    }

    public void Stop()
    {
        velocity = Vector3.zero;
    }

    private void WalkSound()
    {
        if (isMoving() && isGrounded)
        {
            //play audio
            GameObject temp = Instantiate(AudioPlayerPrefab);
        }
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}
