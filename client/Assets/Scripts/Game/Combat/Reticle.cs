﻿using UnityEngine;
using System.Collections;

public class Reticle : MonoBehaviour {

    public Transform farSight;
    public Transform nearSight;
    public Transform muzzlePoint;

    public float range;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(muzzlePoint.position, muzzlePoint.parent.forward, out hitInfo, range)) {
            farSight.position = hitInfo.point;
            nearSight.position = hitInfo.point - hitInfo.normal * 0.5f;
        } else {
            farSight.position = muzzlePoint.position + muzzlePoint.parent.forward * range;
            nearSight.position = muzzlePoint.position + muzzlePoint.parent.forward * (range * 0.5f);
        }
	
	}
}