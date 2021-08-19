using System.Net.Mail;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

[RequireComponent(typeof(Transform))]
[System.Obsolete]
public class PlayerShoot : NetworkBehaviour {
    [SerializeField] private PlayerManager player;
    [SerializeField] private float shootEnergyCost = 0.5f;

    [SerializeField] private Rigidbody parentRigBod;
    [SerializeField] private Weapon weapon;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private Transform leftBarrel;
    [SerializeField] private Transform rightBarrel;
    [SerializeField] LayerMask mask;
    private RaycastHit targetHit;
    private string remoteLayerName = "Remote Players";

    GameObject bulletHoleInstance;
    GameObject sprayInstance;

    private float nextFireR;
    private float nextFireL;
    [SerializeField]
    private float fireRate = 3f;

    private float fireDelay;
/*    private bool alternate;*/

    private void Start() {
        if (isLocalPlayer)
        {
            player = GetComponent<PlayerManager>();
            weapon = GetComponent<Weapon>();
            parentRigBod = rayOrigin.root.gameObject.GetComponent<Rigidbody>();
        }
        
    }

    private void Update() {
        if (isLocalPlayer)
        {
            fireDelay = 1 / fireRate;
            RapidFire(fireDelay);

            if (Input.GetKeyDown(KeyCode.T) && !weapon.spraySFX.isPlaying)
            {
                Spray();
            }
        }  
    }

    private void Spray()
    {
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out targetHit, 3, mask))
        {
            if (targetHit.collider.gameObject.layer != LayerMask.NameToLayer(remoteLayerName))
            {
                CmdOnSpray(targetHit.point, targetHit.normal);  //draws spray graphic (stationary)
            }      
        }
    }

    [Command]
    private void CmdOnSpray(Vector3 pos, Vector3 normal)
    {
        RpcOnSpray(pos, normal);
    }

    [ClientRpc]
    private void RpcOnSpray(Vector3 pos, Vector3 normal)
    {
        Vector3 sprayMark = new Vector3(pos.x + normal.x / 1000, pos.y + normal.y / 1000, pos.z + normal.z / 1000);               //finds spray mark designated spot
        if (sprayInstance != null)
        {
            Destroy(sprayInstance);
            sprayInstance = (GameObject)Instantiate(weapon.sprayPrefab, sprayMark, Quaternion.LookRotation(normal));   //instantiates spray graphic
        }
        else
        {
            sprayInstance = (GameObject)Instantiate(weapon.sprayPrefab, sprayMark, Quaternion.LookRotation(normal));   //instantiates spray graphic
        }
        weapon.spraySFX.Play();

        
    }

    private void RapidFire(float shootDelay)
    {
        if (nextFireR > 0)                     //right blaster
        {
            nextFireR -= Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (nextFireR <= 0 && player.energy >= shootEnergyCost)
            {
                Shoot(0);
                /*alternante = !alternante;*/
                nextFireR += shootDelay;
            }
        }

        if (nextFireL > 0)                     //left blaster
        {
            nextFireL -= Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (nextFireL <= 0 && player.energy >= shootEnergyCost)
            {
                Shoot(1);
                nextFireL += shootDelay;
            }
        }
    }

    [Command]
    void CmdOnShoot(int firingSide)
    {
        RpcDoMuzzleFlash(firingSide);
        RpcDoShootSFX(firingSide);
        RpcRecoil(firingSide);
    }

    [ClientRpc]
    void RpcRecoil(int firingSide)
    {
        if (firingSide == 0)
        {
/*            parentRigBod.AddForceAtPosition(-rayOrigin.forward * weapon.getDamage(), leftBarrel.position);*/
            parentRigBod.AddTorque(0f, -rayOrigin.up.y * weapon.getDamage() * Random.Range(1.0f, 2.0f), 0f, ForceMode.Impulse);
        } 
        else if (firingSide == 1)
        {
            /*            parentRigBod.AddForceAtPosition(-rayOrigin.forward * weapon.getDamage(), rightBarrel.position);*/
            parentRigBod.AddTorque(0f, rayOrigin.up.y * weapon.getDamage() * Random.Range(1.0f, 2.0f), 0f, ForceMode.Impulse);
        }
        
    }

    [ClientRpc]
    void RpcDoShootSFX(int firingSide)
    {
        if (firingSide == 0)  //left side
        {
            weapon.shootSFXL.Play();
        }
        else if (firingSide == 1)   //right side
        {
            weapon.shootSFXR.Play();
        }

    }

    [ClientRpc]
    void RpcDoMuzzleFlash(int firingSide)
    {
        if (firingSide == 0)  //left side
        {
            weapon.muzzleFlashL.Play();
        }
        else if (firingSide == 1)   //right side
        {
            weapon.muzzleFlashR.Play();
        }
    }

    [Command]
    void CmdOnHit(Vector3 pos, Vector3 normal, NetworkIdentity networkID, bool headshot)
    {
        RpcOnHit(pos, normal, networkID, headshot);
    }

    [ClientRpc] 
    void RpcOnHit(Vector3 pos, Vector3 normal, NetworkIdentity networkID, bool headshot)
    {
        Vector3 bulletMark = new Vector3(pos.x + normal.x / 1000, pos.y + normal.y / 1000, pos.z + normal.z / 1000);               //finds bullet mark designated spot
        bulletHoleInstance = (GameObject)Instantiate(weapon.hitEffectPrefab, bulletMark, Quaternion.LookRotation(normal));   //instantiates bullet hole graphic
        bulletHoleInstance.GetComponent<AudioSource>().Play();

        if (networkID != null)
        {
            if (!headshot)  //if not headshot
            {
                bulletHoleInstance.transform.parent = networkID.transform;      //NORMAL SHOT -> move with body
            }
            else            //if headshot
            {
                if (networkID.GetComponent<PlayerManager>() != null)    //if not a bot
                {
                    bulletHoleInstance.transform.parent = networkID.GetComponent<PlayerManager>().GetHeadVisor();   //HEADSHOT PLAYER -> move with visor
                }
                else
                {
                    bulletHoleInstance.transform.parent = networkID.transform;      //HEADSHOT but BOT -> move with body
                }
            }        
        }
        
        Destroy(bulletHoleInstance, 2f);       //destroys graphic after 2 sec
    }

    private void Shoot(int firingSide) {

        CmdOnShoot(firingSide);   //muzzle flash for server & all clients

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out targetHit, weapon.getRange(), mask)) {       //if something is hit
            if (targetHit.collider.gameObject.layer == LayerMask.NameToLayer(remoteLayerName)) {            //and if hit is a player
                if(targetHit.collider.tag == "Head")    //if headshot
                {                       //self
                    CmdRequestTakeDmg(this.transform.root.gameObject, targetHit.transform.root.gameObject, weapon.getDamage(), targetHit.normal, targetHit.point, true);
                    CmdOnHit(targetHit.point, targetHit.normal, targetHit.transform.root.GetComponent<NetworkIdentity>(), true);
                }
                else
                {                       //self
                    CmdRequestTakeDmg(this.transform.root.gameObject, targetHit.transform.root.gameObject, weapon.getDamage(), targetHit.normal, targetHit.point, false);               //damages player
                    CmdOnHit(targetHit.point, targetHit.normal, targetHit.transform.root.GetComponent<NetworkIdentity>(), false);    //draws hit graphic (mobile)
                }        
            }
            else
            {
                CmdOnHit(targetHit.point, targetHit.normal, null, false);  //draws hit graphic (stationary)
            }             

        }

        player.energy -= shootEnergyCost;
    }

    [Command]
    private void CmdRequestTakeDmg(GameObject sourcePlayer, GameObject targetPlayer, int dmg, Vector3 normal, Vector3 point, bool headshot) {
        targetPlayer.GetComponent<PlayerManager>().RpcApproveTakeDamage(sourcePlayer, dmg, normal, point, headshot);
    }

}
