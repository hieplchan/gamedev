using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObject player;

    [Header("Camera")]
    [SerializeField] private Camera topCam;
    [SerializeField] private Camera underWaterCam;

    [Header("UI")]
    [SerializeField] private GameObject objUI;
    [SerializeField] private TMP_Text objUIText;

    public void OnPlayerEnterSwimTriggerZone() {
        objUI.SetActive(true);
        objUIText.text = "X - Swim";
    }

    public void OnPlayerExitSwimTriggerZone() {
        objUI.SetActive(false);
    }
}