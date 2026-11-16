using UnityEngine;
using UnityEngine.InputSystem;

public class Building : MonoBehaviour
{
    private bool isSelected = false;
    GridManager gridManager;

    private SpriteRendererState[] spriteRendererStates = new SpriteRendererState[0];
    private bool isSpriteRendererStateModified = false;

    void Awake()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        spriteRendererStates = new SpriteRendererState[spriteRenderers.Length];

        for(int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRendererStates[i] = new SpriteRendererState(spriteRenderers[i]);
        }
    }

    void Start()
    {
        gridManager = GridManager.Instance;
    }

    void Update()
    {
        HandleBuildingPlacement();
        HandleSelectionEffect();
    }

    void HandleBuildingPlacement()
    {
        if (!isSelected) return;
        Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
        Vector2 pointerPos = Camera.main.ScreenToWorldPoint(new Vector3(pointerScreenPos.x, pointerScreenPos.y, -Camera.main.transform.position.z));
        Vector3Int cellPos = gridManager.Grid.WorldToCell(pointerPos);

        cellPos.x = Mathf.Clamp(cellPos.x, 0, gridManager.Size - 1);
        cellPos.y = Mathf.Clamp(cellPos.y, 0, gridManager.Size - 1);

        Vector3 centerPos = gridManager.Grid.GetCellCenterWorld(cellPos);
        transform.position = centerPos;
    }

    private void HandleSelectionEffect()
    {
        if(isSelected)
        {
            isSpriteRendererStateModified = true;
            float alpha = (Mathf.Sin(Time.time * 4f) + 1f) / 2f * 0.5f + 0.3f;
            foreach(SpriteRendererState state in spriteRendererStates)
            {
                state.spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            }
        }
        else if(isSpriteRendererStateModified)
        {
            isSpriteRendererStateModified = false;
            foreach(SpriteRendererState state in spriteRendererStates)
            {
                state.spriteRenderer.color = state.defaultColor;
            }
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }
}

class SpriteRendererState
{
    public SpriteRenderer spriteRenderer;
    public Color defaultColor;

    public SpriteRendererState(SpriteRenderer spriteRenderer)
    {
        this.spriteRenderer = spriteRenderer;
        this.defaultColor = spriteRenderer.color;
    }
}