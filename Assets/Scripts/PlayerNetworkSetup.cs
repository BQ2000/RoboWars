/*using UnityEngine;
using UnityEngine.Networking;

[System.Obsolete]
[RequireComponent(typeof(PlayerManager))]
public class PlayerNetworkSetup : NetworkBehaviour
{
    [SerializeField] Behaviour[] componentsToDisable;
    [SerializeField] public GameObject sceneCamera;

    private string remoteLayerName = "Remote Players";
    private string netID;
    private PlayerManager player;

    [SerializeField]
    private GameObject playerUIPrefab;
    public GameObject playerUIInstance;


    private void Start()
    {
        CheckIn();

        if (!isLocalPlayer)
        {   //Remote Players
            AssignRemoteLayer();
            DisableComponents();
        }
        else
        {                //Local Player

            playerUIInstance = Instantiate(playerUIPrefab);    //Instantiate playerUI


            UIController ui = playerUIInstance.GetComponent<UIController>();
            if (ui == null)
            {
                Debug.LogError("No UIController on UI prefab");
            }

            ui.SetPlayerManager(player);

            if (sceneCamera != null) { sceneCamera.SetActive(false); }

            GetComponent<PlayerManager>().PlayerSetup();
            Debug.Log("attempted player setup");
        }


    }

    private void CheckIn()
    {            //checks them in to GameManager dictionary
        netID = GetComponent<NetworkIdentity>().netId.ToString();
        player = GetComponent<PlayerManager>();
        GameManager.RegisterPlayer(netID, player);      //tbh this method not necessary cuz NetworkIdentity() can already access all info
    }

    private void AssignRemoteLayer()
    {
        this.gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    private void DisableComponents()
    {
        foreach (Behaviour component in componentsToDisable)
        {
            component.enabled = false;
        }
    }



    private void OnDisable()
    {
        if (isLocalPlayer)
        {
            GameManager.singleton.ActivateSceneCamera(true);
        }

        GameManager.DeregisterPlayer(transform.name);
        Destroy(playerUIInstance);


        if (sceneCamera != null)
        {
            sceneCamera.SetActive(true);
        }
    }

}
*/