using UnityEngine;
using System.Collections.Generic;

using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text ChatText;
    [SerializeField] TMP_InputField InputText;
    [SerializeField] TMP_Text PlayersText;

    [SerializeField] GameObject startButton;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        RefreshPlayers();
        if (!PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(false);
        }
        if (PlayerPrefs.HasKey("Winner") && PhotonNetwork.IsMasterClient)
        {
            string winner = PlayerPrefs.GetString("Winner");
            string message = "The last match was won by: " + winner;

            if (PlayerPrefs.HasKey("WinnerKills"))
            {
                int winnerKills = PlayerPrefs.GetInt("WinnerKills");
                message += " and killed " + winnerKills.ToString() + " enemies.";
            }

            photonView.RPC("ShowMessage", RpcTarget.All, message);
            PlayerPrefs.DeleteAll();
        }
    }
    public void StartGame()
    {
        PhotonNetwork.LoadLevel("Game");
        
    }
    
    void Log(string message)
    {
        ChatText.text += "\n";
        ChatText.text += message;
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
   
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Log(otherPlayer.NickName + " left the room");
        RefreshPlayers();
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Log(newPlayer.NickName + " entered the room");
        RefreshPlayers();
    }
    
    [PunRPC]
    public void ShowMessage(string message, PhotonMessageInfo info)
    {
        ChatText.text += "\n";
        ChatText.text += message;
    }
    public void Send()
    {
        // Si el campo no tiene ningún texto, no hacemos nada
        if (string.IsNullOrWhiteSpace(InputText.text)) { return; }
        // Si un jugador presiona el botón Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Llamamos al método ShowMessage para todos los jugadores del servidor
            // Necesitamos generar el apodo del jugador y todo el texto que escribió en su campo de entrada
            photonView.RPC("ShowMessage", RpcTarget.All, PhotonNetwork.NickName + ": " + InputText.text);
            // Borrar el string de texto en el campo de entrada
            InputText.text = string.Empty;
        }
    }
    void RefreshPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ShowPlayers", RpcTarget.All);
        }
    }

    [PunRPC]
    public void ShowPlayers()
    {
        PlayersText.text = "Players: ";
        foreach (Photon.Realtime.Player otherPlayer in PhotonNetwork.PlayerList)
        {
            PlayersText.text += "\n";
            PlayersText.text += otherPlayer.NickName;
        }
    }
}
