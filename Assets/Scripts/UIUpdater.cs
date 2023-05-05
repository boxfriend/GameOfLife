using UnityEngine;


public class UIUpdater : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private TMPro.TMP_Text _alive, _dead, _round;

    private void Awake ()
    {
        _gridManager.OnRoundComplete += SetTexts;
    }

    private void SetTexts(int alive, int dead, int round)
    {
        _alive.text = alive.ToString();
        _dead.text = dead.ToString();
        _round.text = round.ToString();
    }
}

