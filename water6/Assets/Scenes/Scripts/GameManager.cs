using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObject player;

    [Header("Camera")]
    [SerializeField] private Transform topCam;
    [SerializeField] private Transform underWaterCam;

    [Header("UI")]
    [SerializeField] private GameObject objUI;
    [SerializeField] private TMP_Text objUIText;

    private bool _isPlayerInSwimTriggerZone = false;

    public void OnPlayerEnterSwimTriggerZone() {
        _isPlayerInSwimTriggerZone = true;
        objUI.SetActive(true);
        objUIText.text = "X - Swim";
    }

    public void OnPlayerExitSwimTriggerZone() {
        _isPlayerInSwimTriggerZone = false;
        objUI.SetActive(false);
    }
}