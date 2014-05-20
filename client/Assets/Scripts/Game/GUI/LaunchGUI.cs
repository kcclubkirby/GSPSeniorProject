﻿using UnityEngine;
using System.Collections;

public class LaunchGUI : MonoBehaviour {
    public Rect singlePlayerRect;
    public Rect multiPlayerRect;
    public Rect creditsRect;
    public Rect quitRect;
    public Rect controlsRect;

    public GUIStyle launchStyle;

    public GUIContent singlePlayerContent;
    public GUIContent multiPlayerContent;
    public GUIContent creditContent;
    public GUIContent quitContent;
    public GUIContent controlsContent;

    public float groupWidth;
    public float groupHeight;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	
	}

    void OnGUI ( ) {
        GUI.BeginGroup(new Rect(Screen.width / 2.0f - groupWidth / 2.0f, Screen.height / 2.0f - groupHeight / 2.0f, groupWidth, groupHeight));

        if ( GUI.Button ( singlePlayerRect, singlePlayerContent, launchStyle ) ) {
            GameManager.gameManager.gameType = GameType.SINGLEPLAYER;
            Application.LoadLevel ( "singleplayer" );
        }

        if ( GUI.Button ( multiPlayerRect, multiPlayerContent, launchStyle ) ) {
            GameManager.gameManager.gameType = GameType.MULTIPLAYER;
            Application.LoadLevel ( "login" );
        }

        if (GUI.Button(creditsRect, creditContent, launchStyle)) {
            GameManager.gameManager.gameType = GameType.SINGLEPLAYER;
            Application.LoadLevel("credits");
        }

        if (GUI.Button(controlsRect, controlsContent, launchStyle)) {
            Application.LoadLevel("controls");
        }

        if (GUI.Button(quitRect, quitContent, launchStyle)) {
            Application.Quit();
        }

        GUI.EndGroup();
    }
}
