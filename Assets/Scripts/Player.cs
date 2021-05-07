using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    public string Name { get; set; }

    [SerializeField]
    private Tile representation;
    public Tile Representation { get => representation; }
}
