using UnityEngine;
using UnityEngine.UI;

public class HowToPlayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] Button runButton;
    [SerializeField] Image guideImage;
    [SerializeField] Sprite guidePc;
    [SerializeField] Sprite guideMobile;

    void Awake() {
        runButton.onClick.AddListener(OnRunClicked);
        ResolveGuideImage();
        ApplyGuideSprite();
    }

    void OnEnable() {
        ApplyGuideSprite();
    }

    public void OnRunClicked() {
        gameFlow.StartRun();
    }

    void ResolveGuideImage() {
        if (guideImage != null) return;

        Transform content = transform.Find("Content Guide");
        if (content == null) {
            content = FindDeepChild(transform, "Content Guide");
        }

        Transform image = content != null ? content.Find("Image Guide") : FindDeepChild(transform, "Image Guide");
        if (image != null) {
            guideImage = image.GetComponent<Image>();
        }
    }

    void ApplyGuideSprite() {
        if (guideImage == null) return;

        bool useMobile = Application.isMobilePlatform;
        Sprite selected = useMobile ? guideMobile : guidePc;
        if (selected != null) {
            guideImage.sprite = selected;
        }
    }

    static Transform FindDeepChild(Transform parent, string name) {
        for (int i = 0; i < parent.childCount; i++) {
            Transform child = parent.GetChild(i);
            if (child.name == name) return child;
            Transform nested = FindDeepChild(child, name);
            if (nested != null) return nested;
        }
        return null;
    }
}
