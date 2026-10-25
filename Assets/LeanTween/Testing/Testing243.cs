using UnityEngine;
using System.Collections;

public class Testing243 : MonoBehaviour {
	public GameObject cube1;
	public Light lightInScene;

	public RectTransform imageRectTransform;

	









	void Start () {


		


		LeanTween.sequence().append(LeanTween.scale(cube1, Vector3.one * 3f, .1f)).append(LeanTween.scale(cube1, Vector3.one*5.6f, .2f));
        LeanTween.sequence().append(LeanTween.value (lightInScene.gameObject, (float f) =>	lightInScene.range = f, 8, 12, .1f)).append (LeanTween.value (lightInScene.gameObject, (float f) =>	lightInScene.range = f, 12, 8, .2f));
	} 
	
	
	void Update () {
	
	}
}
