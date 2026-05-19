using NoSuchStudio.Common;
using NoSuchStudio.Variables;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NoSuchMonoBehaviour {

    [SerializeField] GameObject _enemyPrefab;
    [SerializeField] GameObject _panelEnded;
    [SerializeField] GameObject _txtHighscore;
    [SerializeField] Slider _sliderHP;
    [SerializeField] Text _textScore;
    [SerializeField] Text _textHP;
    
    [Header("Parameters")]
    [SerializeField] float _spawnInterval;


    Enemy _currentEnemy;
    bool _ended;

    public static int GetVariableAsInt(string variable) {
        var str = VariablesService.GetVariable(variable);
        int result;
        int.TryParse(str, out result);
        return result;
    }

    public static void SetVariableAsInt(string variable, int value) {
        VariablesService.SetVariable(variable, value.ToString());
    }

    public void Restart() {
        _panelEnded.SetActive(false);
        _ended = false;
        enabled = true;
        SetVariableAsInt("hp", 100);
        SetVariableAsInt("score", 0);
    }

    public void OnVariablesChanged() {
        var hp = GetVariableAsInt("hp");
        var score = GetVariableAsInt("score");

        _sliderHP.value = hp;
        _textHP.text = "" + hp;
        _textScore.text = "" + score;

        if (hp <= 0 && !_ended) {
            _ended = true;
            _panelEnded.SetActive(true);
            _txtHighscore.SetActive(false);
            var highscore = GetVariableAsInt("highscore");
            if (score > highscore) {
                SetVariableAsInt("highscore", score);
                _txtHighscore.SetActive(true);
            }
            enabled = false;
        }
    }

    void Update() {
        if (_ended) return;

        if (!_currentEnemy) {
            var pos = Random.insideUnitSphere * 4;
            _currentEnemy = Instantiate(_enemyPrefab, pos, Quaternion.identity, transform).GetComponent<Enemy>();
            _currentEnemy.lifeTime = _spawnInterval;
        }
    }
}
