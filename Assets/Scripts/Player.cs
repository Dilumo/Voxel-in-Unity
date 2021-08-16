using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    Settings Settings = new Settings();
    public bool isGrounded;
    public bool isSprinting;
    public bool isMoving = false;

    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.25f;

    public int orientation;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    public Transform highlightBlock;
    public Transform placeBlock;

    public float checkIncrement = 0.1f;
    public float reach = 8f;

    public Toolbar toolbar;

    private void Start()
    {
        cam = Camera.main.transform;
        world = GameObject.Find("World").GetComponent<World>(); // junk
        world.inUI = false;
    }

    private void FixedUpdate()
    {
        if (world.inUI) return;

        CalculateVelocity();

        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            world.inUI = !world.inUI;

        if (world.inUI) return;

        if (jumpRequest)
            Jump();

        Vector3 XZDirection = transform.forward;
        XZDirection.y = 0;

        if (Vector3.Angle(XZDirection, Vector3.forward) <= 45)
            orientation = 0;
        else if (Vector3.Angle(XZDirection, Vector3.right) <= 45)
            orientation = 5;
        else if (Vector3.Angle(XZDirection, Vector3.back) <= 45)
            orientation = 1;
        else
            orientation = 4;

        GetPlayerInputs();
        PlaceCursorBlock();


        transform.Rotate(Vector3.up * mouseHorizontal * Settings.MouseSensitivity);
        cam.Rotate(Vector3.right * -mouseVertical * Settings.MouseSensitivity);
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        // Affect vertical momentum with gravity
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;
        // If we�re sprinting, use sprint mutipler
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // Apply vertical momentum(falling/jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        bool isLineMoving = (velocity.z > 0 && front) || (velocity.z < 0 && back);
        bool isLateralMoving = (velocity.x > 0 && right) || (velocity.x < 0 && left);
        bool isFalling = velocity.y < 0;
        bool isJumpimg = velocity.y > 0;

        if (isLineMoving)
            velocity.z = 0;
        if (isLateralMoving)
            velocity.x = 0;
        if (isFalling)
            velocity.y = CheckDownSpeed(velocity.y);
        if (isJumpimg)
            velocity.y = CheckUpSpeed(velocity.y);
        if (isLateralMoving || isLineMoving)
            isMoving = true;
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

        if (highlightBlock.gameObject.activeSelf)
        {
            // DESTROY
            if (Input.GetMouseButtonDown(0))
                DestroyBlock();
            // PLACE
            if (Input.GetMouseButtonDown(1))
                PlaceBlock();
        }
    }

    private void PlaceBlock()
    {
        if (toolbar.slots[toolbar.slotIndex].HasItem)
        {
            world.GetChunkForVector3(placeBlock.position).EditVoxel(placeBlock.position, toolbar.slots[toolbar.slotIndex].itemSlot.itemStack.id);
            toolbar.slots[toolbar.slotIndex].itemSlot.Take(1);
        }
    }

    private void DestroyBlock()
    {
        if (world.blockTypes[world.GetVoxel(highlightBlock.position)].density < 100)
            world.GetChunkForVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
    }

    private void PlaceCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();
        Vector3 pos = new Vector3();
        while (step < reach)
        {
            pos = cam.position + (cam.forward * step);

            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;
                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }
        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    private float CheckDownSpeed(float downSpeed)
    {
        bool isCollided =
           world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth))
        || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth))
        || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
        || world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth));

        isGrounded = isCollided;

        if (isGrounded) return 0;
        else return downSpeed;
    }

    private float CheckUpSpeed(float upSpeed)
    {
        bool isCollided =
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth))
         || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth))
         || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
         || world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth));

        if (isCollided) return 0;
        else return upSpeed;
    }

    public bool front
    {
        get
        {
            bool isCollided =
                   world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth))
                || world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth));

            return isCollided;
        }
    }
    public bool back
    {
        get
        {
            bool isCollided =
                   world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth))
                || world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth));

            return isCollided;
        }
    }
    public bool left
    {
        get
        {
            bool isCollided =
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z))
             || world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z));

            return isCollided;
        }
    }
    public bool right
    {
        get
        {
            bool isCollided =
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z))
             || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z));

            return isCollided;
        }
    }
}
