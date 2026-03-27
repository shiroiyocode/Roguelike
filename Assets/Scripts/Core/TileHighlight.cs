using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHighlight : MonoBehaviour
{
    public static TileHighlight Instance { get; private set; }

    [Header("Highlight Sprites")]
    public GameObject highlightObject;
    public SpriteRenderer highlightRenderer;

    [Header("Colors")]
    public Color walkableColor = new Color(1f, 1f, 0f, 0.4f);
    public Color attackColor = new Color(1f, 0f, 0f, 0.4f);
    public Color blockedColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);

    private Camera mainCamera;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        mainCamera = Camera.main;

        // Make sure highlight object exists
        if (highlightObject == null)
            CreateHighlightObject();
    }

    void CreateHighlightObject()
    {
        highlightObject = new GameObject("TileHighlight");
        highlightRenderer =
            highlightObject.AddComponent<SpriteRenderer>();

        // Create a simple square sprite for highlight
        highlightRenderer.sprite = CreateSquareSprite();
        highlightRenderer.sortingLayerName = "Entities";
        highlightRenderer.sortingOrder = -1;

        // Scale slightly smaller than tile
        highlightObject.transform.localScale =
            new Vector3(0.95f, 0.95f, 1f);
    }

    Sprite CreateSquareSprite()
    {
        // Create a 1x1 white texture
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );
    }

    void Update()
    {
        UpdateHighlight();
    }

    void UpdateHighlight()
    {
        if (mainCamera == null) return;

        // Convert mouse to world position
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(
            Input.mousePosition
        );

        // Snap to grid center
        Vector2 hoveredTile = new Vector2(
            Mathf.Floor(mouseWorld.x) + 0.5f,
            Mathf.Floor(mouseWorld.y) + 0.5f
        );

        // Move highlight to hovered tile
        highlightObject.transform.position = new Vector3(
            hoveredTile.x,
            hoveredTile.y,
            0
        );

        // Check what is on this tile and color accordingly
        Color highlightColor = GetTileColor(hoveredTile);
        highlightRenderer.color = highlightColor;
    }

    Color GetTileColor(Vector2 tilePos)
    {
        // Check for mob
        Collider2D mobHit = Physics2D.OverlapBox(
            tilePos,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Mobs")
        );

        if (mobHit != null)
            return attackColor;

        // Check for wall
        Collider2D wallHit = Physics2D.OverlapBox(
            tilePos,
            Vector2.one * 0.9f,
            0f,
            LayerMask.GetMask("Walls")
        );

        if (wallHit != null)
            return blockedColor;

        // Walkable tile
        return walkableColor;
    }
}