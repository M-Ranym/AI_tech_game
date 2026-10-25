using UnityEngine;
using System.Collections;
using DentedPixel;



public class PathSplineTrack : MonoBehaviour {

	public GameObject car;
	public GameObject carInternal;
	public GameObject trackTrailRenderers;

	public Transform[] trackOnePoints;

	private LTSpline track;
	private int trackIter = 1;
	private float trackPosition; 

	void Start () {
		
		track = new LTSpline( new Vector3[] {trackOnePoints[0].position, trackOnePoints[1].position, trackOnePoints[2].position, trackOnePoints[3].position, trackOnePoints[4].position, trackOnePoints[5].position, trackOnePoints[6].position} );

		
		LeanTween.moveSpline( trackTrailRenderers, track, 2f ).setOrientToPath(true).setRepeat(-1);
	}
	
	void Update () {
		
		float turn = Input.GetAxis("Horizontal");
		if(Input.anyKeyDown){
			if(turn<0f && trackIter>0){
				trackIter--;
				playSwish();
			}else if(turn>0f && trackIter < 2){ 
				trackIter++;
				playSwish();
			}
			
			LeanTween.moveLocalX(carInternal, (trackIter-1)*6f, 0.3f).setEase(LeanTweenType.easeOutBack);

		}

		
		track.place( car.transform, trackPosition );

		trackPosition += Time.deltaTime * 0.03f;

		if (trackPosition < 0f) 
			trackPosition = 1f;
		else if(trackPosition>1f)
			trackPosition = 0f; 
	}

	
	void OnDrawGizmos(){
		LTSpline.drawGizmo( trackOnePoints, Color.red);
	}

	
	void playSwish(){
		AnimationCurve volumeCurve = new AnimationCurve( new Keyframe(0f, 0.005464481f, 1.83897f, 0f), new Keyframe(0.1114856f, 2.281785f, 0f, 0f), new Keyframe(0.2482903f, 2.271654f, 0f, 0f), new Keyframe(0.3f, 0.01670286f, 0f, 0f));
		AnimationCurve frequencyCurve = new AnimationCurve( new Keyframe(0f, 0.00136725f, 0f, 0f), new Keyframe(0.1482391f, 0.005405405f, 0f, 0f), new Keyframe(0.2650336f, 0.002480127f, 0f, 0f));

		AudioClip audioClip = LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setVibrato( new Vector3[]{ new Vector3(0.2f,0.5f,0f)} ).setWaveNoise().setWaveNoiseScale(1000));

		LeanAudio.play( audioClip ); 
	}
}
