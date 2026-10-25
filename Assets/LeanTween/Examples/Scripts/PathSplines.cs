using UnityEngine;
using System.Collections;
using DentedPixel;

public class PathSplines : MonoBehaviour {

	public Transform[] trans;
	
	LTSpline cr;
	private GameObject avatar1;

	void OnEnable(){
		
		cr = new LTSpline( new Vector3[] {trans[0].position, trans[1].position, trans[2].position, trans[3].position, trans[4].position} );
		
	}

	void Start () {
		avatar1 = GameObject.Find("Avatar1");

		
		LeanTween.move(avatar1, cr, 6.5f).setOrientToPath(true).setRepeat(1).setOnComplete( ()=>{
			Vector3[] next = new Vector3[] {trans[4].position, trans[3].position, trans[2].position, trans[1].position, trans[0].position};
			LeanTween.moveSpline( avatar1, next, 6.5f); 
		}).setEase(LeanTweenType.easeOutQuad);
	}
	
	private float iter;
	void Update () {
		
		

		iter += Time.deltaTime*0.07f;
		if(iter>1.0f)
			iter = 0.0f;
	}

	void OnDrawGizmos(){
		
		if(cr==null)
			OnEnable();
		Gizmos.color = Color.red;
		if(cr!=null)
			cr.gizmoDraw(); 
	}
}
