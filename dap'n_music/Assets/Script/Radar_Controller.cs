using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Radar_Controller : MonoBehaviour {

    public Transform Rader;
    public Image RaderImage;
    float Rotate = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Rotate += Input.GetAxis("Horizontal");
        //transform.Rotate(new Vector3(0, 0, Rotate));
        if(Rader) Rader.localEulerAngles = new Vector3(0, 0, Rotate);
    }
}
