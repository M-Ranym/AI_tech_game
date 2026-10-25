using UnityEngine;
using System.Collections;
using DentedPixel;

public class TestingColorTweening : MonoBehaviour {

	
	void Start () {
		
		LeanTween.value(gameObject, Color.red, Color.green, 1f)
                .setOnUpdate(OnTweenUpdate)
                .setOnUpdateParam(new object[] { "" + 2 });
	}

	private void OnTweenUpdate( Color update, object obj){
		object[] objArr = obj as object[];
		Debug.Log("update:"+update+" obj:"+objArr[0]);
		
	}
	
}
