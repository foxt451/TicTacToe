using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

// graphical representation of the field
public class FieldGrid : MonoBehaviour
{
    // sprites for possible cell outlines
    [SerializeField]
    private Tile upperLeftCell;
    [SerializeField]
    private Tile upperCenterCell;
    [SerializeField]
    private Tile upperRightCell;

    [SerializeField]
    private Tile centerLeftCell;
    [SerializeField]
    private Tile centerCenterCell;
    [SerializeField]
    private Tile centerRightCell;
    
    [SerializeField]
    private Tile lowerLeftCell;
    [SerializeField]
    private Tile lowerCenterCell;
    [SerializeField]
    private Tile lowerRightCell;
    
    // matrix of the above tiles
    private Tile[,] tileMatrix;

    // field object we are going to draw
    [SerializeField]
    private Field field;

    // gameController to tell it to move on a cell click
    [SerializeField]
    private GameController gameController;

    [SerializeField]
    // tilemap to draw the field (without players)
    private Tilemap fieldTileMap;

    [SerializeField]
    // tilemap to draw the player moves
    private Tilemap playerTileMap;
    [SerializeField]
    private Player player1;
    [SerializeField]
    private Player player2;

    private BoxCollider2D boxCollider;

    void Awake()
    {
        tileMatrix = new Tile[,] { { lowerLeftCell, lowerCenterCell, lowerRightCell },
            { centerLeftCell, centerCenterCell, centerRightCell }, { upperLeftCell, upperCenterCell, upperRightCell} };

        Messenger.AddListener(GameEvents.FIELD_UPDATED, RedrawGrid);
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void OnDestroy()
    {
        Messenger.RemoveListener(GameEvents.FIELD_UPDATED, RedrawGrid);
    }

    // when the mouse is pressed, we may move into the pressed cell
    void OnMouseDown()
    {
        if (GameController.controller.GameState != GameState.INGAME)
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector3 clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int pos = fieldTileMap.WorldToCell(clickPoint);

        Vector3Int stableMatrixPos = pos;
        gameController.MoveWithPlayer(new Vector2Int(stableMatrixPos.x, stableMatrixPos.y));
    }

    // totally updates the grid
    void RedrawGrid()
    {
        ResizeCollider();
        RedrawFieldTilemap();
        RedrawPlayersTilemap();
    }

    // resize collider, so that the clicks are getting registered
    void ResizeCollider()
    {
        float newWidth = Mathf.Ceil(field.Width * fieldTileMap.cellSize.x / 2) * 2;
        float newHeight = Mathf.Ceil(field.Height * fieldTileMap.cellSize.y / 2) * 2;
        boxCollider.size = new Vector2(newWidth, newHeight);

        Vector2 offset = new Vector2(
            (boxCollider.size.x / 2 - field.totalIncrease.xLeft - field.initialSize.width / 2) * fieldTileMap.cellSize.x,
            (boxCollider.size.y / 2 - field.totalIncrease.yBot - field.initialSize.height / 2) * fieldTileMap.cellSize.y
            );
        boxCollider.offset = offset;
    }

    // redraws tilemap with cell outlines
    void RedrawFieldTilemap()
    {
        fieldTileMap.ClearAllTiles();

        // iterate through all cells
        var stableBounds = field.GetStableBounds();
        for (int i = stableBounds.yBot; i <= stableBounds.yTop; i++)
        {
            for (int j = stableBounds.xLeft; j <= stableBounds.xRight; j++)
            {
                int verticalIndex = i == stableBounds.yBot ? 0 : (i == stableBounds.yTop ? 2 : 1);
                int horizontalIndex = j == stableBounds.xLeft ? 0 : (j == stableBounds.xRight ? 2 : 1);
                Tile tile = tileMatrix[verticalIndex, horizontalIndex];
                fieldTileMap.SetTile(new Vector3Int(j, i, 0), tile);
            }
        }
    }

    // redraws the tilemap with player marks
    void RedrawPlayersTilemap()
    {
        playerTileMap.ClearAllTiles();

        // iterate through all cells
        var stableBounds = field.GetStableBounds();
        for (int i = stableBounds.yBot; i <= stableBounds.yTop; i++)
        {
            for (int j = stableBounds.xLeft; j <= stableBounds.xRight; j++)
            {
                PlayerMark player = field.GetPlayerAtCell(j, i);
                if (player == PlayerMark.Empty)
                {
                    continue; // empty cell
                }
                Tile tile;
                if (field.stableLastMove == (j, i))
                {
                    tile = player == PlayerMark.Player1 ? player1.Highlighted : player2.Highlighted;
                }
                else
                {
                    tile = player == PlayerMark.Player1 ? player1.Representation : player2.Representation;
                }
                
                playerTileMap.SetTile(new Vector3Int(j, i, 0), tile);
            }
        }
    }
}
