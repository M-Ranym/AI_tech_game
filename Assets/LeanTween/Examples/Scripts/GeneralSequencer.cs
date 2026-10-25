using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralSequencer : MonoBehaviour {

	public GameObject avatar1;

    public GameObject star;

	public GameObject dustCloudPrefab;

	public float speedScale = 1f;

	public void Start(){

		
		var seq = LeanTween.sequence();


		seq.append( LeanTween.moveY( avatar1, avatar1.transform.localPosition.y + 6f, 1f).setEaseOutQuad() );

        
        seq.insert( LeanTween.alpha(star, 0f, 1f) );
        seq.insert( LeanTween.scale( star, Vector3.one * 3f, 1f) );

		
		seq.append( LeanTween.rotateAround( avatar1, Vector3.forward, 360f, 0.6f ).setEaseInBack() );

		
		seq.append( LeanTween.moveY( avatar1, avatar1.transform.localPosition.y, 1f).setEaseInQuad() );

		
		seq.append(() => {
			for(int i = 0; i < 50f; i++){
				GameObject cloud = Instantiate(dustCloudPrefab) as GameObject;
				cloud.transform.parent = avatar1.transform;
				cloud.transform.localPosition = new Vector3(Random.Range(-2f,2f),0f,0f);
				cloud.transform.eulerAngles = new Vector3(0f,0f,Random.Range(0,360f));

				var range = new Vector3(cloud.transform.localPosition.x, Random.Range(2f,4f), Random.Range(-10f,10f));

				
				LeanTween.moveLocal(cloud, range, 3f*speedScale).setEaseOutCirc();
				LeanTween.rotateAround(cloud, Vector3.forward, 360f*2, 3f*speedScale).setEaseOutCirc();
				LeanTween.alpha(cloud, 0f, 3f*speedScale).setEaseOutCirc().setDestroyOnComplete(true);
			}
		});

		
		seq.setScale(speedScale);

        

        
        
        
        
        
	}
}
