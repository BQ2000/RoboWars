using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : NetworkBehaviour {
    [SerializeField] private PlayerManager player;
    [SerializeField] private float runEnergyCost = 7f;
    [SerializeField] private float jumpEnergyCost = 7f;

    [SerializeField] private Transform headCam;
    [SerializeField] private Transform headVisor;
    [SerializeField] private Transform leftArmPivot;
    [SerializeField] private Transform rightArmPivot;

    Transform playerRef;
    private Rigidbody playerRb;

    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float jumpForce = 1000f;

    private Vector3 sideDirection;
    private Vector3 forwardDirection;
    private Vector3 backVelocity;
    private Vector3 velocity;       //Velocity
    private float xRotationFloat;
    private Vector3 xRotation;      //X Rotation
    private Vector3 yRotation;      //Y Rotation
    private bool jumped;
    private bool levitating;

    private bool needResetXRotation;
    private bool flyModeOn;
    private float shiftBuffFactor = 1f;

    private bool shapeShifted = false;
    private bool teleported = false;

    private void Start() {
        if (isLocalPlayer)
        {
            player = GetComponent<PlayerManager>();
            playerRb = GetComponent<Rigidbody>();
            playerRef = this.transform;
        }  
    }

    private void Update() {     //Record Movement Input
        if (isLocalPlayer)
        {
            RecordCurrentMotion(playerRef);
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyUp(KeyCode.Alpha0))
            {
                flyModeOn = !flyModeOn;
                Debug.Log("FLYMODE: " + flyModeOn);
            }
        }

    }

    private void LateUpdate()
    {
        if (needResetXRotation)
        {
            xRotationFloat = 0;
            xRotation = xRotationFloat * Vector3.left;
            headCam.transform.localRotation = Quaternion.Euler(xRotation);
            headVisor.transform.localRotation = Quaternion.Euler(xRotation);
            leftArmPivot.transform.localRotation = Quaternion.Euler(xRotation);
            rightArmPivot.transform.localRotation = Quaternion.Euler(xRotation);

            needResetXRotation = false;
        }
    }

    public void ResetXRotation()
    {
        needResetXRotation = true;
    }

    public Transform GetRightArmPivot()
    {
        return rightArmPivot;
    }

    public Transform GetLeftArmPivot()
    {
        return leftArmPivot;
    }

    private void FixedUpdate() {       //Move Player
        Move();
    }

    private void Move() {
        //Translation
        if (velocity != Vector3.zero) {

            playerRb.MovePosition(playerRb.position + velocity * Time.fixedDeltaTime);

        }

        if (jumped) {
            playerRb.AddForce(transform.up * jumpForce * Time.fixedDeltaTime, ForceMode.Acceleration);
            player.energy -= jumpEnergyCost;
            jumped = false;
        }

        //Rotation
        if (yRotation != Vector3.zero || xRotation != Vector3.zero) {
            transform.Rotate(yRotation);

            /*            headCam.transform.Rotate(xRotation);
                        Mathf.Clamp(headCam.localEulerAngles.x, 90, 270);*/
            headCam.transform.localRotation = Quaternion.Euler(xRotation);
            headVisor.transform.localRotation = Quaternion.Euler(xRotation);
            leftArmPivot.transform.localRotation = Quaternion.Euler(xRotation);
            rightArmPivot.transform.localRotation = Quaternion.Euler(xRotation);
        }
    }

    private void RecordCurrentMotion(Transform thing) {
        GetVelocity(thing);
        GetYRotation();
        GetXRotation();
        GetJump();
    }

    private void GetJump() {

        if (Input.GetKey(KeyCode.Space) && !levitating && player.energy >= jumpEnergyCost) {
            jumped = true;
        }

        Levitate();

    }

    void Levitate() {
        if (Input.GetKey(KeyCode.LeftShift) && player.energy > 0) {
            player.energy -= runEnergyCost * Time.deltaTime;
            if(player.energy >= runEnergyCost / 2)
            {
                if (flyModeOn)
                {
                    if (!shapeShifted)
                    {
                        CmdShapeShift(shapeShifted);
                    }
                    levitating = true;
                    playerRb.velocity = Vector3.zero;
                    playerRb.angularVelocity = Vector3.zero;
                    playerRb.useGravity = false;
                    playerRef = headCam;
                    shiftBuffFactor = 3f;

                    if (Input.GetKeyDown(KeyCode.F) && player.energy >= 10f)        //teleportation blink
                    {
                        player.transform.localPosition += playerRef.forward * 12f;
                        player.energy -= 10f;

                        needResetXRotation = true;

/*                        headCam.transform.eulerAngles *= -1f;


                        
                        xRotationFloat = 0;
                        xRotation = xRotationFloat * Vector3.left;*/

                        /*                        xRotation = Vector3.zero;
                                                xRotation = headCam.transform.eulerAngles;*/
                        /*headCam.transform.localRotation = Quaternion.Euler(xRotation);
                        headVisor.transform.localRotation = Quaternion.Euler(xRotation);
                        leftArmPivot.transform.localRotation = Quaternion.Euler(xRotation);
                        rightArmPivot.transform.localRotation = Quaternion.Euler(xRotation);*/

                    }
                }
                else
                {
                    shiftBuffFactor = 2f;
                }
            }
        } 
        else {
            if (shapeShifted)
            {
                CmdShapeShift(shapeShifted);
            }

            levitating = false;
            playerRb.useGravity = true;
            playerRef = this.transform;
            shiftBuffFactor = 1f;
        }
    }

    [Command]
    private void CmdShapeShift(bool shifted)
    {
        RpcShapeShift(shifted);
    }

    [ClientRpc]
    private void RpcShapeShift(bool shifted)
    {
        if (!shifted)
        {
            player.GetBody().localPosition += Vector3.up * 0.5f;
            player.GetBody().localScale -= Vector3.up * 0.5f;
            shapeShifted = true;
        }
        else if (shifted)
        {
            player.GetBody().localScale += Vector3.up * 0.5f;
            player.GetBody().position -= Vector3.up * 0.5f;
            shapeShifted = false;
        }
    }

    private void GetXRotation() {
        xRotationFloat += Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRotationFloat = Mathf.Clamp(xRotationFloat, -90f, 90f);
        xRotation = xRotationFloat * Vector3.left;

        /*        xRotation = Input.GetAxis("Mouse Y") * Vector3.left * mouseSensitivity;
                Debug.Log(headCam.transform.localEulerAngles.x);*/
    }

    private void GetYRotation() {
        yRotation = Input.GetAxis("Mouse X") * Vector3.up * mouseSensitivity;
    }

    private void GetVelocity(Transform thing) {
        sideDirection = thing.right * Input.GetAxisRaw("Horizontal") * 0.5f;
        if (Input.GetKey(KeyCode.W))
        {
            forwardDirection = (thing.forward + sideDirection).normalized;
            velocity = (forwardDirection) * movementSpeed;
            /*velocity *= shiftBuffFactor;*/
        } 
        else if (Input.GetKey(KeyCode.S))
        {
            forwardDirection = (-thing.forward + sideDirection).normalized * 0.5f;
            velocity = (forwardDirection) * movementSpeed;
        }
        else
        {
            forwardDirection = Vector3.zero;
            velocity = (sideDirection) * movementSpeed;
        }
        velocity *= shiftBuffFactor;
    }

}
