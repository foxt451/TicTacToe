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

        //EventTrigger.Entry triggerEntry = new EventTrigger.Entry();
        //triggerEntry.eventID = EventTriggerType.PointerClick;
        //triggerEntry.callback.AddListener((BaseEventData data) => OnClick((PointerEventData)data));
        //EventTrigger trigger = fieldTileMap.GetComponent<EventTrigger>();
        //trigger.triggers.Add(triggerEntry);

        // TODO
        // implement clicking detection for tilemap

        Messenger.AddListener(GameEvents.FIELD_UPDATED, RedrawGrid);
        boxCollider = GetComponent<BoxCollider2D>();
        //sddsds
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
        Debug.Log("Clicked!");
        Vector3 clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int pos = fieldTileMap.WorldToCell(clickPoint);
        Vector3Int matrixPos = pos + new Vector3Int(field.Width / 2, field.Height / 2, 0);
        field.MakeMove(matrixPos);
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
    }

    void RedrawFieldTilemap()
    {
        fieldTileMap.ClearAllTiles();
        // iterate through all cells
        for (int i = 0; i < field.Height; i++)
        {
            for (int j = 0; j < field.Width; j++)
            {
                int verticalIndex = i == 0 ? 0 : (i == field.Height - 1 ? 2 : 1);
                int horizontalIndex = j == 0 ? 0 : (j == field.Width - 1 ? 2 : 1);
                Tile tile = tileMatrix[verticalIndex, horizontalIndex];
                int vPos = i - field.Height / 2;
                int hPos = j - field.Width / 2;
                fieldTileMap.SetTile(new Vector3Int(hPos, vPos, 0), tile);
            }
        }
    }

    void RedrawPlayersTilemap()
    {
        playerTileMap.ClearAllTiles();
        var matrix = field.Matrix;
        // iterate through all cells
        for (int i = 0; i < field.Height; i++)
        {
            for (int j = 0; j < field.Width; j++)
            {
                PlayerMark player = matrix[i][j];
                if (player == PlayerMark.Empty)
                {
                    continue; // empty cell
                }
                Tile tile = player == PlayerMark.Player1 ? player1.Representation : player2.Representation;
                Debug.Log(tile);
                int vPos = i - field.Height / 2;
                int hPos = j - field.Width / 2;
                playerTileMap.SetTile(new Vector3Int(hPos, vPos, 0), tile);
            }
        }
    }
}
