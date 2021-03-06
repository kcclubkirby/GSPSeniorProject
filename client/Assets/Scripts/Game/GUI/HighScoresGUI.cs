﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HighScoresGUI : MonoBehaviour, IEventListener {
    private long myscore;
    private List<long> topScores;
    private List<string> topNames;

    public GUIStyle highScoreStyleText;
    public GUIStyle highScoreStylePoints;
    public GUIStyle buttonStyle;
    public Rect backRect;
    public Rect myscoreRect;

    public GUIContent backContent;

    public float scoresXMin;
    public float scoresYMin;
    public float scoresWidth;
    public float scoresHeight;
    public float scoresPadding;
    public float groupWidth;
    public float groupHeight;


	// Use this for initialization
	void Start () {
        GameManager.gameManager.ClientController.Register(this);
        topScores = new List<long>();
        topNames = new List<string>();
        GameManager.gameManager.ClientController.Send(DataType.SCORES_GET, new object());
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Notify(string eventType, object data) {
        switch (eventType) {
            case "scores":
                DisplayScores(data);
                break;
                
            default:
                break;
        }
    }
    public AudioSource click;
    void OnGUI() {
        if (GUI.Button(backRect, backContent, buttonStyle)) {
            click.Play();
            Application.LoadLevel("lobby");
        }

        DrawMyScore();
        DrawTop();
    }

    private void DrawMyScore() {
        GUI.TextArea(myscoreRect, "My Current Score: " + myscore.ToString(), highScoreStyleText);
    }

    public float height;
    public float width;
    private void DrawTop() {
        
        GUI.BeginGroup(new Rect(Screen.width  - groupWidth, 0, groupWidth, groupHeight));
        float top = scoresYMin;
        float xshift = scoresXMin + scoresWidth;
        GUI.TextArea(new Rect(scoresXMin, 0, scoresWidth, scoresHeight), "Top Players:", highScoreStyleText);
        top += scoresHeight;
        foreach(long l in topScores){
            GUI.TextArea(new Rect(xshift, top + scoresPadding, scoresWidth, scoresHeight), l.ToString(), highScoreStylePoints);
            top += scoresHeight;
        }

        top = scoresYMin;
        top += scoresYMin;
        foreach (string s in topNames) {
            GUI.TextArea(new Rect(scoresXMin, top + scoresPadding, scoresWidth, scoresHeight), s, highScoreStyleText);
            top += scoresHeight;
        }
        GUI.EndGroup();
         
    }

    private void DisplayScores(object data) {
        if (data.GetType() != typeof(Dictionary<string, object>)) {
            return;
        }

        Dictionary<string, object> cdata = data as Dictionary<string, object>;
        Dictionary<string, long> scores = cdata["scores"] as Dictionary<string, long>;
        Dictionary<string, string> names = cdata["names"] as Dictionary<string, string>;
        myscore = (long)cdata["my.score"];

        int size = scores.Count;
        for (int i = 0; i < size; ++i) {
            topScores.Add(scores["score" + i.ToString()]);
            topNames.Add(names["player" + i.ToString()]);
        }
    }
}
