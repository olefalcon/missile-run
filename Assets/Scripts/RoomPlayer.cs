﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPlayer : NetworkBehaviour
{
    public LobbyManager lm;
    public NewNetworkRoomManager nm;
    //public SyncList<string> playerNames = new SyncList<string>();
    [SyncVar]
    public int playerNum;
    [SyncVar]
    public string playerName;
    //When a room player is created player has joined the room
    void Start() {
        playerName = GameObject.Find("NetworkManager").GetComponent<NewNetworkRoomManager>().playerName;
        lm = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        if (isLocalPlayer) {
            lm.playerJoin(playerName);
        }
    }
    
    //Clients command when they join room
    [Command]
    public void playerJoin(string name) {
        playerName = name;
        lm = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        nm = GameObject.Find("NetworkManager").GetComponent<NewNetworkRoomManager>();
        ++nm.players;
        switch(nm.players) {
            case 1:
                nm.p1n = name;
                break;
            case 2:
                nm.p2n = name;
                break;
            case 3:
                nm.p3n = name;
                break;
            case 4:
                nm.p4n = name;
                break;
        }
        List<string> _playerNames = new List<string>();
        _playerNames.Add(nm.p1n);
        _playerNames.Add(nm.p2n);
        _playerNames.Add(nm.p3n);
        _playerNames.Add(nm.p4n);
        playerNum = nm.players-1;
        playerNumTargetRpc(connectionToClient, nm.players-1);
        playerJoinRpc(_playerNames);
    }
    [ClientRpc]
    //Call from network manager when player enters room
    public void playerJoinRpc(List<string> _playerNames) {        
        lm = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        lm.playerJoinRpc(_playerNames);
    }
    [TargetRpc]
    //Tells client what playernum they are
    public void playerNumTargetRpc(NetworkConnection target, int p) {
        lm = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        nm = GameObject.Find("NetworkManager").GetComponent<NewNetworkRoomManager>();
        lm.playerNum = p;
        nm.playerNum = p;
    }
    
    //function for players readying
    public void readyUp() {
        lm = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        CmdPlayerReady(lm.playerNum);
        gameObject.GetComponent<NetworkRoomPlayer>().CmdChangeReadyState(true);
    }
    public void unReadyUp() {
        lm = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        CmdPlayerUnReady(lm.playerNum);
        gameObject.GetComponent<NetworkRoomPlayer>().CmdChangeReadyState(false);
    }
    //Command for players readying across the server
    [Command]
    public void CmdPlayerReady(int p) {
        playerReadyRpc(p);
    }
    [Command]
    public void CmdPlayerUnReady(int p) {
        playerUnReadyRpc(p);
    }
    
    
    [ClientRpc]
    //Call from network manager when player readys up
    public void playerReadyRpc(int p)
    {
        lm = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        lm.playerReadyRpc(p);
    }
    [ClientRpc]
    public void playerUnReadyRpc(int p)
    {
        lm = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        lm.playerUnReadyRpc(p);
    }

    [ClientRpc]
    public void allReadyRpc() {
        lm = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        lm.allPlayerReady();
    }


    //functions for when a player leaves
    [ClientRpc]
    public void playerLeave(int p, string name) {
        Debug.Log("Player Index Number " + p.ToString() + " left!");
        nm = GameObject.Find("NetworkManager").GetComponent<NewNetworkRoomManager>();
        --nm.players;
        switch(p) {
            case 1: //player 2 leaves
                nm.p2n = nm.p3n;
                nm.p3n = nm.p4n;
                nm.p4n = "";
                if (playerNum > 1) {
                    --playerNum;
                }
                break;
            case 2: //player 3 leaves
                nm.p3n = nm.p4n;
                nm.p4n = "";
                if (playerNum > 2) {
                    --playerNum;
                }
                break;
            case 3: //player 4 leaves
                nm.p4n = "";
                break;
        }
        List<string> _playerNames = new List<string>();
        _playerNames.Add(nm.p1n);
        _playerNames.Add(nm.p2n);
        _playerNames.Add(nm.p3n);
        _playerNames.Add(nm.p4n);
        lm.playerLeaveRpc(p, name, _playerNames);
    }
}
