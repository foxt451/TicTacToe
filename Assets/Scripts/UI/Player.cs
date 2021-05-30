using UnityEngine;
using UnityEngine.Tilemaps;

// graphical representaion of a player
public class Player : MonoBehaviour
{
    public string Name { get; set; }

    [SerializeField]
    private Tile representation;

    [SerializeField]
    private Tile hightlightedRepresentation;
    public Tile Representation { get => representation; }
    public Tile Highlighted { get => hightlightedRepresentation; }
}
