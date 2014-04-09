﻿using UnityEngine;
using System.Collections;
using Sfs2X.Core;
using Sfs2X.Logging;
using Sfs2X.Requests;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Entities;
using Sfs2X;
using System.Collections.Generic;

[System.Serializable]
public class SFSClient : IClientController {
    private SmartFox SFSInstance;
    private static SFSClient client;
    public LogLevel logLevel = LogLevel.DEBUG;
    private static object mutex = new object ();
    public bool debug;
    private string username = "";
    private string server = "";
    private int port = 0;
    private string zone = "";
    private string room = "";
    private string currentMessage;
    private List<IEventListener> listeners = new List<IEventListener> ();
    private bool useUDP = false;

    //names of 'levels/scenes' for each respective part.
    public string lobby;
    public string login;
    public string game;

    //singleton caller
    public static SFSClient Singleton {
        get {
            if ( client == null ) {
                lock ( mutex ) {
                    if ( client == null ) {
                        client = new SFSClient ();
                    }
                }
            }
            return client;
        }
    }

    private void RegisterCallbacks ( ) {
        SFSInstance.AddEventListener ( SFSEvent.CONNECTION, OnConnection );
        SFSInstance.AddEventListener ( SFSEvent.CONNECTION_LOST, OnConnectionLost );
        SFSInstance.AddEventListener ( SFSEvent.UDP_INIT, OnUDPInit );

        SFSInstance.AddEventListener ( SFSEvent.LOGIN, OnLogin );
        SFSInstance.AddEventListener ( SFSEvent.LOGOUT, OnLogout );
        SFSInstance.AddEventListener ( SFSEvent.LOGIN_ERROR, OnLoginError );

        SFSInstance.AddEventListener ( SFSEvent.EXTENSION_RESPONSE, OnExtensionReponse );
        SFSInstance.AddEventListener ( SFSEvent.PUBLIC_MESSAGE, OnPublicMessage );

        SFSInstance.AddEventListener ( SFSEvent.ROOM_JOIN, OnJoinRoom );
        SFSInstance.AddEventListener ( SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError );
        SFSInstance.AddEventListener ( SFSEvent.ROOM_ADD, OnRoomAdd );
        SFSInstance.AddEventListener ( SFSEvent.ROOM_REMOVE, OnRoomRemove );
        SFSInstance.AddEventListener ( SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom );
        SFSInstance.AddEventListener ( SFSEvent.USER_EXIT_ROOM, OnUserExitRoom );
    }

    private void UnregisterCallbacks ( ) {
        SFSInstance.RemoveEventListener ( SFSEvent.CONNECTION, OnConnection );
        SFSInstance.RemoveEventListener ( SFSEvent.CONNECTION_LOST, OnConnectionLost );
        SFSInstance.RemoveEventListener ( SFSEvent.UDP_INIT, OnUDPInit );

        SFSInstance.RemoveEventListener ( SFSEvent.LOGIN, OnLogin );
        SFSInstance.RemoveEventListener ( SFSEvent.LOGOUT, OnLogout );
        SFSInstance.RemoveEventListener ( SFSEvent.LOGIN_ERROR, OnLoginError );

        SFSInstance.RemoveEventListener ( SFSEvent.EXTENSION_RESPONSE, OnExtensionReponse );
        SFSInstance.RemoveEventListener ( SFSEvent.PUBLIC_MESSAGE, OnPublicMessage );

        SFSInstance.RemoveEventListener ( SFSEvent.ROOM_JOIN, OnJoinRoom );
        SFSInstance.RemoveEventListener ( SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError );
        SFSInstance.RemoveEventListener ( SFSEvent.ROOM_ADD, OnRoomAdd );
        SFSInstance.RemoveEventListener ( SFSEvent.ROOM_REMOVE, OnRoomRemove );
        SFSInstance.RemoveEventListener ( SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom );
        SFSInstance.RemoveEventListener ( SFSEvent.USER_EXIT_ROOM, OnUserExitRoom );
    }

    private SFSClient ( ) {
        SFSInstance = new SmartFox ( debug );

        if ( Application.isWebPlayer || Application.isEditor ) {
            Security.PrefetchSocketPolicy ( server, port, 500 );
        }

        RegisterCallbacks ();

        SFSInstance.AddLogListener ( logLevel, OnDebugMessage );
    }

    //SFS callbacks
    private void OnExtensionReponse ( BaseEvent evt ) {
        string cmd = ( string ) evt.Params[ "cmd" ];
        SFSObject sfsdata = ( SFSObject ) evt.Params[ "params" ];

        switch ( cmd ) {
            case "transform":
                TransformResponse ( sfsdata );
                break;

            case "player.hit":
                PlayerHitResponse ( sfsdata );
                break;

            case "death":
                DeathResponse ( sfsdata );
                break;

            case "spawn":
                SpawnResponse ( sfsdata );
                break;

            case "$SignUp.Submit":
                SignUpResponse(sfsdata);
                break;

            default:
                break;
        }
    }

    private void OnUDPInit ( BaseEvent evt ) {
        if ( ( bool ) evt.Params[ "success" ] ) {
            Debug.Log ( "UPD Initialized" );
            useUDP = true;
        } else {
            Debug.LogWarning ( "UPD Not Initialized, using TCP" );
        }
    }

    private void OnLogout ( BaseEvent evt ) {
        Debug.Log ( "User Logged Out" );
    }

    private void OnPublicMessage ( BaseEvent evt ) {
        string message = ( string ) evt.Params[ "message" ];
        OnEvent ( "charmessage", message );
    }

    private void OnLoginError( BaseEvent evt ) {
        string message = (string)evt.Params["errorMessage"];
        Debug.Log(message);
    }

    private void OnJoinRoom ( BaseEvent evt ) {
        Room room = ( Room ) evt.Params[ "room" ];
        Debug.Log ( "[Room Joined: " + room.Name + "]" );
        if ( room.IsGame ) {
            UnregisterCallbacks ();
            Application.LoadLevel ( "multiplayer" );
            RegisterCallbacks ();

            List<User> users = SFSInstance.LastJoinedRoom.UserList;
            foreach ( User u in users ) {
                if ( u.Id != SFSInstance.MySelf.Id ) {
                    GameManager.gameManager.AddRemotePlayer ( u.Id, u.Name );
                    Debug.Log ( "User " + u.Name + " is in room." );
                }
            }
        } else if ( room.Name == "lobby" ) {
            UnregisterCallbacks ();
            Application.LoadLevel ( "lobby" );
            RegisterCallbacks ();
        }
        this.room = room.Name;
    }

    private void OnRoomCreationError ( BaseEvent evt ) {
        Debug.LogError ( "[Room Creation Error" + ( string ) evt.Params[ "error" ] + "]" );
    }

    private void OnRoomAdd ( BaseEvent evt ) {
        Debug.Log ( "[Room Added: " + ( ( Room ) evt.Params[ "room" ] ).Name + "]" );
        OnEvent ( "roomadd", ( string ) ( ( Room ) evt.Params[ "room" ] ).Name );
    }

    private void OnRoomRemove ( BaseEvent evt ) {
        Debug.Log ( "[Room Removed: " + ( ( Room ) evt.Params[ "room" ] ).Name + "]" );
    }

    private void OnUserEnterRoom ( BaseEvent evt ) {
        User user = ( User ) evt.Params[ "user" ];
        Debug.Log ( "[User Enter Room: " + user.Name + "]" );
        Room room = ( Room ) evt.Params[ "room" ];

        if ( room.IsGame ) {
            int id = user.Id;
            if ( !GameManager.gameManager.Players.ContainsKey ( id ) && id != SFSInstance.MySelf.Id ) {
                GameManager.gameManager.AddRemotePlayer ( id, user.Name );
            }
        }
    }

    private void OnUserExitRoom ( BaseEvent evt ) {
        Debug.Log ( "[User Exit Room (" + ( ( Room ) evt.Params[ "room" ] ).Name + "): " + ( ( User ) evt.Params[ "user" ] ).Name + "]" );
        if ( ( ( Room ) evt.Params[ "room" ] ).IsGame ) {
            GameManager.gameManager.RemoveRemotePlayer ( ( ( User ) evt.Params[ "user" ] ).Id );
        }
    }

    private void OnLogin ( BaseEvent evt ) {
        if ( evt.Params.Contains ( "success" ) && !( bool ) evt.Params[ "success" ] ) {
            // Login failed
            currentMessage = ( string ) evt.Params[ "errorMessage" ];
            Debug.Log ( "Login error: " + currentMessage );
            SFSInstance.InitUDP();
        } else {
            // On to the lobby
            currentMessage = "Successful Login.";

            if (Application.loadedLevelName != "register") {
                Application.LoadLevel("lobby");
            } else {
                OnEvent("login.success", null);
            }
        }
    }

    private void OnConnection ( BaseEvent evt ) {
        bool success = ( bool ) evt.Params[ "success" ];
        if ( success ) {
            currentMessage = "Connection successful.";
        } else {
            currentMessage = "Cannot connect to the server.";
        }
        Debug.Log ( currentMessage );
    }

    private void OnConnectionLost ( BaseEvent evt ) {
        currentMessage = "Connection lost; reason: " + ( string ) evt.Params[ "reason" ];
    }

    private void OnDebugMessage ( BaseEvent evt ) {
        Debug.Log ( "[SFS DEBUG]: " + ( string ) evt.Params[ "message" ] );
    }

    //end SFS callbacks
    //start client interface methods
    public void Login ( string username, string password ) {
        this.username = username;
        if (!password.Equals("")) {
            password = Sfs2X.Util.PasswordUtil.MD5Password(password);
        }
        SFSInstance.Send ( new LoginRequest ( this.username, password, "StarboundAces" ) );
    }

    public void Logout() {
        SFSInstance.Send(new LogoutRequest());
    }

    public void Disconnect ( ) {
        UnregisterCallbacks ();
        if ( SFSInstance.IsConnected ) {
            SFSInstance.Disconnect ();
        }
    }

    public void Connect ( string server, int port ) {
        this.server = server;
        this.port = port;
        if ( !SFSInstance.IsConnected ) {
            SFSInstance.Connect ( server, port );
            while ( SFSInstance.IsConnecting ) {
                //block and show some kind of connection graphics
            }
        }
    }

    public void Send ( DataType type, object data ) {
        switch ( type ) {
            case DataType.TRANSFORM:
                SendTransform ( data );
                break;

            case DataType.CHARMESSAGE:
                SendCharMessage ( data );
                break;

            case DataType.MAKEGAME:
                SendRoomRequest ( data );
                break;

            case DataType.JOINGAME:
                SendRoomJoinRequest ( data );
                break;

            case DataType.SPAWNED:
                SendSpawnRequest ( data );
                break;

            case DataType.FIRE:
                SendFireRequest ( data );
                break;

            case DataType.DEATH:
                SendDeathRequest ( data );
                break;

            case DataType.REGISTER:
                SendRegistrationRequest(data);
                break;

            default:
                Debug.LogError ( "Should not reach this point in Send( SendType, object)" );
                break;
        }
    }

    public void Update ( ) {
        SFSInstance.ProcessEvents ();
    }

    public string Username {
        get {
            return username;
        }
    }

    public string Server {
        get {
            return server;
        }
    }

    public int Port {
        get {
            return port;
        }
    }

    public string Room {
        get {
            return room;
        }
    }

    public string Zone {
        get {
            return zone;
        }
    }
    //end client interface methods
    //start eventmessenger interface methods
    public void Register ( IEventListener listener ) {
        if ( !listeners.Contains ( listener ) ) {
            listeners.Add ( listener );
        }
    }

    public void Unregister ( IEventListener listener ) {
        if ( listeners.Contains ( listener ) ) {
            listeners.Remove ( listener );
        }
    }

    public void OnEvent ( string eventType, object o ) {
        lock ( mutex ) {
            foreach ( IEventListener el in listeners ) {
                el.Notify ( eventType, o );
            }
        }
    }
    //end eventmessenger interface methods

    private void CheckType ( object data, System.Type expected ) {
        if ( data.GetType () != expected ) {
            Debug.LogError ( "Wrong Type passed for message!\n[Expected]: " + expected.ToString () + "\n[Actual]: " + data.GetType ().ToString () );
        }
    }

    //methods for send()
    private void SendRegistrationRequest(object data) {
        Dictionary<string, string> cdata = data as Dictionary<string, string>;
        SFSObject sfsdata = new SFSObject();
        sfsdata.PutUtfString("user_name", cdata["username"]);
        sfsdata.PutUtfString("user_password", cdata["password"]);
        sfsdata.PutUtfString("user_email", cdata["email"]);

        SFSUserVariable userVar = new SFSUserVariable("username", cdata["username"]);
        SFSInstance.MySelf.SetVariable(userVar);

        SFSInstance.Send(new SetUserVariablesRequest(SFSInstance.MySelf.GetVariables()));
        SFSInstance.Send(new ExtensionRequest("$SignUp.Submit", sfsdata));
    }

    private void SendSpawnRequest ( object data ) {
        SFSInstance.Send ( new ExtensionRequest ( "server.spawn", new SFSObject () ) );
    }

    private void SendTransform ( object data ) {
        SFSObject sfso = new SFSObject ();
        Transform t = data as Transform;

        sfso.PutFloat ( "position.x", t.position.x );
        sfso.PutFloat ( "position.y", t.position.y );
        sfso.PutFloat ( "position.z", t.position.z );
        sfso.PutFloat ( "rotation.x", t.rotation.x );
        sfso.PutFloat ( "rotation.y", t.rotation.y );
        sfso.PutFloat ( "rotation.z", t.rotation.z );
        sfso.PutFloat ( "rotation.w", t.rotation.w );

        SFSInstance.Send ( new ExtensionRequest ( "server.transform", sfso ) );//, null, useUDP) );
    }

    private void SendCharMessage ( object data ) {
        if ( SFSInstance.LastJoinedRoom == null ) {
            Debug.LogError ( "Cannot send CharMessage request as not in room" );
        } else {
            string message = SFSInstance.MySelf.Name + " said: " + data as string;
            SFSInstance.Send ( new PublicMessageRequest ( message, new SFSObject (), SFSInstance.LastJoinedRoom ) );
        }
    }

    private void SendRoomRequest ( object data ) {
        RoomSettings settings = new RoomSettings ( SFSInstance.MySelf.Name + "'s game." );
        settings.Extension = new RoomExtension ( "StarboundAcesExtension", "com.gspteama.main.StarboundAcesExtension" );
        settings.MaxUsers = 8;
        settings.IsGame = true;
        SFSInstance.Send ( new CreateRoomRequest ( settings, false ) );
        SendRoomJoinRequest ( settings.Name );
    }

    private void SendRoomJoinRequest ( object data ) {
        if ( SFSInstance.LastJoinedRoom != null ) {
            //no private games/passwords supported as of yet.
            SFSInstance.Send ( new JoinRoomRequest ( data, "", SFSInstance.LastJoinedRoom.Id, false ) );
        } else {
            SFSInstance.Send ( new JoinRoomRequest ( data ) );
        }
    }

    private void SendFireRequest ( object data ) {
        SFSObject sfsdata = new SFSObject ();
        Dictionary<string, object> firedata = data as Dictionary<string, object>;

        sfsdata.PutFloat ( "damage", ( float ) firedata[ "damage" ] );
        sfsdata.PutInt ( "player.hit.id", ( int ) firedata[ "player.hit.id" ] );

        sfsdata.PutFloat ( "contact.point.x", ( float ) firedata[ "contact.point.x" ] );
        sfsdata.PutFloat ( "contact.point.y", ( float ) firedata[ "contact.point.y" ] );
        sfsdata.PutFloat ( "contact.point.z", ( float ) firedata[ "contact.point.z" ] );

        SFSInstance.Send ( new ExtensionRequest ( "server.fire", sfsdata, SFSInstance.LastJoinedRoom, useUDP ) );
    }

    private void SendDeathRequest ( object data ) {
        SFSObject sfsdata = new SFSObject ();
        SFSInstance.Send ( new ExtensionRequest ( "server.death", sfsdata, SFSInstance.LastJoinedRoom, useUDP ) );
    }
    //end methods for send()
    //methods for responses
    private void SignUpResponse(SFSObject sfsdata) {
        bool success = sfsdata.GetBool("success");
        if (success) {
            OnEvent("register", "Registration successful, please check your email for further instructions.");
        } else {
            OnEvent("register", sfsdata.GetUtfString("errorMessage"));
        }
    }

    private void SpawnResponse ( SFSObject sfsdata ) {
        int id = sfsdata.GetInt ( "player" );
        if ( id != SFSInstance.MySelf.Id ) {
            return;
        }

        float px = sfsdata.GetFloat ( "position.x" );
        float py = sfsdata.GetFloat ( "position.y" );
        float pz = sfsdata.GetFloat ( "position.z" );
        float rx = sfsdata.GetFloat ( "rotation.x" );
        float ry = sfsdata.GetFloat ( "rotation.y" );
        float rz = sfsdata.GetFloat ( "rotation.z" );
        float rw = sfsdata.GetFloat ( "rotation.w" );

        Dictionary<string, float> data = new Dictionary<string, float> ();
        data.Add ( "position.x", px );
        data.Add ( "position.y", py );
        data.Add ( "position.z", pz );
        data.Add ( "rotation.x", rx );
        data.Add ( "rotation.y", ry );
        data.Add ( "rotation.z", rz );
        data.Add ( "rotation.w", rw );

        int numWeapons = sfsdata.GetInt("numWeapons");
        float[] cooldowns = sfsdata.GetFloatArray("damages");
        float[] damages = sfsdata.GetFloatArray("cooldowns");

        data.Add("numWeapons", (float)sfsdata.GetInt("numWeapons"));
        for (int i = 0; i < numWeapons; ++i) {
            data.Add("cooldown" + i.ToString(), cooldowns[i]);
            data.Add("damage" + i.ToString(), damages[i]);
        }        

        OnEvent ( "spawn", data );
    }

    private void TransformResponse ( SFSObject sfsdata ) {
        int id = sfsdata.GetInt ( "player" );

        float px = sfsdata.GetFloat ( "position.x" );
        float py = sfsdata.GetFloat ( "position.y" );
        float pz = sfsdata.GetFloat ( "position.z" );
        float rx = sfsdata.GetFloat ( "rotation.x" );
        float ry = sfsdata.GetFloat ( "rotation.y" );
        float rz = sfsdata.GetFloat ( "rotation.z" );
        float rw = sfsdata.GetFloat ( "rotation.w" );

        //hack, needs to be fixed
        if ( !GameManager.gameManager.Players.ContainsKey ( id ) && id != SFSInstance.MySelf.Id) {
            GameManager.gameManager.AddRemotePlayer ( id, "AceGuest#"+id.ToString() );
        }

        Dictionary<string, object> data = new Dictionary<string, object> ();
        data.Add ( "id", id );
        data.Add ( "position.x", px );
        data.Add ( "position.y", py );
        data.Add ( "position.z", pz );
        data.Add ( "rotation.x", rx );
        data.Add ( "rotation.y", ry );
        data.Add ( "rotation.z", rz );
        data.Add ( "rotation.w", rw );
        OnEvent ( "transform", data );
    }

    private void PlayerHitResponse ( SFSObject sfsdata ) {
        float damage = sfsdata.GetFloat ( "damage" );
        int playerid = sfsdata.GetInt ( "player.hit.id" );
        Debug.Log ( "Player hit: " + playerid.ToString () );
        Dictionary<string, object> fdata = new Dictionary<string, object> ();
        fdata.Add ( "player.hit.id", playerid );
        fdata.Add ( "damage", damage );
        fdata.Add ( "contact.point.x", sfsdata.GetFloat ( "contact.point.x" ) );
        fdata.Add ( "contact.point.y", sfsdata.GetFloat ( "contact.point.y" ) );
        fdata.Add ( "contact.point.z", sfsdata.GetFloat ( "contact.point.z" ) );

        if ( playerid == SFSInstance.MySelf.Id ) {
            OnEvent ( "player.hit", fdata );
        } else {
            OnEvent ( "player.remote.hit", fdata );
        }
    }

    private void DeathResponse ( SFSObject sfsdata ) {
        OnEvent ( "player.remote.death", sfsdata.GetInt ( "id" ) );
    }
    //end methods for response
}