﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-room-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomManager.html

	See Also: NetworkManager
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

/// <summary>
/// This is a specialized NetworkManager that includes a networked room.
/// The room has slots that track the joined players, and a maximum player count that is enforced.
/// It requires that the NetworkRoomPlayer component be on the room player objects.
/// NetworkRoomManager is derived from NetworkManager, and so it implements many of the virtual functions provided by the NetworkManager class.
/// </summary>
public class NewNetworkRoomManager : NetworkRoomManager
{
    //State Trackers
    public bool isLobby;
    public bool isGame;
    //MainMenuTrackers
    public bool hasMenuOnce;
    //Player tracking info
    public int playerNum;
    public string playerName;
    //Lobby manager object
    public LobbyManager lm;
    public RoomPlayer nrp;
    //vars for player names
    public string p1n = "";
    public string p2n = "";
    public string p3n = "";
    public string p4n = "";
    public int players = 0;
    //coroutine
    private IEnumerator coroutine;

    public Image errorPanel;
    public Text errorText;


    #region Server Callbacks

    /// <summary>
    /// This is called on the server when the server is started - including when a host is started.
    /// </summary>
    public override void OnRoomStartServer() {
        isLobby = true;
        isGame = false;
    }

    /// <summary>
    /// This is called on the server when the server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnRoomStopServer() { }

    /// <summary>
    /// This is called on the host when a host is started.
    /// </summary>
    public override void OnRoomStartHost() { }

    /// <summary>
    /// This is called on the host when the host is stopped.
    /// </summary>
    public override void OnRoomStopHost() { }

    /// <summary>
    /// This is called on the server when a new client connects to the server.
    /// </summary>
    /// <param name="conn">The new connection.</param>
    public override void OnRoomServerConnect(NetworkConnection conn) {
    }

    /// <summary>
    /// This is called on the server when a client disconnects.
    /// </summary>
    /// <param name="conn">The connection that disconnected.</param>
    public override void OnRoomServerDisconnect(NetworkConnection conn) {
        
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        if (isLobby) {
            RoomPlayer rp = conn.identity.GetComponent<RoomPlayer>();
            int playerNum = rp.playerNum;
            string playerName = rp.playerName;
            nrp = NetworkClient.connection.identity.GetComponent<RoomPlayer>();
            Debug.Log("TO SERVER: Player " + playerNum + " has left the lobby");
            nrp.playerLeave(playerNum, playerName);
            Destroy(conn.identity.gameObject);
        } else if (isGame) {
            Player p = conn.identity.GetComponent<Player>();
            int playerNum = p.pIndex;
            string playerName = p.playerName;
            GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
            Debug.Log("TO SERVER: Player " + playerNum + " has left the game");
            gm.playerLeave(playerNum, playerName);
            Destroy(conn.identity.gameObject);
        }
        
    }

    /// <summary>
    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public override void OnRoomServerSceneChanged(string sceneName) { }

    /// <summary>
    /// This allows customization of the creation of the room-player object on the server.
    /// <para>By default the roomPlayerPrefab is used to create the room-player, but this function allows that behaviour to be customized.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <returns>The new room-player object.</returns>
    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnection conn)
    {
        return base.OnRoomServerCreateRoomPlayer(conn);
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>By default the gamePlayerPrefab is used to create the game-player, but this function allows that behaviour to be customized. The object returned from the function will be used to replace the room-player on the connection.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <param name="roomPlayer">The room player object for this connection.</param>
    /// <returns>A new GamePlayer object.</returns>
    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>This is only called for subsequent GamePlay scenes after the first one.</para>
    /// <para>See OnRoomServerCreateGamePlayer to customize the player object for the initial GamePlay scene.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    public override void OnRoomServerAddPlayer(NetworkConnection conn)
    {
        base.OnRoomServerAddPlayer(conn);
    }

    /// <summary>
    /// This is called on the server when it is told that a client has finished switching from the room scene to a game player scene.
    /// <para>When switching from the room, the room-player is replaced with a game-player object. This callback function gives an opportunity to apply state from the room-player to the game-player object.</para>
    /// </summary>
    /// <param name="conn">The connection of the player</param>
    /// <param name="roomPlayer">The room player object.</param>
    /// <param name="gamePlayer">The game player object.</param>
    /// <returns>False to not allow this player to replace the room player.</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    /// <summary>
    /// This is called on the server when all the players in the room are ready.
    /// <para>The default implementation of this function uses ServerChangeScene() to switch to the game player scene. By implementing this callback you can customize what happens when all the players in the room are ready, such as adding a countdown or a confirmation for a group leader.</para>
    /// </summary>
    public override void OnRoomServerPlayersReady()
    {
        nrp = NetworkClient.connection.identity.GetComponent<RoomPlayer>();
        nrp.allReadyRpc();
        coroutine = readyDelayRoutine(3f);
        StartCoroutine(coroutine);
    }

    private IEnumerator readyDelayRoutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ServerChangeScene(GameplayScene);
        isLobby = false;
        isGame = true;
    }

    /// <summary>
    /// This is called on the server when CheckReadyToBegin finds that players are not ready
    /// <para>May be called multiple times while not ready players are joining</para>
    /// </summary>
    public override void OnRoomServerPlayersNotReady() { }


    #endregion

    #region Client Callbacks

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client enters the room.
    /// </summary>
    public override void OnRoomClientEnter() {
    }

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client exits the room.
    /// </summary>
    public override void OnRoomClientExit() { }

    /// <summary>
    /// This is called on the client when it connects to server.
    /// </summary>
    /// <param name="conn">The connection that connected.</param>
    public override void OnRoomClientConnect(NetworkConnection conn) {    }

    /// <summary>
    /// This is called on the client when disconnected from a server.
    /// </summary>
    /// <param name="conn">The connection that disconnected.</param>
    public override void OnClientDisconnect(NetworkConnection conn) {
        Debug.Log("Client has lost connection to Server! ...OnRoomClientDisconnect()");
        StopHost();
        StopClient();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    /// <summary>
    /// This is called on the client when a client is started.
    /// </summary>
    /// <param name="roomClient">The connection for the room.</param>
    public override void OnRoomStartClient() { }

    /// <summary>
    /// This is called on the client when the client stops.
    /// </summary>
    public override void OnRoomStopClient() { }

    /// <summary>
    /// This is called on the client when the client is finished loading a new networked scene.
    /// </summary>
    /// <param name="conn">The connection that finished loading a new networked scene.</param>
    public override void OnRoomClientSceneChanged(NetworkConnection conn) { }

    /// <summary>
    /// Called on the client when adding a player to the room fails.
    /// <para>This could be because the room is full, or the connection is not allowed to have more players.</para>
    /// </summary>
    public override void OnRoomClientAddPlayerFailed() { }
    /*
    public override void OnClientError(NetworkConnection conn, int errorCode) {
        errorPanel = GameObject.Find("ErrorScreen").GetComponent<Image>();
        errorText = GameObject.Find("ErrorText").GetComponent<Text>();
        errorPanel.gameObject.SetActive(true);
        errorText.text = "ERROR CODE " + errorCode.ToString();
        Debug.Log("ERROR CODE " + errorCode.ToString());
    }
    */

    #endregion

    #region Optional UI

    public override void OnGUI()
    {
        base.OnGUI();
    }

    #endregion
}
