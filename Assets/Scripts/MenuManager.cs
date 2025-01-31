﻿using Mirror;
using EpicTransport;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using DG.Tweening;

public class MenuManager : MonoBehaviour
{
    //Menu settings
    public float pulseScale;
    //Menu timers
    public float preGameScreenTime;
    private float preGameScreenTimer;
    //Pre Game Screens
    public Image devAuthScreen;
    public Image developerScreen;
    public Image warningsScreen;
    //Audio mixer
    public AudioMixer amixer;
    //Menu buttons
    public Button hostGameButton;
    public Button joinGameButton;
    public Button settingsGameButton;
    public Button quitGameButton;
    public Slider volumeSlider;
    //Menu panels
    public Image splashScreen;
    public Image hostMenu;
    public Image joinMenu;
    public Image settingsMenu;
    public Image hostGameSlidersMenu;
    //Input fields
    public InputField hostIPField;
    public InputField hostNameField;
    public InputField joinIPField;
    public InputField joinNameField;
    public InputField devAuthField;
    //Game Settings input fields
    public InputField powerupSpawnTimeField;
    public InputField pillarsField;
    public InputField missileStartSpeedField;
    public InputField missileSpeedGainField;
    public InputField missileStartTurnRateField;
    public InputField missileTurnRateGainField;
    //Settings
    public Toggle logsToggle;
    //Network Manager
    public NewNetworkRoomManager nm;
    public EOSLobby eosLobby;

    void Start() {
        //Reset timescale in case of leaving game during end round slowdown
        Time.timeScale = 1f;
        DOTween.logBehaviour = LogBehaviour.ErrorsOnly;
        preGameScreenTimer = preGameScreenTime;
        nm = GameObject.Find("NetworkManager").GetComponent<NewNetworkRoomManager>();
        //if you have been at the menu once dont play intro things over and over again
        if (Globals.hasMenuOnce == true) {
            Debug.Log("We've seen the pre menus before!");
            developerScreen.gameObject.SetActive(false);
            warningsScreen.gameObject.SetActive(false);
        } else {
            Debug.Log("This is the first time seeing pre menus we shouldn't see them again");
            Globals.hasMenuOnce = true;
        }
        eosLobby = GameObject.Find("EOSManager").GetComponent<EOSLobby>();
        float vol = PlayerPrefs.GetFloat("masterVol", 0f);
        volumeSlider.value = vol;
        amixer.SetFloat("masterVol", vol);
    }

    void Update () {
        if (preGameScreenTimer <= 0) {
            if (developerScreen.gameObject.activeInHierarchy) {
                developerScreen.gameObject.SetActive(false);
            } else if (warningsScreen.gameObject.activeInHierarchy) {
                warningsScreen.gameObject.SetActive(false);
            } else {
                //animateMenuButtons();
            }
            preGameScreenTimer = preGameScreenTime;
        } else {
            preGameScreenTimer -= Time.deltaTime;
        }
    }
    public void HostGame()
    {
        nm.playerName = hostNameField.text;
        if (nm.playerName == "") {
            assignEmptyName();
        }
        nm.playerName = nm.playerName.Substring(0,Mathf.Min(12, nm.playerName.Length));
        NetworkManager.singleton.StartHost();
    }
    public void JoinGame()
    {
        nm.playerName = joinNameField.text;
        if (nm.playerName == "") {
            assignEmptyName();
        }
        nm.playerName = nm.playerName.Substring(0,Mathf.Min(12, nm.playerName.Length));
        NetworkManager.singleton.networkAddress = joinIPField.text;
        NetworkManager.singleton.StartClient();
    }
    public void Settings() {
        settingsMenu.gameObject.SetActive(true);
    }
    public void AcceptSettings() {
        PlayerPrefs.SetFloat("masterVol", volumeSlider.value);
        nm.gameObject.GetComponent<ConsoleToGUI>().doFile = logsToggle;
        settingsMenu.gameObject.SetActive(false);
    }
    public void QuitGame() {
        Application.Quit();
    }
    public void HostMenu() {
        hostIPField.text = EOSSDKComponent.LocalUserProductIdString;
        hostMenu.gameObject.SetActive(true);
    }
    public void CopyId() {
        string id = hostIPField.text;
        id.CopyToClipboard();
    }
    public void HostGameSliders() {
        powerupSpawnTimeField.text = Globals.powerupSpawnTime.ToString();
        pillarsField.text = Globals.pillars.ToString();
        missileStartSpeedField.text = Globals.missileStartSpeed.ToString();
        missileSpeedGainField.text = Globals.missileSpeedGain.ToString();
        missileStartTurnRateField.text = Globals.missileStartTurnRate.ToString();
        missileTurnRateGainField.text = Globals.missileTurnRateGain.ToString();
        hostGameSlidersMenu.gameObject.SetActive(true);
    }
    public void HostGameSlidersClose() {
        try {
            Globals.powerupSpawnTime = float.Parse(powerupSpawnTimeField.text);
            Globals.pillars = int.Parse(pillarsField.text);
            Globals.missileStartSpeed = float.Parse(missileStartSpeedField.text);
            Globals.missileSpeedGain = float.Parse(missileSpeedGainField.text);
            Globals.missileStartTurnRate = float.Parse(missileStartTurnRateField.text);
            Globals.missileTurnRateGain = float.Parse(missileTurnRateGainField.text);
            hostGameSlidersMenu.gameObject.SetActive(false);
        } catch {
            Debug.Log("Some settings are not formatted correctly");
        }
        
    }
    public void HostMenuClose() {
        hostMenu.gameObject.SetActive(false);
    }
    public void JoinMenu() {
        joinMenu.gameObject.SetActive(true);
    }
    public void JoinMenuClose() {
        joinMenu.gameObject.SetActive(false);
    }
    public void InitEOS() {
        EOSSDKComponent eos = GameObject.Find("EOSManager").GetComponent<EOSSDKComponent>();
        eos.devAuthToolCredentialName = devAuthField.text;
        EOSSDKComponent.Initialize();
        devAuthScreen.gameObject.SetActive(false);
    }
    public void SetVolume() {
        amixer.SetFloat("masterVol", volumeSlider.value);
    }
    public void assignEmptyName() {
        nm.playerName = "runner";
    }
    /*
    public void animateMenuButtons() {
        Sequence mainMenuSequence = DOTween.Sequence();
        mainMenuSequence.Append(hostGameButton.rectTransform.DOAnchorPosY(-40f,0.46875, false).SetEasy(Ease.OutQuint))
        .Append(joinGameButton.transform.DOMoveY(-60f,0.46875, false))
        .Append(settingsGameButton.transform.DOMoveY(-80f,0.46875, false))
        .Append(quitGameButton.transform.DOMoveY(-100f,0.46875, false));
        mainMenuSequence();
    }
    */
}