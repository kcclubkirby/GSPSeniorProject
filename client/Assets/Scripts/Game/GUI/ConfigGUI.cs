﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConfigGUI : MonoBehaviour {
    public ConfigItem[] configItems;
    public GUIStyle configStyle;
    public Rect configArea;

    public AudioSource click;

    private List<ConfigItem> weaponConfigs = new List<ConfigItem>();
    private List<ConfigItem> shipConfigs = new List<ConfigItem>();

    public GUIContent cancelContent;
    public GUIStyle cancelStyle;

	// Use this for initialization
	void Start () {
        foreach (ConfigItem ci in configItems) {
            if (ci.isWeaponConfig) {
                weaponConfigs.Add(ci);
            } else {
                shipConfigs.Add(ci);
            }
        }
	
	}

    void OnGUI() {
        GUILayout.BeginArea(new Rect((Screen.width * 0.5f) - (configArea.width* 0.5f), 0,
            configArea.width, configArea.height));

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        DrawWeaponConfigs();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        DrawShipConfigs();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical();
        if (GUILayout.Button(cancelContent, cancelStyle, GUILayout.MaxWidth(512), GUILayout.MaxHeight(32))) {
            Application.LoadLevel("lobby");
        }
        GUILayout.EndVertical();
        GUILayout.EndVertical();


        GUILayout.EndArea();
    }

    public void DrawWeaponConfigs() {
        foreach (ConfigItem ci in weaponConfigs) {
            if (GUILayout.Button(ci.display, configStyle, GUILayout.MaxWidth(128), GUILayout.MaxHeight(128))) {
                click.Play();
                GameManager.gameManager.CurrentWeaponChoice = ci.relatedGunChoice;
            }
        }
    }

    public void DrawShipConfigs() {
        foreach (ConfigItem ci in shipConfigs) {
            if (GUILayout.Button(ci.display, configStyle, GUILayout.MaxWidth(128), GUILayout.MaxHeight(128))) {
                click.Play();
                GameManager.gameManager.ShipModelPrefab = ci.shipModelPrefab;
                GameManager.gameManager.ShipShieldPrefab = ci.shipShieldEffectPrefab;
            }
        }
    }
}
