using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGetter : MonoBehaviour
{
    [SerializeField] private Tilemap _map;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            var cellPosition = _map.WorldToCell(mousePosition);
            Debug.Log($"Mouse: {mousePosition}, Tile: {cellPosition}");
        }
    }
}
