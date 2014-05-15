﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RemotePlayerScript : MonoBehaviour, IEventListener {
    private string username = "";
    GUIText guiTextref;
    private int remoteId = -1;
    private float localHealth = 100.0f;
    IClientController server;

	// Use this for initialization
	void Start () {
        guiTextref = gameObject.GetComponentInChildren<GUIText>();
        guiTextref.text = username;
        guiTextref.color = new Color(0.0f, 1.0f, 0.0f);
        server = GameManager.gameManager.ClientController;
        server.Register(this);
	}

	// Update is called once per frame
	void Update () {
        if (Application.isEditor) {
            if (localHealth <= 0.0f) {
                Notify("player.remote.death", remoteId);
            }
        }
	}

    public float Health {
        get {
            return localHealth;
        }
    }

    void OnDestroy() {
        //TODO: instantiate explosion prefab
        Instantiate(Resources.Load("DeathPrefabRemote"));
    }

    public string Username {
        get {
            return username;
        }
        set {
            username = value;
            try {
                guiTextref.text = username;
            } catch (System.Exception e) {
                //eat it
            }
        }
    }

    public int Id {
        get {
            return remoteId;
        }
        set {
            remoteId = value;
            GetComponent<NetworkTransformer>().NetworkId = remoteId;
        }
    }

    public void Notify(string eventType, object o) {
        switch (eventType) {
            case "player.remote.hit":
                handlePlayerRemoteHit ( o );
                break;

            case "player.remote.death":
                handlePlayerRemoteDeath ( o );
                break;

            default:
                break;
        }
    }

    private void handlePlayerRemoteHit(object o){
        Dictionary<string, object> data = o as Dictionary<string, object>;
        if ( ( int ) data[ "player.hit.id" ] == remoteId ) {
            localHealth -= ( float ) data[ "damage" ];
            Vector3 contactPoint = new Vector3 (
                ( float ) data[ "contact.point.x" ],
                ( float ) data[ "contact.point.y" ],
                ( float ) data[ "contact.point.z" ] );
            Instantiate ( Resources.Load ( "HitPrefab" ), contactPoint, transform.rotation );

            Debug.Log ( "Damage taken <Damage : Health> <" + ( ( float ) data[ "damage" ] ).ToString () + " : "
                + localHealth.ToString () + "> to remote player." );
        }
    }

    private void handlePlayerRemoteDeath ( object o ) {
        if ( ( int ) o == remoteId ) {
            GameManager.gameManager.RemoveRemotePlayer ( remoteId );
        }
    }
}
