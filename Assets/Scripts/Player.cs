using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private readonly Settings _settings = new Settings();
    public bool isGrounded;
    public bool isSprinting;
    public bool isMoving = false;

    private Transform _cam;
    private World _world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.25f;

    public int orientation;

    private float _horizontal;
    private float _vertical;
    private float _mouseHorizontal;
    private float _mouseVertical;
    private Vector3 _velocity;
    private float _verticalMomentum = 0;
    private bool _jumpRequest;

    public Transform highlightBlock;
    public Transform placeBlock;

    public float checkIncrement = 0.1f;
    public float reach = 8f;

    public Toolbar toolbar;

    private void Start()
    {
        if (Camera.main != null) _cam = Camera.main.transform;
        _world = GameObject.Find("World").GetComponent<World>(); // junk
        _world.inUI = false;
    }

    private void FixedUpdate()
    {
        if (_world.inUI) return;

        CalculateVelocity();

        transform.Translate(_velocity, Space.World);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            _world.inUI = !_world.inUI;

        if (_world.inUI) return;

        if (_jumpRequest)
            Jump();

        var xzDirection = transform.forward;
        xzDirection.y = 0;

        if (Vector3.Angle(xzDirection, Vector3.forward) <= 45)
            orientation = 0;
        else if (Vector3.Angle(xzDirection, Vector3.right) <= 45)
            orientation = 5;
        else if (Vector3.Angle(xzDirection, Vector3.back) <= 45)
            orientation = 1;
        else
            orientation = 4;

        GetPlayerInputs();
        PlaceCursorBlock();


        transform.Rotate(Vector3.up * (_mouseHorizontal * _settings.mouseSensitivity));
        _cam.Rotate(Vector3.right * (-_mouseVertical * _settings.mouseSensitivity));
    }

    private void Jump()
    {
        _verticalMomentum = jumpForce;
        isGrounded = false;
        _jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        // Affect vertical momentum with gravity
        if (_verticalMomentum > gravity)
            _verticalMomentum += Time.fixedDeltaTime * gravity;
        // If we�re sprinting, use sprint multiple
        if (isSprinting)
            _velocity = ((transform.forward * _vertical) + (transform.right * _horizontal)) * (Time.fixedDeltaTime * sprintSpeed);
        else
            _velocity = ((transform.forward * _vertical) + (transform.right * _horizontal)) * (Time.fixedDeltaTime * walkSpeed);

        // Apply vertical momentum(falling/jumping)
        _velocity += Vector3.up * (_verticalMomentum * Time.fixedDeltaTime);

        var isLineMoving = (_velocity.z > 0 && Front) || (_velocity.z < 0 && Back);
        var isLateralMoving = (_velocity.x > 0 && Right) || (_velocity.x < 0 && Left);
        var isFalling = _velocity.y < 0;
        var isJumping = _velocity.y > 0;

        if (isLineMoving)
            _velocity.z = 0;
        if (isLateralMoving)
            _velocity.x = 0;
        if (isFalling)
            _velocity.y = CheckDownSpeed(_velocity.y);
        if (isJumping)
            _velocity.y = CheckUpSpeed(_velocity.y);
        if (isLateralMoving || isLineMoving)
            isMoving = true;
    }

    private void GetPlayerInputs()
    {
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
        _mouseHorizontal = Input.GetAxis("Mouse X");
        _mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            _jumpRequest = true;

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
            var position = placeBlock.position;
            _world.GetChunkForVector3(position).EditVoxel(position, toolbar.slots[toolbar.slotIndex].itemSlot.itemStack.id);
            toolbar.slots[toolbar.slotIndex].itemSlot.Take(1);
        }
    }

    private void DestroyBlock()
    {
        if (!(_world.blockTypes[_world.GetVoxel(highlightBlock.position)].density < 100)) return;
        var position = highlightBlock.position;
        _world.GetChunkForVector3(position).EditVoxel(position, 0);
    }

    private void PlaceCursorBlock()
    {
        var step = checkIncrement;
        var lastPos = new Vector3();
        while (step < reach)
        {
            var pos = _cam.position + (_cam.forward * step);

            if (_world.CheckForVoxel(pos))
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
        var position = transform.position;
        var isCollided =
           _world.CheckForVoxel(new Vector3(position.x - playerWidth, position.y + downSpeed, position.z - playerWidth))
        || _world.CheckForVoxel(new Vector3(position.x + playerWidth, position.y + downSpeed, position.z - playerWidth))
        || _world.CheckForVoxel(new Vector3(position.x + playerWidth, position.y + downSpeed, position.z + playerWidth))
        || _world.CheckForVoxel(new Vector3(position.x - playerWidth, position.y + downSpeed, position.z + playerWidth));

        isGrounded = isCollided;

        if (isGrounded) return 0;
        else return downSpeed;
    }

    private float CheckUpSpeed(float upSpeed)
    {
        var position = transform.position;
        var isCollided =
            _world.CheckForVoxel(new Vector3(position.x - playerWidth, position.y + 2f + upSpeed, position.z - playerWidth))
         || _world.CheckForVoxel(new Vector3(position.x + playerWidth, position.y + 2f + upSpeed, position.z - playerWidth))
         || _world.CheckForVoxel(new Vector3(position.x + playerWidth, position.y + 2f + upSpeed, position.z + playerWidth))
         || _world.CheckForVoxel(new Vector3(position.x - playerWidth, position.y + 2f + upSpeed, position.z + playerWidth));

        if (isCollided) return 0;
        else return upSpeed;
    }

    private bool Front
    {
        get
        {
            var position = transform.position;
            var isCollided =
                   _world.CheckForVoxel(new Vector3(position.x, position.y, position.z + playerWidth))
                || _world.CheckForVoxel(new Vector3(position.x, position.y + 1f, position.z + playerWidth));

            return isCollided;
        }
    }

    private bool Back
    {
        get
        {
            var position = transform.position;
            var isCollided =
                   _world.CheckForVoxel(new Vector3(position.x, position.y, position.z - playerWidth))
                || _world.CheckForVoxel(new Vector3(position.x, position.y + 1f, position.z - playerWidth));

            return isCollided;
        }
    }

    private bool Left
    {
        get
        {
            var position = transform.position;
            var isCollided =
                _world.CheckForVoxel(new Vector3(position.x - playerWidth, position.y, position.z))
             || _world.CheckForVoxel(new Vector3(position.x - playerWidth, position.y + 1f, position.z));

            return isCollided;
        }
    }

    private bool Right
    {
        get
        {
            var position = transform.position;
            var isCollided =
                _world.CheckForVoxel(new Vector3(position.x + playerWidth, position.y, position.z))
             || _world.CheckForVoxel(new Vector3(position.x + playerWidth, position.y + 1f, position.z));

            return isCollided;
        }
    }
}
