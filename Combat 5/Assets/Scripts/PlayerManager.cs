using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerShoot))]
public class PlayerManager : NetworkBehaviour {

    [SerializeField]
    private float maxHealth = 100f;
    [SyncVar]
    private float currentHealth;
    [SyncVar]
    private bool isDead = false;
    [SyncVar]
    private int kills = 0;
    [SyncVar]
    private int deaths = 0;

    public float maxEnergy = 200f;
    public float energy = 200f;
    [SerializeField]
    private float energyRegenRate = 3f;
    private bool reloading;

    [SerializeField]
    private Dictionary<Behaviour, bool> activeBehaviours = new Dictionary<Behaviour, bool>();
/*    [SerializeField]
    private PlayerNetworkSetup playerNetworkSetup;*/
    [SerializeField]
    private Rigidbody rigBody;
    private bool isFirstSetup = true;

    [SerializeField]
    private GameObject playerHeadCam;
/*    [SerializeField]
    private UIController UIController;*/

    //new method
    [SerializeField]
    private GameObject uiPrefab;
    private GameObject uiInstance;
    private PlayerUI playerUI;

    [SerializeField] private AudioSource startupSFX;
    [SerializeField] private AudioSource hitSFX;

    [SerializeField] private AudioSource shutdownSFX;
    [SerializeField] private AudioSource shutdownSFXVoice;

    [SerializeField] private AudioSource respawnSFX;
    [SerializeField] private AudioSource respawnSFXVoice;

    [SerializeField] private AudioSource scoreOnBot;
    [SerializeField] private AudioSource scoreOnPlayer;

    [SerializeField] private AudioSource taunt;
    [SerializeField] private AudioSource reload;
    [SerializeField] private AudioSource beep;

    private bool canReload = true;

    [SerializeField]
    GameObject body;
    [SerializeField]
    GameObject headVisor;

    [SerializeField] private Transform leftArmPivot;
    [SerializeField] private Transform rightArmPivot;
    [SerializeField] private Transform visorPivot;

    private PlayerManager killersManager;
    [SyncVar]
    private bool spawnProtected = false;

    private PlayerMove playerMove;
    private PlayerShoot playerShoot;


    [SerializeField] MeshRenderer visorRenderer;
    [SerializeField] MeshRenderer muzzleRendererL;
    [SerializeField] MeshRenderer muzzleRendererR;
    [SerializeField] Material visorOnMaterial;
    [SerializeField] Material visorOffMaterial;


    private void Start()
    {
        if (isLocalPlayer)
        {
            uiInstance = Instantiate(uiPrefab);
            playerUI = uiInstance.GetComponent<PlayerUI>();

            
            if (playerUI == null)
            {
                Debug.Log(this.name + " has no UI...");
            }
            transform.name = "Player " + GetComponent<NetworkIdentity>().netId.ToString();
            headVisor.layer = LayerMask.NameToLayer("Hide From Cam");
            rigBody = GetComponent<Rigidbody>();
            PlayerSetup();
        }

        if (!isLocalPlayer)
        {
            playerHeadCam.SetActive(false);
            body.layer = LayerMask.NameToLayer("Remote Players");
            headVisor.layer = LayerMask.NameToLayer("Remote Players");
        }

    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.K) && !isDead && !spawnProtected)
            {
                SelfDestruct();
            }

            if (Input.GetKeyDown(KeyCode.B) && !taunt.isPlaying)
            {
                CmdPlayTaunt();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(Reload());
            }

            EnergyRegenAndDisplay();

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyUp(KeyCode.Equals))      //energy hack
            {
                energy += 100000f;
            }

            /*            if (isDead)
                        {
                            DeathCamZoom(killer);
                        }*/
        }
    }

    IEnumerator Reload()
    {
        if (!reload.isPlaying && energy < maxEnergy)
        {
            canReload = false;
            reloading = true;
            playerShoot.enabled = false;
            reload.Play();
            yield return new WaitForSeconds(3f);
            energy = maxEnergy;
            playerShoot.enabled = true;
            beep.Play();
            reloading = false;
            yield return new WaitForSeconds(5f);
            canReload = true;
        }
    }

    [Command]
    private void CmdPlayTaunt()
    {
        RpcPlayTaunt();
    }

    [ClientRpc]
    private void RpcPlayTaunt()
    {
        taunt.Play();
    }

    private void SelfDestruct()
    {
        Die(null, Vector3.zero, Vector3.zero, 0);
        deaths++;
        if (playerUI != null)
        {
            playerUI.SetScore("Score: " + kills + " / " + deaths);

        }
    }

    private void DeathCamZoom(Transform killer)
    {
        playerHeadCam.transform.LookAt(killer);
    }

    public float getHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public void PlayerSetup() {
        if (isLocalPlayer)
        {
            //switch cameras
            /*            GameManager.singleton.ActivateDeathCam(null, false);            //doesn't work...*/

            playerUI.HideComponents(false);
/*            playerNetworkSetup.playerUIInstance.SetActive(true);*/
        }

        CmdBroadcastNewPlayerSetup();
    }

    [Command]
    void CmdBroadcastNewPlayerSetup()
    {
        RpcPlayerSetupOnAllClients();
    }

    [ClientRpc]
    void RpcPlayerSetupOnAllClients()
    {
        if (isFirstSetup)
        {
            
            playerMove = GetComponent<PlayerMove>();
            playerShoot = GetComponent<PlayerShoot>();
            activeBehaviours.Add(playerMove, playerMove.enabled);       //records which behaviours are initially active
            activeBehaviours.Add(playerShoot, playerShoot.enabled);

            startupSFX.Play();
            isFirstSetup = false;
        }
        else
        {
            respawnSFX.Play();
            respawnSFXVoice.Play();
            ClearBulletHoles();
        }

        
        SetDefaults();


    }

    public void SetDefaults() {
        isDead = false;
        currentHealth = maxHealth;
        energy = maxEnergy;
        /*        UIController.SetHealthbarFill(1f);*/

        //arms back in place
        /*        leftArmPivot.Rotate(-90f, 0f, 0f);
                rightArmPivot.Rotate(-90f, 0f, 0f);*/ 


        if (playerUI != null)
        {
            playerUI.SetHealthbarFill(currentHealth / maxHealth);
        }
        if (rigBody != null)
        {
            rigBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        StartCoroutine(EnableWithSpawnProtection());
        
    }

    [ClientRpc]
    public void RpcApproveTakeDamage(GameObject sourcePlayer, int amount, Vector3 normal, Vector3 point, bool headshot) {
        if (isDead || spawnProtected) {   //if they shooting a corpse or new revived
            return;
        }

        if (headshot)
        {
            currentHealth -= amount * 10;
        }
        else
        {
            currentHealth -= amount;
        }
          

        if (playerUI != null)
        {
            playerUI.SetHealthbarFill(getHealthPercentage());
        }
       
        if (headshot)
        {
            hitSFX.Play();  //play head shot sound
        }       

        if (currentHealth <= 0) {
            Die(sourcePlayer, normal, point, amount);
        }
    }

    private void Die(GameObject sourcePlayer, Vector3 normal, Vector3 point, int amount) {
        //play death sound
        shutdownSFX.Play();
        shutdownSFXVoice.Play();
        currentHealth = 0;
        if (playerUI != null)
        {
            playerUI.SetHealthbarFill(0);
        }

        isDead = true;
        rigBody.useGravity = true;

        //SNAP to death pose
        leftArmPivot.localEulerAngles = Vector3.right * 90;             //(90,0,0) down
        rightArmPivot.localEulerAngles = Vector3.right * 90;            //(0,90,0) sideways out (0,0,90) twirls


        /*        StartCoroutine(ArmsDown(1));*/

        //turn on spawn protect
        spawnProtected = true;
        visorRenderer.material = visorOffMaterial;
        muzzleRendererL.material = visorOffMaterial;
        muzzleRendererR.material = visorOffMaterial;
/*        playerHeadCam.transform.localRotation = Quaternion.identity;*/

        //DISABLE COMPONENTS
        foreach (Behaviour behaviour in activeBehaviours.Keys) {    //disables all behaviours (when dead)
            behaviour.enabled = false;
        }
        if (rigBody != null)
        {
            rigBody.constraints = RigidbodyConstraints.None;                            //go limp
            /*            rigBody.AddTorque(-normal.z * 5000, -normal.y * 500, normal.x * 5000);     //fall according to how u were shot*/

            amount *= 500;
            rigBody.AddForceAtPosition(new Vector3(-normal.x * amount, -normal.y * amount, -normal.z * amount), point);
        }


        /*        Collider collider = GetComponent<Collider>();
                if (collider != null) {
                    collider.enabled = false;
                }*/
        //switch cameras
        if (isLocalPlayer)
        {
            /*            GameManager.singleton.ActivateDeathCam(sourcePlayer, true);     //doesn't work either...*/

            /*            if (sourcePlayer != null)
                        {
                            killer = sourcePlayer.transform;        //replacement hopefully
                            Debug.Log("killer found");
                        }
            */
            playerUI.HideComponents(true);
        }

        
        if (sourcePlayer != null && sourcePlayer.activeSelf)
        {
            deaths++;
            
            killersManager = sourcePlayer.GetComponent<PlayerManager>();
            if (this.tag != "Bot")
            {
                killersManager.kills++;
                killersManager.scoreOnPlayer.Play();

            }
            else
            {
                killersManager.scoreOnBot.Play();
            } 
            if (playerUI != null)
            {
                playerUI.SetScore("Score: " + kills + " / " + deaths);
            }
            if (killersManager.playerUI != null)    //not sure why i need this... when will be null if I'm not bot?
            {
                
                killersManager.playerUI.SetScore("Score: " + killersManager.kills + " / " + killersManager.deaths);
            }
            
        }

        
        //RESPAWN/**/
        StartCoroutine(Respawn());
    }
    
    IEnumerator Respawn() {
        yield return new WaitForSeconds(4f);

        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;

        transform.localRotation = Quaternion.identity;
        transform.rotation = spawnPoint.rotation;

        rigBody.velocity = Vector3.zero;
        rigBody.angularVelocity = Vector3.zero;

        //yield return new WaitForSeconds(0.1f);

        //SetDefaults();
        PlayerSetup();
    }

    //BROKEN :((
    /*    IEnumerator ArmsDown(int duration)      //when dead
        {
            while (leftArmPivot.localRotation != Quaternion.Euler(0,0, 90))
            {
                leftArmPivot.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(0,0,90), Time.deltaTime * duration);
                rightArmPivot.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(0, 0, 0), Time.deltaTime * duration);
                yield return null;

            }
        }*/

    IEnumerator ArmsUp(int duration)        //when respawn
    {
        playerHeadCam.transform.localEulerAngles = Vector3.right * 90;
        visorPivot.localEulerAngles = Vector3.right * 90;
        while (leftArmPivot.localRotation != Quaternion.identity)
        {
            leftArmPivot.localRotation = Quaternion.Slerp(leftArmPivot.localRotation, Quaternion.identity, Time.deltaTime * duration);
            rightArmPivot.localRotation = Quaternion.Slerp(rightArmPivot.localRotation, Quaternion.identity, Time.deltaTime * duration);
            playerHeadCam.transform.localRotation = Quaternion.Slerp(playerHeadCam.transform.localRotation, Quaternion.identity, Time.deltaTime * duration);
            visorPivot.localRotation = Quaternion.Slerp(visorPivot.localRotation, Quaternion.identity, Time.deltaTime * duration);

            yield return null;

        }
    }

    IEnumerator EnableWithSpawnProtection()
    {
        yield return StartCoroutine(ArmsUp(5));       //for arms animation to play out

        playerMove.ResetXRotation();
        foreach (Behaviour behaviour in activeBehaviours.Keys)
        {    //restores behaviours to their initial state
            behaviour.enabled = activeBehaviours[behaviour];
        }

        muzzleRendererL.material = visorOnMaterial;
        muzzleRendererR.material = visorOnMaterial;

        yield return new WaitForSeconds(1);     //immunity duration

        spawnProtected = false;
        visorRenderer.material = visorOnMaterial;
    }

    public Transform GetHeadVisor()
    {
        return headVisor.transform;
    }

    public Transform GetBody()
    {
        return body.transform;
    }

    public void ClearBulletHoles()
    {
        foreach (Transform child in this.transform)
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("Bullet Hole"))
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Transform child in headVisor.transform)
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("Bullet Hole"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void EnergyRegenAndDisplay()
    {
        if (energy < maxEnergy && !reloading)
        {
            energy += energyRegenRate * Time.deltaTime;
        }
        playerUI.SetEnergyBarFill(Mathf.Clamp(energy, 0f, maxEnergy), maxEnergy);
    }

    private void OnDisable()
    {
        if (isLocalPlayer)
        {
            Destroy(uiInstance);
        }
    }
}
