using NoSuchStudio.Common;
using NoSuchStudio.Variables;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : NoSuchMonoBehaviour {

    [SerializeField] AudioClip _clipBurst;
    [SerializeField] AudioClip _clipPop;

    [SerializeField] Color _startColor;
    [SerializeField] Color _endColor;
    [SerializeField] float _lifeTime;
    public float lifeTime {
        get { return _lifeTime; }
        set { _lifeTime = value; }
    }

    float _spawnTime;
    SpriteRenderer _sr;
    AudioSource _as;
    void Start() {
        _spawnTime = Time.time;
        _sr = GetComponent<SpriteRenderer>();
        _as = GetComponent<AudioSource>();
        _as.clip = _clipPop;
        _as.Play();
    }

    void OnMouseDown() {
        var score = GameManager.GetVariableAsInt("score");
        GameManager.SetVariableAsInt("score", score + 10);
        _as.clip = _clipBurst;
        _as.Play();
        RunDelayed(0.2f, () => { Destroy(gameObject); });
        enabled = false;
    }

    void Update() {
        float livedTime = Time.time - _spawnTime;
        float ratio = livedTime / _lifeTime;
        _sr.color = Color.Lerp(_startColor, _endColor, ratio);

        if (Time.time > _spawnTime + _lifeTime) {
            var hp = GameManager.GetVariableAsInt("hp");
            GameManager.SetVariableAsInt("hp", hp - 10);
            Destroy(gameObject);
        }
    }
}
