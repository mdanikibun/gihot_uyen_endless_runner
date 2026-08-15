using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileGameplayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] Button pauseButton;
    [SerializeField] Button jumpButton;
    [SerializeField] RectTransform moveStick;

    [Header("Move Stick")]
    [SerializeField] float moveDeadZone = 0.2f;
    [SerializeField] float moveVisualRange = 80f;

    [Header("Debug")]
    [SerializeField] bool forceShowInEditor;

    bool stickActive;
    float stickMoveX;

    void Awake() {
        ResolveReferences();
        EnsureButtons();
        BindButtons();
        ApplyVisibility();
    }

    void OnEnable() {
        ApplyVisibility();
    }

    void OnDisable() {
        StopMobileMove();
    }

    void Update() {
        if (!gameObject.activeInHierarchy) return;
        if (!stickActive) return;

        PlayerController player = GetActivePlayer();
        if (player != null) {
            player.SetMoveX(stickMoveX);
        }
    }

    void ResolveReferences() {
        if (gameFlow == null) {
            gameFlow = FindAnyObjectByType<GameFlowController>();
        }

        if (pauseButton == null) {
            Transform pause = transform.Find("Pause Icon");
            if (pause != null) {
                pauseButton = pause.GetComponent<Button>();
            }
        }

        if (jumpButton == null) {
            Transform jump = transform.Find("Jump Icon");
            if (jump != null) {
                jumpButton = jump.GetComponent<Button>();
            }
        }

        if (moveStick == null) {
            Transform stick = transform.Find("joystick Left Right");
            if (stick != null) {
                moveStick = stick as RectTransform;
            }
        }
    }

    void EnsureButtons() {
        if (pauseButton == null) {
            Transform pause = transform.Find("Pause Icon");
            if (pause != null) {
                pauseButton = pause.GetComponent<Button>();
                if (pauseButton == null) {
                    pauseButton = pause.gameObject.AddComponent<Button>();
                    pauseButton.targetGraphic = pause.GetComponent<Graphic>();
                }
            }
        }

        if (jumpButton == null) {
            Transform jump = transform.Find("Jump Icon");
            if (jump != null) {
                jumpButton = jump.GetComponent<Button>();
                if (jumpButton == null) {
                    jumpButton = jump.gameObject.AddComponent<Button>();
                    jumpButton.targetGraphic = jump.GetComponent<Graphic>();
                }
            }
        }

        if (moveStick != null) {
            MobileLeftRightStick stick = moveStick.GetComponent<MobileLeftRightStick>();
            if (stick == null) {
                stick = moveStick.gameObject.AddComponent<MobileLeftRightStick>();
            }
            stick.Setup(OnStickMoveX, moveDeadZone, moveVisualRange);
        }
    }

    void BindButtons() {
        if (pauseButton != null) {
            pauseButton.onClick.RemoveListener(OnPauseClicked);
            pauseButton.onClick.AddListener(OnPauseClicked);
        }

        if (jumpButton != null) {
            jumpButton.onClick.RemoveListener(OnJumpClicked);
            jumpButton.onClick.AddListener(OnJumpClicked);
        }
    }

    void ApplyVisibility() {
        bool show = ShouldShowMobileControls();
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(show);
        }

        if (!show) {
            StopMobileMove();
        }
    }

    bool ShouldShowMobileControls() {
        if (Application.isMobilePlatform) return true;
#if UNITY_EDITOR
        return forceShowInEditor;
#else
        return false;
#endif
    }

    void OnPauseClicked() {
        if (gameFlow == null) return;
        gameFlow.PauseGame();
    }

    void OnJumpClicked() {
        PlayerController player = GetActivePlayer();
        if (player == null) return;
        player.TryJump();
    }

    void OnStickMoveX(float moveX) {
        stickActive = Mathf.Abs(moveX) > 0.001f;
        stickMoveX = moveX;

        PlayerController player = GetActivePlayer();
        if (player != null) {
            player.SetMoveX(moveX);
        }
    }

    void StopMobileMove() {
        stickActive = false;
        stickMoveX = 0f;
        PlayerController player = GetActivePlayer();
        if (player != null) {
            player.SetMoveX(0f);
        }
    }

    PlayerController GetActivePlayer() {
        return gameFlow != null ? gameFlow.ActivePlayerController : null;
    }
}

public class MobileLeftRightStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    RectTransform rect;
    RectTransform parentRect;
    Vector2 defaultAnchoredPosition;
    Vector2 defaultLocalInParent;
    System.Action<float> onMoveX;
    float deadZone = 0.2f;
    float maxRange = 80f;

    public void Setup(System.Action<float> moveCallback, float moveDeadZone, float visualRange) {
        onMoveX = moveCallback;
        deadZone = Mathf.Clamp01(moveDeadZone);
        maxRange = Mathf.Max(1f, visualRange);
        CacheTransforms();
        CacheDefaultPose();
    }

    void CacheTransforms() {
        rect = transform as RectTransform;
        parentRect = rect != null ? rect.parent as RectTransform : null;
    }

    void CacheDefaultPose() {
        if (rect == null) return;
        defaultAnchoredPosition = rect.anchoredPosition;
        if (parentRect != null) {
            defaultLocalInParent = parentRect.InverseTransformPoint(rect.position);
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        UpdateMove(eventData);
    }

    public void OnDrag(PointerEventData eventData) {
        UpdateMove(eventData);
    }

    public void OnPointerUp(PointerEventData eventData) {
        ResetStick();
    }

    void OnDisable() {
        ResetStick();
    }

    void ResetStick() {
        if (rect != null) {
            rect.anchoredPosition = defaultAnchoredPosition;
        }
        onMoveX?.Invoke(0f);
    }

    void UpdateMove(PointerEventData eventData) {
        if (rect == null || parentRect == null) {
            CacheTransforms();
            if (rect == null || parentRect == null) return;
        }

        Camera eventCamera = eventData.pressEventCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventCamera, out Vector2 pointerLocal)) {
            return;
        }

        float offsetX = Mathf.Clamp(pointerLocal.x - defaultLocalInParent.x, -maxRange, maxRange);
        rect.anchoredPosition = defaultAnchoredPosition + new Vector2(offsetX, 0f);

        float normalizedX = offsetX / maxRange;
        if (Mathf.Abs(normalizedX) < deadZone) {
            normalizedX = 0f;
        } else {
            float sign = Mathf.Sign(normalizedX);
            normalizedX = sign * Mathf.InverseLerp(deadZone, 1f, Mathf.Abs(normalizedX));
        }

        onMoveX?.Invoke(normalizedX);
    }
}
