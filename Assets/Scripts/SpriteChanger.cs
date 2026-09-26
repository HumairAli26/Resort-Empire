using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpriteChangerTime : MonoBehaviour
{
    public Image uiImage;          // Drag your UI Image GameObject here in the Inspector
    public Sprite secondarySprite; // Drag the new sprite here in the Inspector
    public float delaySeconds = 3f; // Time to stay on the original sprite
    public float returnSeconds = 3f; // Time to stay on the secondary sprite

    private Sprite originalSprite; // Stores the starting sprite automatically

    void Start()
    {
        // Save whatever sprite is currently assigned to the UI image when the game starts
        if (uiImage != null)
        {
            originalSprite = uiImage.sprite;
        }

        // Start the infinite sequence loop
        StartCoroutine(InfiniteSpriteSequence());
    }

    IEnumerator InfiniteSpriteSequence()
    {
        // Safety check to ensure we have images assigned to prevent crashes/errors
        if (uiImage == null || secondarySprite == null || originalSprite == null)
        {
            Debug.LogError("SpriteChangerTime: Missing Image or Sprite references in the inspector!");
            yield break; // Exit the coroutine safely
        }

        // This loop runs endlessly as long as the GameObject is active
        while (true)
        {
            // 1. Wait while showing the original sprite
            yield return new WaitForSeconds(delaySeconds);

            // 2. Swap to the secondary sprite
            uiImage.sprite = secondarySprite;

            // 3. Wait while showing the secondary sprite
            yield return new WaitForSeconds(returnSeconds);

            // 4. Swap back to the original sprite
            uiImage.sprite = originalSprite;
        }
    }
}
