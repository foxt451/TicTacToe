using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class FieldGrid : MonoBehaviour
{
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
    

    private Tile[,] tileMatrix;

    [SerializeField]
    private Field field;

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

    //private Vector3 GridOffsetVector { get => new Vector3(Mathf.RoundToInt(field.offset.x + fieldTileMap.tileAnchor.x) / 2 *
    //    fieldTileMap.cellSize.x,
    //    Mathf.RoundToInt(field.offset.y + fieldTileMap.tileAnchor.y) / 2 * fieldTileMap.cellSize.y, 0); }
    //// previous to calculate delta
    //private Vector3 PreviousGridOffsetVector = new Vector3(0, 0, 0);

    void Awake()
    {
        tileMatrix = new Tile[,] { { lowerLeftCell, lowerCenterCell, lowerRightCell },
            { centerLeftCell, centerCenterCell, centerRightCell }, { upperLeftCell, upperCenterCell, upperRightCell} };

        Messenger.AddListener(GameEvents.FIELD_UPDATED, RedrawGrid);
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {

    }

    void OnDestroy()
    {
        Messenger.RemoveListener(GameEvents.FIELD_UPDATED, RedrawGrid);
    }

    public void OnMouseDown()
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
        Debug.Log("Cell " + pos);

        // matrix position are passed not adjusted to expansion
        // so that they hold even after Expand
        // they are adjusted in field Move method
        Vector3Int stableMatrixPos = pos;
        Debug.Log("Stable " + stableMatrixPos);
        gameController.MoveWithPlayer(new Vector2Int(stableMatrixPos.x, stableMatrixPos.y));
    }

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
