using UnityEngine;
using System.Collections;
using DentedPixel;

public class TestingRotate : MonoBehaviour {

	
	public GameObject sun ;
	public GameObject earth;

	
	public GameObject sun2;
	public GameObject earth2;

	
	public GameObject sun3;
	public GameObject earth3;

	void Start () {

		
		Vector3 sunLocalForEarth = earth.transform.InverseTransformPoint(sun.transform.position);
		Debug.Log("sunLocalForEarth:"+sunLocalForEarth);
		LeanTween.rotateAround(earth, earth.transform.up, 360f, 5.0f).setPoint(sunLocalForEarth).setRepeat(-1);

		
		Vector3 sunLocalForEarth2 = earth2.transform.InverseTransformPoint(sun2.transform.position);
		LeanTween.rotateAroundLocal(earth2, earth2.transform.up, 360f, 5.0f).setPoint(sunLocalForEarth2);

	}

	void Update() {

		
		earth3.transform.RotateAround(sun3.transform.position, sun3.transform.up, 72f * Time.deltaTime);

	}
}
