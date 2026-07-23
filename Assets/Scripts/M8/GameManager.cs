using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] List<Transform> spawns = new List<Transform>();
    [SerializeField] List<Transform> spawnsWalk = new List<Transform>();
    [SerializeField] List<Transform> spawnsTurret = new List<Transform>();
    [SerializeField] GameObject enemiesDefeatedUi;

    [SerializeField] public TMP_Text playersText;
    [SerializeField] public TMP_Text enemiesText;
    [SerializeField] public TMP_Text enemyKillsText;
    [SerializeField] GameObject ExitButton;
    GameObject[] players;
    GameObject[] enemies;
    List<string> activePlayers = new List<string>();
    List<string> activeEnemies = new List<string>();
    int checkPlayers = 0;
    int checkEnemies = 0;
    int randSpawn;

    private int previousPlayerCount;
    private bool enemiesDefeated;
    private bool menuOpen;
    private const string EnemyKillsKeyPrefix = "EnemyKills_";
    
    void Start()
    {
        randSpawn = Random.Range(0, spawns.Count);
        PhotonNetwork.Instantiate("Player", spawns[randSpawn].position, spawns[randSpawn].rotation);
        Invoke("SpawnEnemy", 5f);
        previousPlayerCount = PhotonNetwork.PlayerList.Length;
        enemiesDefeated = false;
        menuOpen = false;
        RefreshEnemyKillsText();

        if (enemiesDefeatedUi != null)
        {
            enemiesDefeatedUi.SetActive(false);
        }

        SetMenuVisible(false);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
        if (PhotonNetwork.PlayerList.Length < previousPlayerCount)
        {
            ChangePlayersList();
        }
        previousPlayerCount = PhotonNetwork.PlayerList.Length;
    }
    public void ChangePlayersList()
    {
        photonView.RPC("PlayerList", RpcTarget.All);   
    }

    public void ChangeEnemiesList()
    {
        photonView.RPC("EnemyList", RpcTarget.All);
    }

    public bool IsMenuOpen()
    {
        return menuOpen;
    }

    public void ToggleMenu()
    {
        SetMenuVisible(!menuOpen);
    }

    public void SetMenuVisible(bool visible)
    {
        menuOpen = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;

        if (ExitButton != null)
        {
            ExitButton.SetActive(visible);
        }
    }

    public void RegisterEnemyDefeat(int killerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        string key = GetEnemyKillsKey(killerActorNumber);
        int currentKills = 0;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object value) && value is int storedKills)
        {
            currentKills = storedKills;
        }

        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { key, currentKills + 1 }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        RefreshEnemyKillsText();
    }
    

    public void RefreshEnemyKillsText()
    {
        if (enemyKillsText == null || PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        stringBuilder.Append("Kills: " + GetEnemyKills(PhotonNetwork.LocalPlayer.ActorNumber));
        enemyKillsText.text = stringBuilder.ToString();
    }
    // Devuelve una posición de reaparición aleatoria de la lista de spawns
    public Vector3 GetRandomSpawnPosition()
    {
        if (spawns != null && spawns.Count > 0)
        {
            int randomIndex = Random.Range(0, spawns.Count);
            return spawns[randomIndex].position;
        }
        return Vector3.zero; // Por si acaso la lista está vacía
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach (DictionaryEntry entry in propertiesThatChanged)
        {
            if (entry.Key is string key && key.StartsWith(EnemyKillsKeyPrefix))
            {
                RefreshEnemyKillsText();
                break;
            }
        }
    }

    private int GetEnemyKills(int actorNumber)
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            return 0;
        }

        string key = GetEnemyKillsKey(actorNumber);
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object value) && value is int kills)
        {
            return kills;
        }

        return 0;
    }

    private string GetEnemyKillsKey(int actorNumber)
    {
        return EnemyKillsKeyPrefix + actorNumber;
    }

    [PunRPC]
    public void PlayerList()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        activePlayers.Clear();
        foreach(GameObject player in players)
        {
            if (player.GetComponent<PlayerController>().dead == false)
            {
                activePlayers.Add(player.GetComponent<PhotonView>().Owner.NickName);
            }
        }
        
        playersText.text = "Players: " + activePlayers.Count.ToString();
        RefreshEnemyKillsText();

        if (checkPlayers > 0)
        {
            if (activePlayers.Count == 1)
            {
                PlayerPrefs.SetString("Winner", activePlayers[0]);
                var enemies = GameObject.FindGameObjectsWithTag("enemy");
                foreach (GameObject enemy in enemies)
                {
                    Enemy enemyComponent = enemy.GetComponent<Enemy>();
                    if (enemyComponent == null)
                    {
                        enemyComponent = enemy.GetComponentInChildren<Enemy>(true);
                    }
                    if (enemyComponent != null)
                    {
                        enemyComponent.ChangeHealth(100);
                    }
                }
                Invoke("EndGame", 5f);
            }
            else if (activePlayers.Count == 0)
            {
                PlayerPrefs.SetString("Winner", "Nobody");
                var enemies = GameObject.FindGameObjectsWithTag("enemy");
                foreach (GameObject enemy in enemies)
                {
                    Enemy enemyComponent = enemy.GetComponent<Enemy>();
                    if (enemyComponent == null)
                    {
                        enemyComponent = enemy.GetComponentInChildren<Enemy>(true);
                    }
                    if (enemyComponent != null)
                    {
                        enemyComponent.ChangeHealth(100);
                    }
                }
                Invoke("EndGame", 5f);
            }
        }
        checkPlayers++;        
    }

    [PunRPC]
    public void EnemyList()
    {
        enemies = GameObject.FindGameObjectsWithTag("enemy");
        activeEnemies.Clear();

        foreach (GameObject enemy in enemies)
        {
            Enemy enemyComponent = enemy.GetComponent<Enemy>();

            if (enemyComponent == null)
            {
                enemyComponent = enemy.GetComponentInChildren<Enemy>(true);
            }

            if (enemyComponent != null && enemyComponent.dead == false)
            {
                activeEnemies.Add(enemy.name);
            }
        }

        if (enemiesText != null)
        {
            enemiesText.text = "Enemies:" + activeEnemies.Count.ToString();
        }

        if (!enemiesDefeated && activeEnemies.Count <= 0 && checkEnemies > 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Invoke("TriggerVictory", 0.5f);
            }

            enemiesDefeated = true;
        }

        checkEnemies++;
    }
    private void TriggerVictory()
    {
        photonView.RPC("ShowEnemiesDefeatedUi", RpcTarget.All);
    }
    public void EndGame()
    {        
        PhotonNetwork.LoadLevel("Lobby");        
    }

    [PunRPC]
    public void ShowEnemiesDefeatedUi()
    {
        SaveEnemyVictoryProgress();
        PlayerPrefs.Save();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (enemiesDefeatedUi != null)
        {
            enemiesDefeatedUi.SetActive(true);
        }

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
            PhotonView playerPhotonView = playerObject.GetComponent<PhotonView>();
            PlayerController playerController = playerObject.GetComponent<PlayerController>();

            if (playerPhotonView != null && playerPhotonView.IsMine && playerController != null)
            {
                playerController.HideHudOnVictory();
            }
        }
    }

    private void SaveEnemyVictoryProgress()
    {
        string winnerName = string.Empty;
        int winnerKills = 0;

        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            int kills = GetEnemyKills(player.ActorNumber);
            if (kills >= winnerKills)
            {
                winnerKills = kills;
                winnerName = player.NickName;
            }
        }

        if (string.IsNullOrWhiteSpace(winnerName) && PhotonNetwork.LocalPlayer != null)
        {
            winnerName = PhotonNetwork.LocalPlayer.NickName;
        }

        PlayerPrefs.SetString("Winner", winnerName);
        PlayerPrefs.SetInt("WinnerKills", winnerKills);
    }
    public void ExitGame()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
        ChangePlayersList();
    }
    public void SpawnEnemy()
    {
        if (photonView.IsMine)
        {
            for (int i = 0; i < spawnsWalk.Count; i++)
            {
                PhotonNetwork.Instantiate("WalkEnemy", spawnsWalk[i].position, spawnsWalk[i].rotation);
            }
            for (int i = 0; i < spawnsTurret.Count; i++)
            {
                PhotonNetwork.Instantiate("Turret", spawnsTurret[i].position, spawnsTurret[i].rotation);
            }

            ChangeEnemiesList();
        }
    }
    

}