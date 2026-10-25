using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DentedPixel;


public class PathSplinePerformance : MonoBehaviour {

	public GameObject trackTrailRenderers;
	public GameObject car;
	public GameObject carInternal;


	public float circleLength = 10f;
	public float randomRange = 1f;
	public int trackNodes = 30;
	public float carSpeed = 30f;
	public float tracerSpeed = 2f;

	private LTSpline track;
	private int trackIter = 1;
	private float carAdd;
	private float trackPosition; 

	void Start () {
		Application.targetFrameRate = 240;

		
		List<Vector3> randList = new List<Vector3>();
		float degree = 0f;
		int nodeLength = trackNodes + 1;
		for(int i = 0; i < nodeLength; i++){
			float x = Mathf.Cos( degree * Mathf.Deg2Rad ) * circleLength + Random.Range(0f, randomRange);
			float z = Mathf.Sin( degree * Mathf.Deg2Rad ) * circleLength + Random.Range(0f, randomRange);
			randList.Add( new Vector3(x,1f,z ) );

			degree += 360f/(float)trackNodes;
		}
		randList[0] = randList[ randList.Count-1 ]; 
		randList.Add( randList[1] ); 
		randList.Add( randList[2] ); 

		track = new LTSpline( randList.ToArray() );

		carAdd = carSpeed / track.distance;

		tracerSpeed = track.distance / (carSpeed*1.2f);

		
		LeanTween.moveSpline( trackTrailRenderers, track, tracerSpeed ).setOrientToPath(true).setRepeat(-1);
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

		trackPosition += Time.deltaTime * carAdd;

		if(trackPosition>1f)
			trackPosition = 0f; 
	}

	
	void OnDrawGizmos(){
		if(track!=null)
			track.drawGizmo( Color.red );
	}

	
	void playSwish(){
		AnimationCurve volumeCurve = new AnimationCurve( new Keyframe(0f, 0.005464481f, 1.83897f, 0f), new Keyframe(0.1114856f, 2.281785f, 0f, 0f), new Keyframe(0.2482903f, 2.271654f, 0f, 0f), new Keyframe(0.3f, 0.01670286f, 0f, 0f));
		AnimationCurve frequencyCurve = new AnimationCurve( new Keyframe(0f, 0.00136725f, 0f, 0f), new Keyframe(0.1482391f, 0.005405405f, 0f, 0f), new Keyframe(0.2650336f, 0.002480127f, 0f, 0f));

		AudioClip audioClip = LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setVibrato( new Vector3[]{ new Vector3(0.2f,0.5f,0f)} ).setWaveNoise().setWaveNoiseScale(1000));

		LeanAudio.play( audioClip ); 
	}
}
