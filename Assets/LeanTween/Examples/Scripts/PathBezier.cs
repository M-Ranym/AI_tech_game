using UnityEngine;
using System.Collections;
using DentedPixel;

namespace DentedPixel.LTExamples{

public class PathBezier : MonoBehaviour {

	public Transform[] trans;
	
	LTBezierPath cr;
	private GameObject avatar1;

	void OnEnable(){
		
		cr = new LTBezierPath( new Vector3[] {trans[0].position, trans[2].position, trans[1].position, trans[3].position, trans[3].position, trans[5].position, trans[4].position, trans[6].position} );
	}

	void Start () {
		avatar1 = GameObject.Find("Avatar1");

		
		LTDescr descr = LeanTween.move(avatar1, cr.pts, 6.5f).setOrientToPath(true).setRepeat(-1);
		Debug.Log("length of path 1:"+cr.length);
		Debug.Log("length of path 2:"+descr.optional.path.length);
	}
	
	private float iter;
	void Update () {
		
		

		iter += Time.deltaTime*0.07f;
		if(iter>1.0f)
			iter = 0.0f;
	}

	void OnDrawGizmos(){
		
		if(cr!=null)
			OnEnable();
		Gizmos.color = Color.red;
		if(cr!=null)
			cr.gizmoDraw(); 
	}
}

}