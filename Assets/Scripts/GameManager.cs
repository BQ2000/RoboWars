using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager singleton;
    public MatchSettings matchSettings;
    [SerializeField]
    private Camera sceneCamera;

    public static bool playerDead;

    private void Awake()
    {

        if (singleton == null)
        {
            GameManager.singleton = this;
        }
        else
        {
            Debug.LogError("More than one GameManager");
        }
    }

    private void Update()
    {

    }

    public void ActivateDeathCam(GameObject sourcePlayer, bool isActive)
    {
        if (sourcePlayer != null && sceneCamera != null)
        {
            sceneCamera.enabled = isActive;
            sceneCamera.transform.LookAt(sourcePlayer.transform);

            Debug.Log("happened");
        }
        Debug.Log("nothin happened");

    }

    IEnumerator DeathCamZoom(GameObject sourcePlayer)
    {
        Debug.Log("should zoom to " + sourcePlayer);

        while (sceneCamera.orthographicSize > 3)
        {
            yield return new WaitForSeconds(0.01f);
            transform.LookAt(sourcePlayer.transform);
            sceneCamera.orthographicSize -= 0.1f;
        }

    }

    /*    private static Dictionary<string, PlayerManager> players = new Dictionary<string, PlayerManager>();*/

    /*    public static void RegisterPlayer(string netID, PlayerManager player) {
            string playerID = "Player " + netID;
            players.Add(playerID, player);
            player.transform.name = playerID;
        }*/

    /*    public static void DeregisterPlayer(string playerID) {
            players.Remove(playerID);
        }*/

    /*    public static PlayerManager GetPlayer(string playerID) {
            return players[playerID];
        }*//*

        //Sample GUI
        *//*    private void OnGUI() {
                GUILayout.BeginArea(new Rect(200, 200, 200, 500));
                GUILayout.BeginVertical();

                foreach (string playerID in players.Keys) {
                    GUILayout.Label(playerID + " - " + players[playerID].transform.name);
                }

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }*//*

    }
    */
}