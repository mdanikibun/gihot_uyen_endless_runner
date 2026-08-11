using UnityEngine;
using UnityEngine.UI;

public class HowToPlayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] Button runButton;

    void Awake() {
        runButton.onClick.AddListener(OnRunClicked);
    }

    public void OnRunClicked() {
        gameFlow.StartRun();
    }
}
