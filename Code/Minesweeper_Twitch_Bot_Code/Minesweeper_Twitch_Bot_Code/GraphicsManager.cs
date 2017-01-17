using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

static class GetItWorking {
    public static int HashableInt(this IntVector2 vector) {
        int x = Mathf.RoundToInt(vector.x);
        int y = Mathf.RoundToInt(vector.y);
        return x * 1000 + y * 1000000;
    }
}

class GraphicsManager : MonoBehaviour{

    public TileManager tm;
    public Memory_Leak_Denier mld;

    private IntVector2 gridSize;

    public GameObject tilePrefab;
    public GameObject gridNumberingPrefab;
    public Transform tileHolder;

    public Sprite[] numberSprites;

    public Sprite flagSprite;
    public Sprite bombSprite;
    public Sprite unRevealedSprite;

    private bool firstGame = true;

    private Dictionary<int, SpriteRenderer> graphicsReference = new Dictionary<int, SpriteRenderer>();

    private void OnEnable() {
        tm.OnTileRevealed += RevealTile;
        tm.OnTileFlagged += FlagTile;
        tm.OnInitialize += SetMapSize;
        mld.OnCleanUp += FillGraphicsReference;
    }

    private void OnDisable() {
        tm.OnTileFlagged -= FlagTile;
        tm.OnTileRevealed -= RevealTile;
        tm.OnInitialize -= SetMapSize;
        mld.OnCleanUp -= FillGraphicsReference;
    }

    private void FlagTile(Tile tile) {
        if(graphicsReference.ContainsKey(GetItWorking.HashableInt(tile.position).GetHashCode())) {
            if(tile.isFlagged) {
                graphicsReference[GetItWorking.HashableInt(tile.position).GetHashCode()].sprite = flagSprite;
            } else {
                graphicsReference[GetItWorking.HashableInt(tile.position).GetHashCode()].sprite = unRevealedSprite;
            }
        }
    }

    private void SetMapSize(IntVector2 levelSize) {
        gridSize = levelSize;
        if(firstGame == true) {
            firstGame = false;
            FillGraphicsReference();
        }
    }

    private void RevealTile(Tile tile) {
        if(graphicsReference.ContainsKey(GetItWorking.HashableInt(tile.position).GetHashCode())) {
            if(!tile.isBomb) {
                graphicsReference[GetItWorking.HashableInt(tile.position).GetHashCode()].sprite = numberSprites[tile.surroundingBombs];
            } else {
                graphicsReference[GetItWorking.HashableInt(tile.position).GetHashCode()].sprite = bombSprite;
            }
        }
    }

    private void FillGraphicsReference() {
        graphicsReference = new Dictionary<int, SpriteRenderer>();
        for(int y = 0; y < gridSize.y; y++) {
            if(y == 0) {
                for(int i = 0; i < gridSize.x; i++) {
                    GameObject newNewNewObj = Instantiate(gridNumberingPrefab, new Vector2(i, -1), Quaternion.identity, tileHolder);
                    newNewNewObj.GetComponent<TextMesh>().text = i.ToString();
                }
            }
            for(int x = 0; x < gridSize.x; x++) {
                if(x == 0) {
                    GameObject newNewObj = Instantiate(gridNumberingPrefab, new Vector2(-1, y), Quaternion.identity, tileHolder);
                    newNewObj.GetComponent<TextMesh>().text = y.ToString();
                }
                IntVector2 newVec = new IntVector2(x, y);
                GameObject newObj = Instantiate(tilePrefab, new Vector2(newVec.x, newVec.y), Quaternion.identity, tileHolder);
                graphicsReference.Add(GetItWorking.HashableInt(newVec).GetHashCode(), newObj.GetComponent<SpriteRenderer>());
                graphicsReference[GetItWorking.HashableInt(newVec).GetHashCode()].sprite = unRevealedSprite;
            }
        }
        gridSize = gridSize;
    }
}