#if !UNITY_FLASH
using UnityEngine;
using System.Collections;
using DentedPixel;

public class GeneralCameraShake : MonoBehaviour {

	private GameObject avatarBig;
	private float jumpIter = 9.5f;
	private AudioClip boomAudioClip;

	
	void Start () {
		avatarBig = GameObject.Find("AvatarBig");

		AnimationCurve volumeCurve = new AnimationCurve( new Keyframe(8.130963E-06f, 0.06526042f, 0f, -1f), new Keyframe(0.0007692695f, 2.449077f, 9.078861f, 9.078861f), new Keyframe(0.01541314f, 0.9343268f, -40f, -40f), new Keyframe(0.05169491f, 0.03835937f, -0.08621139f, -0.08621139f));
		AnimationCurve frequencyCurve = new AnimationCurve( new Keyframe(0f, 0.003005181f, 0f, 0f), new Keyframe(0.01507768f, 0.002227979f, 0f, 0f));
		boomAudioClip = LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setVibrato( new Vector3[]{ new Vector3(0.1f,0f,0f)} ));
		

		bigGuyJump();	
	}

	void bigGuyJump(){
		float height = Mathf.PerlinNoise(jumpIter, 0f)*10f;
		height = height*height * 0.3f;
		

		LeanTween.moveY(avatarBig, height, 1f).setEase(LeanTweenType.easeInOutQuad).setOnComplete( ()=>{
			LeanTween.moveY(avatarBig, 0f, 0.27f).setEase(LeanTweenType.easeInQuad).setOnComplete( ()=>{
				LeanTween.cancel(gameObject);

				
				
				float shakeAmt = height*0.2f; 
				float shakePeriodTime = 0.42f; 
				float dropOffTime = 1.6f; 
				LTDescr shakeTween = LeanTween.rotateAroundLocal( gameObject, Vector3.right, shakeAmt, shakePeriodTime)
				.setEase( LeanTweenType.easeShake ) 
				.setLoopClamp()
				.setRepeat(-1);

				
				LeanTween.value(gameObject, shakeAmt, 0f, dropOffTime).setOnUpdate( 
					(float val)=>{
						shakeTween.setTo(Vector3.right*val);
					}
				).setEase(LeanTweenType.easeOutQuad);


				

				
				GameObject[] boxes = GameObject.FindGameObjectsWithTag("Respawn"); 
		        foreach (GameObject box in boxes) {
		            box.GetComponent<Rigidbody>().AddForce(Vector3.up * 100 * height);
		        }

		        
		        GameObject[] lamps = GameObject.FindGameObjectsWithTag("GameController"); 
		        foreach (GameObject lamp in lamps) {
		        	float z = lamp.transform.eulerAngles.z;
		        	z = z > 0.0f && z < 180f ? 1 : -1; 
		            lamp.GetComponent<Rigidbody>().AddForce(new Vector3(z, 0f, 0f ) * 15 * height);
		        }

		        
		        LeanAudio.play(boomAudioClip, transform.position, height*0.2f); 
		        
		        
		        LeanTween.delayedCall(2f, bigGuyJump);
			});
		});
		jumpIter += 5.2f;
	}

}
#endif