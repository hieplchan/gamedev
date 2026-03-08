using UnityEngine;
using TMPro;
using BrunoMikoski.AnimationSequencer;

public class GameManager : MonoBehaviour {
    [SerializeField] private PlayerController player;

    [Header("Camera")]
    [SerializeField] private Transform topCam;
    [SerializeField] private Transform underWaterCam;

    [Header("UI")]
    [SerializeField] private GameObject objUI;
    [SerializeField] private TMP_Text objUIText;

    [Header("Animation Sequencer Controller")]
    [SerializeField] private AnimationSequencerController _jumpToSwimAnimSeq;

    private void OnEnable() =>
        player.OnSwimInteractStateChanged += HandleSwimInteractStateChanged;

    private void OnDisable() =>
        player.OnSwimInteractStateChanged -= HandleSwimInteractStateChanged;

    private void HandleSwimInteractStateChanged() {
        objUI.SetActive(player.IsInSwimTriggerZone);
        if (player.IsInSwimTriggerZone) {
            objUIText.text = "X - Swim";
        }
    }
}