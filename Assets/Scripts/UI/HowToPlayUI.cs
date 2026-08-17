using UnityEngine;
using UnityEngine.UI;

public class HowToPlayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] Button runButton;
    [SerializeField] GameObject textGuidePc;
    [SerializeField] GameObject textGuideMobile;
    [SerializeField] Image guideImage;
    [SerializeField] Sprite imagePc;
    [SerializeField] Sprite imageMobile;

    void Awake() {
        runButton.onClick.AddListener(OnRunClicked);
        ApplyGuideForPlatform();
    }

    void OnEnable() {
        ApplyGuideForPlatform();
    }

    public void OnRunClicked() {
        gameFlow.StartRun();
    }

    void ApplyGuideForPlatform() {
        bool useMobile = Application.isMobilePlatform;

        if (textGuidePc != null) {
            textGuidePc.SetActive(!useMobile);
        }

        if (textGuideMobile != null) {
            textGuideMobile.SetActive(useMobile);
        }

        if (guideImage != null) {
            Sprite selected = useMobile ? imageMobile : imagePc;
            if (selected != null) {
                guideImage.sprite = selected;
            }
        }
    }
}
