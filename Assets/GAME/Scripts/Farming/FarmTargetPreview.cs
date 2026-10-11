using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class FarmTargetPreview : MonoBehaviour
{
    private SpriteRenderer previewRenderer;

    public void Initialize(Tilemap tilemap, SpriteRenderer renderer)
    {
        previewRenderer = renderer;

        if (previewRenderer != null && tilemap != null && previewRenderer.sprite != null)
        {
            Vector3 cellSize = tilemap.cellSize;
            Vector3 spriteSize = previewRenderer.sprite.bounds.size;

            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                previewRenderer.transform.localScale = new Vector3(
                    cellSize.x / spriteSize.x,
                    cellSize.y / spriteSize.y,
                    1f
                );
            }
        }

        Hide();
    }

    public void ShowAt(Vector3 worldPosition)
    {
        if (previewRenderer == null)
            return;

        previewRenderer.transform.position = worldPosition;
        previewRenderer.enabled = true;
    }

    public void Hide()
    {
        if (previewRenderer != null)
            previewRenderer.enabled = false;
    }
}
