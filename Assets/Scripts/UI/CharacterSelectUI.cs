using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    const string previewLayerName = "CharacterPreview";
    const string runStateName = "Root|Run";
    const string runSpeedParam = "RunAnimSpeed";

    [Header("References")]
    [SerializeField] GameFlowController gameFlow;
    [SerializeField] TMP_InputField playerNameInput;
    [SerializeField] Button[] characterSlots;
    [SerializeField] Button nextButton;
    [SerializeField] CharacterOption[] characters;
    [SerializeField] GameSettings settings;

    CharacterPreviewSlot[] previewSlots;
    GameObject previewRoot;
    readonly List<RenderTexture> previewTextures = new List<RenderTexture>();
    int previewLayer;
    int selectedIndex = -1;

    public int SelectedIndex => selectedIndex;
    public string PlayerName {
        get {
            return playerNameInput.text.Trim();
        }
    }

    bool HasValidName => !string.IsNullOrWhiteSpace(PlayerName);

    void Awake() {
        playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
        SetupSlotListeners();
        nextButton.onClick.AddListener(OnNextClicked);
        BuildCharacterPreviews();
        RefreshNextButton();
    }

    void OnEnable() {
        selectedIndex = -1;
        if (previewRoot != null) {
            previewRoot.SetActive(true);
        }
        RefreshSlotVisuals();
        RefreshNextButton();
    }

    void OnDisable() {
        if (previewRoot != null) {
            previewRoot.SetActive(false);
        }
    }

    void OnDestroy() {
        for (int i = 0; i < previewTextures.Count; i++) {
            if (previewTextures[i] != null) {
                previewTextures[i].Release();
            }
        }
        previewTextures.Clear();

        if (previewRoot != null) {
            Destroy(previewRoot);
        }
    }

    public void OnCharacterClicked(int index) {
        if (index < 0 || index >= characterSlots.Length) return;

        selectedIndex = index;
        RefreshSlotVisuals();
        RefreshNextButton();

        gameFlow.SetSelectedCharacter(index, GetSelectedOption());
    }

    public void OnNextClicked() {
        if (!CanGoNext()) return;

        gameFlow.SetPlayerName(PlayerName);
        gameFlow.ShowHowToPlay();
    }

    void OnPlayerNameChanged(string _) {
        RefreshNextButton();
    }

    bool CanGoNext() {
        return selectedIndex >= 0 && HasValidName;
    }

    void RefreshNextButton() {
        nextButton.interactable = CanGoNext();
    }

    CharacterOption GetSelectedOption() {
        if (selectedIndex < 0 || selectedIndex >= characters.Length) {
            return null;
        }
        return characters[selectedIndex];
    }

    void RefreshSlotVisuals() {
        for (int i = 0; i < characterSlots.Length; i++) {
            Image image = characterSlots[i].targetGraphic as Image;
            image.color = i == selectedIndex ? settings.characterSelect.selectedSlotColor : settings.characterSelect.normalSlotColor;

            if (previewSlots != null && i < previewSlots.Length && previewSlots[i] != null) {
                previewSlots[i].SetSelected(i == selectedIndex);
            }
        }
    }

    void SetupSlotListeners() {
        for (int i = 0; i < characterSlots.Length; i++) {
            int index = i;
            characterSlots[i].onClick.AddListener(() => OnCharacterClicked(index));
        }
    }

    void BuildCharacterPreviews() {
        previewLayer = LayerMask.NameToLayer(previewLayerName);
        if (previewLayer < 0) {
            previewLayer = 8;
        }

        ExcludePreviewLayerFromSceneCameras();

        previewRoot = new GameObject("Character Select Previews");

        previewSlots = new CharacterPreviewSlot[characterSlots.Length];

        for (int i = 0; i < characterSlots.Length; i++) {
            previewSlots[i] = CreatePreview(i);
        }
    }

    CharacterPreviewSlot CreatePreview(int index) {
        Button slot = characterSlots[index];
        GameObject prefab = GetPrefab(index);
        if (prefab == null) return null;

        if (slot.GetComponent<RectMask2D>() == null) {
            slot.gameObject.AddComponent<RectMask2D>();
        }

        RenderTexture renderTexture = CreateRenderTexture(slot.transform as RectTransform);
        RawImage rawImage = CreateRawImage(slot.transform, renderTexture);

        Vector3 stagePosition = new Vector3(250f + index * 8f, 0f, 0f);
        GameObject stage = new GameObject("Preview Stage " + index);
        stage.transform.SetParent(previewRoot.transform, false);
        stage.transform.position = stagePosition;

        GameObject model = Instantiate(prefab, stage.transform);
        model.name = prefab.name;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        DisableGameplayComponents(model);
        SetLayerRecursively(model, previewLayer);

        SkinnedMeshRenderer[] skinnedMeshes = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < skinnedMeshes.Length; i++) {
            skinnedMeshes[i].updateWhenOffscreen = true;
        }

        Animator animator = model.GetComponentInChildren<Animator>();
        if (animator != null) {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.applyRootMotion = false;
            animator.SetFloat(runSpeedParam, 1f);
            animator.Play(runStateName, 0, 0f);
            animator.Update(0f);
        }

        Camera previewCamera = CreatePreviewCamera(stage.transform, model, renderTexture);
        CreatePreviewLight(stage.transform);
        StartCoroutine(ReframeWhenReady(previewCamera, model));

        CharacterPreviewSlot previewSlot = slot.gameObject.AddComponent<CharacterPreviewSlot>();
        previewSlot.Setup(model.transform, settings.characterSelect.previewRotateSpeed);
        rawImage.enabled = true;

        return previewSlot;
    }

    GameObject GetPrefab(int index) {
        if (index < 0 || index >= characters.Length) return null;

        return characters[index] != null ? characters[index].playerPrefab : null;
    }

    RawImage CreateRawImage(Transform slot, RenderTexture renderTexture) {
        GameObject imageObject = new GameObject("Character Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        imageObject.transform.SetParent(slot, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(8f, 8f);
        rect.offsetMax = new Vector2(-8f, -8f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        RawImage rawImage = imageObject.GetComponent<RawImage>();
        rawImage.texture = renderTexture;
        rawImage.color = Color.white;
        rawImage.raycastTarget = false;

        return rawImage;
    }

    RenderTexture CreateRenderTexture(RectTransform slotRect) {
        int width = 512;
        int height = 640;
        if (slotRect != null && slotRect.rect.width > 1f && slotRect.rect.height > 1f) {
            width = Mathf.Clamp(Mathf.RoundToInt(slotRect.rect.width * 2f), 256, 1024);
            height = Mathf.Clamp(Mathf.RoundToInt(slotRect.rect.height * 2f), 256, 1024);
        }

        RenderTexture renderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 1;
        renderTexture.Create();
        previewTextures.Add(renderTexture);

        return renderTexture;
    }

    Camera CreatePreviewCamera(Transform stage, GameObject model, RenderTexture renderTexture) {
        GameObject cameraObject = new GameObject("Preview Camera");
        cameraObject.transform.SetParent(stage, false);

        Camera previewCamera = cameraObject.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.clear;
        previewCamera.fieldOfView = 28f;
        previewCamera.nearClipPlane = 0.05f;
        previewCamera.farClipPlane = 20f;
        previewCamera.cullingMask = 1 << previewLayer;
        previewCamera.targetTexture = renderTexture;
        previewCamera.depth = -100;
        previewCamera.allowHDR = false;
        previewCamera.allowMSAA = false;

        UniversalAdditionalCameraData cameraData = cameraObject.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null) {
            cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        }
        cameraData.renderType = CameraRenderType.Base;
        cameraData.renderPostProcessing = false;
        cameraData.renderShadows = false;

        FrameCamera(previewCamera, model);

        return previewCamera;
    }

    void FrameCamera(Camera previewCamera, GameObject model) {
        if (!TryGetBounds(model, out Bounds bounds)) {
            previewCamera.transform.position = model.transform.position + new Vector3(0f, 0.9f, 2.2f);
            previewCamera.transform.LookAt(model.transform.position + Vector3.up * 0.7f);
            
            return;
        }

        float padding = settings.characterSelect.previewFitPadding;
        float fov = previewCamera.fieldOfView * Mathf.Deg2Rad;
        float halfHeight = Mathf.Max(bounds.extents.y, 0.01f);
        float halfWidth = Mathf.Max(bounds.extents.x, 0.01f);
        float distanceForHeight = halfHeight / Mathf.Tan(fov * 0.5f);
        float distanceForWidth = halfWidth / (Mathf.Tan(fov * 0.5f) * previewCamera.aspect);
        float distance = Mathf.Max(distanceForHeight, distanceForWidth) * padding + bounds.extents.z;

        Vector3 lookPoint = bounds.center;
        previewCamera.transform.position = lookPoint + Vector3.forward * distance;
        previewCamera.transform.LookAt(lookPoint);
    }

    IEnumerator ReframeWhenReady(Camera previewCamera, GameObject model) {
        yield return null;
        if (previewCamera == null || model == null) yield break;
        FrameCamera(previewCamera, model);
    }

    bool TryGetBounds(GameObject model, out Bounds bounds) {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        bool hasBounds = false;
        bounds = new Bounds(model.transform.position, Vector3.zero);

        for (int i = 0; i < renderers.Length; i++) {
            if (!renderers[i].enabled) continue;
            if (!hasBounds) {
                bounds = renderers[i].bounds;
                hasBounds = true;
            } else {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        return hasBounds;
    }

    void CreatePreviewLight(Transform stage) {
        GameObject lightObject = new GameObject("Preview Light");
        lightObject.transform.SetParent(stage, false);
        lightObject.transform.localPosition = new Vector3(0.4f, 1.8f, 1.2f);

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.white;
        light.intensity = 1.4f;
        light.range = 8f;
        light.shadows = LightShadows.None;
        light.cullingMask = 1 << previewLayer;
    }

    void DisableGameplayComponents(GameObject model) {
        PlayerController playerController = model.GetComponent<PlayerController>();
        if (playerController != null) playerController.enabled = false;

        PlayerCollisionHandler collisionHandler = model.GetComponent<PlayerCollisionHandler>();
        if (collisionHandler != null) collisionHandler.enabled = false;

        UnityEngine.InputSystem.PlayerInput playerInput = model.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        Rigidbody body = model.GetComponent<Rigidbody>();
        if (body != null) {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.isKinematic = true;
            body.constraints = RigidbodyConstraints.FreezeAll;
        }

        Collider[] colliders = model.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = false;
        }
    }

    void SetLayerRecursively(GameObject target, int layer) {
        target.layer = layer;
        Transform transform = target.transform;
        for (int i = 0; i < transform.childCount; i++) {
            SetLayerRecursively(transform.GetChild(i).gameObject, layer);
        }
    }

    void ExcludePreviewLayerFromSceneCameras() {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include);
        int excludeMask = ~(1 << previewLayer);
        for (int i = 0; i < cameras.Length; i++) {
            cameras[i].cullingMask &= excludeMask;
        }
    }
}
