using UnityEngine;
using System.Collections;
using DentedPixel;

public class GeneralBasic : MonoBehaviour {

	public GameObject prefabAvatar;

	void Start () {
		
		GameObject avatarRotate = GameObject.Find("AvatarRotate");
		GameObject avatarScale = GameObject.Find("AvatarScale");
		GameObject avatarMove = GameObject.Find("AvatarMove");

		
		LeanTween.rotateAround( avatarRotate, Vector3.forward, 360f, 5f);

		
		LeanTween.scale( avatarScale, new Vector3(1.7f, 1.7f, 1.7f), 5f).setEase(LeanTweenType.easeOutBounce);
		LeanTween.moveX( avatarScale, avatarScale.transform.position.x + 5f, 5f).setEase(LeanTweenType.easeOutBounce); 

		
		LeanTween.move( avatarMove, avatarMove.transform.position + new Vector3(-9f, 0f, 1f), 2f).setEase(LeanTweenType.easeInQuad);

		
		LeanTween.move( avatarMove, avatarMove.transform.position + new Vector3(-6f, 0f, 1f), 2f).setDelay(3f);

		
		LeanTween.scale( avatarScale, new Vector3(0.2f, 0.2f, 0.2f), 1f).setDelay(7f).setEase(LeanTweenType.easeInOutCirc).setLoopPingPong( 3 );
	
		
		LeanTween.delayedCall(gameObject, 0.2f, advancedExamples);

	}

	
	

	void advancedExamples(){
		LeanTween.delayedCall(gameObject, 14f, ()=>{
			for(int i=0; i < 10; i++){
				
				GameObject rotator = new GameObject("rotator"+i);
				rotator.transform.position = new Vector3(10.2f,2.85f,0f);

				
				GameObject dude = (GameObject)GameObject.Instantiate(prefabAvatar, Vector3.zero, prefabAvatar.transform.rotation );
				dude.transform.parent = rotator.transform;
				dude.transform.localPosition = new Vector3(0f,1.5f,2.5f*i);

				
				dude.transform.localScale = new Vector3(0f,0f,0f);
				LeanTween.scale(dude, new Vector3(0.65f,0.65f,0.65f), 1f).setDelay(i*0.2f).setEase(LeanTweenType.easeOutBack);

				
				float period = LeanTween.tau/10*i;
				float red   = Mathf.Sin(period + LeanTween.tau*0f/3f) * 0.5f + 0.5f;
	  			float green = Mathf.Sin(period + LeanTween.tau*1f/3f) * 0.5f + 0.5f;
	  			float blue  = Mathf.Sin(period + LeanTween.tau*2f/3f) * 0.5f + 0.5f;
				Color rainbowColor = new Color(red, green, blue);
				LeanTween.color(dude, rainbowColor, 0.3f).setDelay(1.2f + i*0.4f);
				
				
				LeanTween.moveZ(dude, 0f, 0.3f).setDelay(1.2f + i*0.4f).setEase(LeanTweenType.easeSpring).setOnComplete(
					()=>{
						LeanTween.rotateAround(rotator, Vector3.forward, -1080f, 12f);
					}
				);

				
				LeanTween.moveLocalY(dude,4f,1.2f).setDelay(5f + i*0.2f).setLoopPingPong(1).setEase(LeanTweenType.easeInOutQuad);
			
				
				LeanTween.alpha(dude, 0f, 0.6f).setDelay(9.2f + i*0.4f).setDestroyOnComplete(true).setOnComplete(
					()=>{
						Destroy( rotator ); 
					}
				);	
			}

		}).setOnCompleteOnStart(true).setRepeat(-1); 
	}
}
