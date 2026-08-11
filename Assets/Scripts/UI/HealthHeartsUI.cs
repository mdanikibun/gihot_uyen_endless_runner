using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthHeartsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Sprite heartSprite;
    [SerializeField] Transform heartsParent;

    [Header("Layout")]
    [SerializeField] Vector2 heartSize = new Vector2(64f, 64f);

    readonly List<Image> hearts = new List<Image>();

    public void SetHealth(int currentHealth, int maxHealth) {
        if (maxHealth < 0) maxHealth = 0;
        if (currentHealth < 0) currentHealth = 0;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        EnsureHeartCount(maxHealth);

        for (int i = 0; i < hearts.Count; i++) {
            hearts[i].gameObject.SetActive(i < currentHealth);
        }
    }

    void EnsureHeartCount(int maxHealth) {
        while (hearts.Count < maxHealth) {
            hearts.Add(CreateHeart());
        }

        for (int i = hearts.Count - 1; i >= maxHealth; i--) {
            Destroy(hearts[i].gameObject);
            hearts.RemoveAt(i);
        }
    }

    Image CreateHeart() {
        GameObject heart = new GameObject("Heart", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        heart.transform.SetParent(heartsParent, false);

        RectTransform rect = heart.GetComponent<RectTransform>();
        rect.sizeDelta = heartSize;

        LayoutElement layout = heart.GetComponent<LayoutElement>();
        layout.preferredWidth = heartSize.x;
        layout.preferredHeight = heartSize.y;
        layout.minWidth = heartSize.x;
        layout.minHeight = heartSize.y;

        Image image = heart.GetComponent<Image>();
        image.sprite = heartSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        return image;
    }
}
