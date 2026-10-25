using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingSequencer : MonoBehaviour {
	public GameObject cube1;

	public void Start(){


		var seq = LeanTween.sequence();
		seq.append(() => { 
			Debug.Log("firsts:"+Time.time);
		});
		seq.append(1f); 
		seq.append(() => { 
			Debug.Log("I have started:"+Time.time);
		});
		seq.append( LeanTween.move(cube1, Vector3.one * 10f, 1f) ); 
		seq.append((object obj) => { 
			var dict = obj as Dictionary<string,string>;
			Debug.Log("We are done now obj value:"+dict["hi"]);
		}, new Dictionary<string,string>(){ {"hi","sup"} } );
	}
}
