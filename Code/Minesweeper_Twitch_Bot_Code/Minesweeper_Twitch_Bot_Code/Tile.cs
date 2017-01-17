using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile {
    public IntVector2 position { get; private set; }
    public Tile[] surroundingTiles;
    public int surroundingBombs { get; private set; }
    public bool isBomb;
    public bool isFlagged;
    public bool isRevealed;

    public Tile(IntVector2 position) {;
        this.isRevealed = false;
        this.isFlagged = false;
        this.surroundingTiles = new Tile[8];
        this.position = position;
    }

    public void SetSurroundingTiles(Tile[] surroundingTiles) {
        this.surroundingTiles = surroundingTiles;
    }

    public void FindNumBombsSurrounding() {
        int numberSurroundingBombs = 0;
        for(int i = 0; i < 8; i++) {
            if(surroundingTiles[i] != null) {
                if(surroundingTiles[i].isBomb) {
                    numberSurroundingBombs++;
                }
            }
        }
        surroundingBombs = numberSurroundingBombs;
    }
}