using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class IntVector2 {
    public int x;
    public int y;

    public static readonly IntVector2 zero = new IntVector2(0, 0);

    public IntVector2(int _x, int _y) {
        x = _x;
        y = _y;
    }

    public IntVector2() {
        x = 0;
        y = 0;
    }

    public static bool operator ==(IntVector2 first, IntVector2 second) {
        return (first.x == second.x && first.y == second.y);
    }

    public static bool operator !=(IntVector2 first, IntVector2 second) {
        return !(first == second);
    }
}

public class TileManager : MonoBehaviour {

    /// <summary>
    /// must be in integers.
    /// </summary>
    public IntVector2 gridSize;
    Tile[,] grid;
    [Range(0, 1)]
    float percentBombs = 0;

    int numTilesRevealedOrFlagged = 0;
    int numTiles = 0;

    public Twitch_Bot bot;

    public delegate void TileRevealEventHandler(Tile tile);
    public event TileRevealEventHandler OnTileRevealed;
    public event TileRevealEventHandler OnTileFlagged;

    public delegate void GameWonEventHandler();
    public event GameWonEventHandler OnGameWon;

    public delegate void BombFoundEventHandler(string blowUpee);
    public event BombFoundEventHandler OnBombFound;

    public delegate int CheckPlayerAvailailityEventHandler(string player);
    public event CheckPlayerAvailailityEventHandler OnCheckDead;

    public delegate void InitializeEventHandler(IntVector2 gridXY);
    public event InitializeEventHandler OnInitialize;

    bool noReveals = true;

    private void BeginGame(int width, int height, float _percentBombs) {
        noReveals = true;
        numTiles = (width * height);
        numTilesRevealedOrFlagged = 0;
        percentBombs = _percentBombs;
        grid = new Tile[width, height];
        gridSize = new IntVector2(width, height);
        CallOnInitialize(gridSize);
    }

    private void OnEnable() {
        bot.OnPlayerRevealRequest += PlayerRevealRequest;
        bot.OnPlayerFlagRequest += PlayerFlagRequest;
        bot.OnGenerateTilemap += BeginGame;
    }

    private void OnDisable() {
        bot.OnPlayerRevealRequest -= PlayerRevealRequest;
        bot.OnPlayerFlagRequest -= PlayerFlagRequest;
        bot.OnGenerateTilemap -= BeginGame;
    }

    public int PlayerFlagRequest(IntVector2 spot, string speaker) {
        if(noReveals == true) {
            return 4;
        }
        int returnMessage = CallOnCheckDead(speaker);
        if(returnMessage == 0) {
            Tile flagTile = GetTileAtPosition(spot);
            if(flagTile.isRevealed == true) {
                return 5;
            }
            if(flagTile == null) {
                return 3;
            }
            if(!flagTile.isFlagged) {
                flagTile.isFlagged = true;
                if(flagTile.isBomb) {
                    numTilesRevealedOrFlagged++;
                }
            } else {
                flagTile.isFlagged = false;
                if(flagTile.isBomb) {
                    numTilesRevealedOrFlagged--;
                }
            }
            CallOnTileFlagged(flagTile);
        } else if (returnMessage == 1) {
            return 1;
        } else if (returnMessage == 2) {
            return 2;
        }
        return 0;
    }

    

    public int PlayerRevealRequest(IntVector2 spot, string speaker) {
        if(noReveals == true) {
            if(spot.x < 0 || spot.x > gridSize.x - 1 || spot.y < 0 || spot.y > gridSize.y - 1) {
                return 3;
            }
            noReveals = false;
            GenerateGrid(spot, speaker);
            return 0;
        }
        int returnMessage = CallOnCheckDead(speaker);
        if(returnMessage == 0) {
            if(spot.x < 0 || spot.x > gridSize.x - 1 || spot.y < 0 || spot.y > gridSize.y - 1) {
                return 3;
            }
            Tile revealTile = GetTileAtPosition(spot);
            if(revealTile.isFlagged) {
                return 4;
            }
            if(!revealTile.isRevealed) {
                revealTile.isRevealed = true;
                CallOnTileRevealed(revealTile);
                numTilesRevealedOrFlagged++;
                if(revealTile.isBomb) {
                    CallOnBombFound(speaker);
                }
                if(revealTile.surroundingBombs == 0) {
                    Queue<Tile> newQueue = new Queue<Tile>();
                    newQueue.Enqueue(revealTile);
                    while(newQueue.Count > 0) {
                        Tile tile = newQueue.Dequeue();
                        if(tile.isRevealed == false) {
                            tile.isRevealed = true;
                            numTilesRevealedOrFlagged++;
                            CallOnTileRevealed(tile);
                        }
                        if(tile.surroundingBombs == 0 && !tile.isBomb) {
                            for(int i = 0; i < 8; i++) {
                                if(tile.surroundingTiles[i] != null &&
                                    tile.surroundingTiles[i].isRevealed == false &&
                                    !newQueue.Contains(tile.surroundingTiles[i])
                                    ) {
                                    newQueue.Enqueue(tile.surroundingTiles[i]);
                                }
                            }
                        }
                    }
                }
            }
            if(numTilesRevealedOrFlagged == numTiles) {
                Debug.Log("You did it!");
                CallOnGameWon();
            }
            return 0;
        } else if (returnMessage == 1) {
            return 1;
        } else if (returnMessage == 2) {
            return 2;
        } else {
            return 5;
        }
    }

    public void GenerateGrid(IntVector2 beginningSpot, string speaker) {
        //First pass: generates all tiles.
        for(int x = 0; x < gridSize.x; x++) {
            for(int y = 0; y < gridSize.y; y++) {
                Tile newTile = new Tile(new IntVector2(x, y));
                grid[x, y] = newTile;
            }
        }
        //second pass, randomly sets bombs. and sets surrounding tiles.
        for(int x = 0; x < gridSize.x; x++) {
            for(int y = 0; y < gridSize.y; y++) {
                Tile[] surroundingTiles = new Tile[8];
                int index = 0;
                for(int _x = -1; _x <= 1; _x++) {
                    for(int _y = -1; _y <= 1; _y++) {
                        if(!(_x == 0 && _y == 0)) {
                            if(x == 0 && y == 0) {
                                //Debug.Log("SettingSurrounding");
                            }
                            IntVector2 newSpot = new IntVector2(x + _x, y + _y);
                            if(newSpot.x < gridSize.x && newSpot.y < gridSize.y && newSpot.x >= 0 && newSpot.y >= 0) {
                                surroundingTiles[index] = GetTileAtPosition(newSpot);
                            } else {
                                surroundingTiles[index] = null;
                            }
                            index++;
                        }
                    }
                }
                GetTileAtPosition(new IntVector2(x, y)).SetSurroundingTiles(surroundingTiles);
            }
        }
        //set bombs
        int numBombs = Mathf.FloorToInt(numTiles * percentBombs);
        if(numBombs > numTiles) {
            //Debug.Log("oh boy, shit totally hit the fan");
        }
        bool[,] bombFlags = new bool[gridSize.x, gridSize.y];
        int numberOfBombsSofar = 0;
        IntVector2[] invalidPositions = new IntVector2[9];
        for(int d = 0; d < 8; d++) {
            if(GetTileAtPosition(beginningSpot).surroundingTiles[d] != null) {
                invalidPositions[d] = GetTileAtPosition(beginningSpot).surroundingTiles[d].position;
            } else {
                invalidPositions[d] = IntVector2.zero;
            }
        }
        invalidPositions[8] = beginningSpot;
        for(int i = 0; i < numBombs; i++) {
            IntVector2 bombSpot = new IntVector2(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y));
            while(bombFlags[bombSpot.x, bombSpot.y] == true) {
                bombSpot = new IntVector2(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y));
            }
            bombFlags[bombSpot.x, bombSpot.y] = true;
            numberOfBombsSofar++;
            bool valid = true;
            for(int f = 0; f < 9; f++) {
                if(invalidPositions[f] == IntVector2.zero) {
                    continue;
                }
                if(bombSpot == invalidPositions[f]) {
                    numberOfBombsSofar--;
                    bombFlags[bombSpot.x, bombSpot.y] = false;
                    i--;
                    valid = false;
                    break;
                }
            }
            if(valid == false) {
                continue;
            }
            if(ableToPutBombHere(beginningSpot, numberOfBombsSofar, bombFlags)) {
                GetTileAtPosition(bombSpot).isBomb = true;
            } else {
                numberOfBombsSofar--;
                i--;
                bombFlags[bombSpot.x, bombSpot.y] = false;
            }
        }

        for(int x = 0; x < gridSize.x; x++) {
            for(int y = 0; y < gridSize.y; y++) {
                grid[x, y].FindNumBombsSurrounding();
            }
        }
        PlayerRevealRequest(beginningSpot, speaker);
    }

    void CallOnInitialize(IntVector2 levelSize) => OnInitialize?.Invoke(levelSize);
    
    bool ableToPutBombHere(IntVector2 beginningSpot, int numBombsSoFar, bool[,] bombFlags) {
        bool[,] mapflags = new bool[bombFlags.GetLength(0), bombFlags.GetLength(1)];
        Queue<IntVector2> thing = new Queue<IntVector2>();
        thing.Enqueue(beginningSpot);
        int accessableTileCount = 1;
        bombFlags[beginningSpot.x, beginningSpot.y] = true;
        while(thing.Count > 0) {
            IntVector2 pos = thing.Dequeue();
            for(int x = -1; x <= 1; x++) {
                for(int y = -1; y <= 1; y++) {
                    int neighbourX = pos.x + x;
                    int neighbourY = pos.y + y;
                    if(x == 0 || y == 0) {
                        if(neighbourX < bombFlags.GetLength(0) && neighbourX >= 0 && neighbourY >= 0 && neighbourY < bombFlags.GetLength(1)) {
                            if(!bombFlags[neighbourX, neighbourY] && !mapflags[neighbourX, neighbourY]) {
                                mapflags[neighbourX, neighbourY] = true;
                                thing.Enqueue(new IntVector2(neighbourX, neighbourY));
                                accessableTileCount++;
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTileCount = numTiles - numBombsSoFar;
        return targetAccessibleTileCount == accessableTileCount;
    }

    Tile GetTileAtPosition(IntVector2 spot) {
        return grid[spot.x, spot.y];
    }

    private void CallOnGameWon() => OnGameWon?.Invoke();
    private void CallOnTileFlagged(Tile tile) => OnTileFlagged?.Invoke(tile);
    private void CallOnTileRevealed(Tile tile) => OnTileRevealed?.Invoke(tile);
    private void CallOnBombFound(string blowUpee) => OnBombFound?.Invoke(blowUpee);
    private int CallOnCheckDead(string player) {
        if(OnCheckDead != null) {
            return OnCheckDead.Invoke(player);
        } else {
            //Debug.Log("nothing able to check if something is dead!");
            return 3;
        }
    }
}