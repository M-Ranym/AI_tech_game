
using UnityEngine;
using System.Collections.Generic;

public class LeanAudioStream {

	public int position = 0;

	public AudioClip audioClip;
	public float[] audioArr;

	public LeanAudioStream( float[] audioArr ){
		this.audioArr = audioArr;
	}

	public void OnAudioRead(float[] data) {
        int count = 0;
        while (count < data.Length) {
            data[count] = audioArr[this.position];
            position++;
            count++;
        }
    }

    public void OnAudioSetPosition(int newPosition) {
    	this.position = newPosition;   
    }
}


public class LeanAudio : object {

	public static float MIN_FREQEUNCY_PERIOD = 0.000115f;
	public static int PROCESSING_ITERATIONS_MAX = 50000;
	public static float[] generatedWaveDistances;
	public static int generatedWaveDistancesCount = 0;

	private static float[] longList;

	public static LeanAudioOptions options(){
		if(generatedWaveDistances==null){
			generatedWaveDistances = new float[ PROCESSING_ITERATIONS_MAX ];
			longList = new float[ PROCESSING_ITERATIONS_MAX ];
		}
		return new LeanAudioOptions();
	}

	public static LeanAudioStream createAudioStream( AnimationCurve volume, AnimationCurve frequency, LeanAudioOptions options = null ){
		if(options==null)
			options = new LeanAudioOptions();

		options.useSetData = false;

		int generatedWavePtsLength = createAudioWave( volume, frequency, options);
		createAudioFromWave( generatedWavePtsLength, options );

		return options.stream;
	}

	
	public static AudioClip createAudio( AnimationCurve volume, AnimationCurve frequency, LeanAudioOptions options = null ){
		if(options==null)
			options = new LeanAudioOptions();
		
		int generatedWavePtsLength = createAudioWave( volume, frequency, options);
		
		return createAudioFromWave( generatedWavePtsLength, options );
	}

	private static int createAudioWave( AnimationCurve volume, AnimationCurve frequency, LeanAudioOptions options ){
		float time = volume[ volume.length - 1 ].time;
		int listLength = 0;
		

		
		
		float passed = 0f;
		for(int i = 0; i < PROCESSING_ITERATIONS_MAX; i++){
			float f = frequency.Evaluate(passed);
			if(f<MIN_FREQEUNCY_PERIOD)
				f = MIN_FREQEUNCY_PERIOD;
			float height = volume.Evaluate(passed + 0.5f*f);
			if(options.vibrato!=null){
				for(int j=0; j<options.vibrato.Length; j++){
					float peakMulti = Mathf.Abs( Mathf.Sin( 1.5708f + passed * (1f/options.vibrato[j][0]) * Mathf.PI ) );
					float diff = (1f-options.vibrato[j][1]);
					peakMulti = options.vibrato[j][1] + diff*peakMulti;
					height *= peakMulti;
				}	
			}

			
			
			if(passed + 0.5f*f>=time)
				break;
			if(listLength >= PROCESSING_ITERATIONS_MAX-1){
				Debug.LogError("LeanAudio has reached it's processing cap. To avoid this error increase the number of iterations ex: LeanAudio.PROCESSING_ITERATIONS_MAX = "+(PROCESSING_ITERATIONS_MAX*2));
				break;
			}else{
				int distPoint = listLength / 2;
				
				
				passed += f;

				generatedWaveDistances[ distPoint ] = passed;
				

				
				

				longList[ listLength ] = passed;
				longList[ listLength + 1 ] = i%2==0 ? -height : height;
			}

			

			listLength += 2;
			
		}

		listLength += -2;
		generatedWaveDistancesCount = listLength / 2;

		
		return listLength;
	}

	private static AudioClip createAudioFromWave( int waveLength, LeanAudioOptions options ){
		float time = longList[ waveLength - 2 ];
		float[] audioArr = new float[ (int)(options.frequencyRate*time) ];

		int waveIter = 0;
		float subWaveDiff = longList[waveIter];
		float subWaveTimeLast = 0f;
		float subWaveTime = longList[waveIter];
		float waveHeight = longList[waveIter+1];
		for(int i = 0; i < audioArr.Length; i++){
			float passedTime = (float)i / (float)options.frequencyRate;
			if(passedTime > longList[waveIter] ){
				subWaveTimeLast = longList[waveIter];
				waveIter += 2;
				subWaveDiff = longList[waveIter] - longList[waveIter-2];
				waveHeight = longList[waveIter+1];
				
			}
			subWaveTime = passedTime - subWaveTimeLast;
			float ratioElapsed = subWaveTime / subWaveDiff;

			float value = Mathf.Sin( ratioElapsed * Mathf.PI );

			if(options.waveStyle==LeanAudioOptions.LeanAudioWaveStyle.Square){
				if(value>0f)
					value = 1f;
				if(value<0f)
					value = -1f;
			}else if(options.waveStyle==LeanAudioOptions.LeanAudioWaveStyle.Sawtooth){
				float sign = value > 0f ? 1f : -1f;
				if(ratioElapsed<0.5f){
					value = (ratioElapsed*2f)*sign;
				}else{ 
					value = (1f - ratioElapsed)*2f*sign;
				}
			}else if(options.waveStyle==LeanAudioOptions.LeanAudioWaveStyle.Noise){
				float peakMulti = (1f-options.waveNoiseInfluence) + Mathf.PerlinNoise(0f, passedTime * options.waveNoiseScale ) * options.waveNoiseInfluence;
				
				

				value *= peakMulti;
			}
			
			
			
			
			value *= waveHeight;


			if(options.modulation!=null){
				for(int k=0; k<options.modulation.Length; k++){
					float peakMulti = Mathf.Abs( Mathf.Sin( 1.5708f + passedTime * (1f/options.modulation[k][0]) * Mathf.PI ) );
					float diff = (1f-options.modulation[k][1]);
					peakMulti = options.modulation[k][1] + diff*peakMulti;
					
						
					
					value *= peakMulti;
				}	
			}

			audioArr[i] = value;
			
		}

		
		int lengthSamples = audioArr.Length;
		
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
		bool is3dSound = false;
		AudioClip audioClip = AudioClip.Create("Generated Audio", lengthSamples, 1, options.frequencyRate, is3dSound, false);
		#else
		AudioClip audioClip = null;
		if(options.useSetData){
			audioClip = AudioClip.Create("Generated Audio", lengthSamples, 1, options.frequencyRate, false, null, OnAudioSetPosition);
			audioClip.SetData(audioArr, 0);
		}else{
			options.stream = new LeanAudioStream(audioArr);
			
			audioClip = AudioClip.Create("Generated Audio", lengthSamples, 1, options.frequencyRate, false, options.stream.OnAudioRead, options.stream.OnAudioSetPosition);
			options.stream.audioClip = audioClip;
		}
		
		#endif

		return audioClip;
	}

	private static void OnAudioSetPosition(int newPosition) {
        
    }

	public static AudioClip generateAudioFromCurve( AnimationCurve curve, int frequencyRate = 44100 ){
		float curveTime = curve[ curve.length - 1 ].time;
		float time = curveTime;
		float[] audioArr = new float[ (int)(frequencyRate*time) ];

		
		for(int i = 0; i < audioArr.Length; i++){
			float pt = (float)i / (float)frequencyRate;
			audioArr[i] = curve.Evaluate( pt );
			
		}

		int lengthSamples = audioArr.Length;
		#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
		bool is3dSound = false;
		AudioClip audioClip = AudioClip.Create("Generated Audio", lengthSamples, 1, frequencyRate, is3dSound, false);
		#else
		AudioClip audioClip = AudioClip.Create("Generated Audio", lengthSamples, 1, frequencyRate, false);
		#endif
		audioClip.SetData(audioArr, 0);

		return audioClip;
	}

	public static AudioSource play( AudioClip audio, float volume ){
		AudioSource audioSource = playClipAt(audio, Vector3.zero);
		audioSource.volume = volume;
		return audioSource; 
	}

	public static AudioSource play( AudioClip audio ){
		return playClipAt( audio, Vector3.zero ); 
	}

	public static AudioSource play( AudioClip audio, Vector3 pos ){
		return playClipAt( audio, pos ); 
	}

	public static AudioSource play( AudioClip audio, Vector3 pos, float volume ){
		
		AudioSource audioSource = playClipAt(audio, pos);
		audioSource.minDistance = 1f;
		
		audioSource.volume = volume;

		return audioSource;
	}

	public static AudioSource playClipAt( AudioClip clip, Vector3 pos ) {
		GameObject tempGO = new GameObject(); 
		tempGO.transform.position = pos; 
		AudioSource aSource = tempGO.AddComponent<AudioSource>(); 
		aSource.clip = clip; 
		aSource.Play(); 
		GameObject.Destroy(tempGO, clip.length); 
		return aSource; 
	}

	public static void printOutAudioClip( AudioClip audioClip, ref AnimationCurve curve, float scaleX = 1f ){
		
		float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);
        int i = 0;

        Keyframe[] frames = new Keyframe[samples.Length];
        while (i < samples.Length) {
           frames[i] = new Keyframe( (float)i * scaleX, samples[i] );
           ++i;
        }
        curve = new AnimationCurve( frames );
	}
}



public class LeanAudioOptions : object {

	public enum LeanAudioWaveStyle{
		Sine,
		Square,
		Sawtooth,
		Noise
	}

	public LeanAudioWaveStyle waveStyle = LeanAudioWaveStyle.Sine;
	public Vector3[] vibrato;
	public Vector3[] modulation;
	public int frequencyRate = 44100;
	public float waveNoiseScale = 1000;
	public float waveNoiseInfluence = 1f;

	public bool useSetData = true;
	public LeanAudioStream stream;

	public LeanAudioOptions(){}

	
	public LeanAudioOptions setFrequency( int frequencyRate ){
		this.frequencyRate = frequencyRate;
		return this;
	}

	
	public LeanAudioOptions setVibrato( Vector3[] vibrato ){
		this.vibrato = vibrato;
		return this;
	}

	

	public LeanAudioOptions setWaveSine(){
		this.waveStyle = LeanAudioWaveStyle.Sine;
		return this;
	}

	public LeanAudioOptions setWaveSquare(){
		this.waveStyle = LeanAudioWaveStyle.Square;
		return this;
	}

	public LeanAudioOptions setWaveSawtooth(){
		this.waveStyle = LeanAudioWaveStyle.Sawtooth;
		return this;
	}

	public LeanAudioOptions setWaveNoise(){
		this.waveStyle = LeanAudioWaveStyle.Noise;
		return this;
	}

	public LeanAudioOptions setWaveStyle( LeanAudioWaveStyle style ){
		this.waveStyle = style;
		return this;
	}


	public LeanAudioOptions setWaveNoiseScale( float waveScale ){
		this.waveNoiseScale = waveScale;
		return this;
	}

	public LeanAudioOptions setWaveNoiseInfluence( float influence ){
		this.waveNoiseInfluence = influence;
		return this;
	}
}


