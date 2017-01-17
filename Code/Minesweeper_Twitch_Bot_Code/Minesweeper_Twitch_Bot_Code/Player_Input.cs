using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class Player_Input : MonoBehaviour {

    IntVector2 mapSize;

    public TileManager tm;
    public GameManager gm;
    public Twitch_Bot tb;

    private bool gameStarted = false;

    private void OnEnable() {
        tm.OnInitialize += myInit;
        tb.OnGenerateTilemap += GameBegan;
    }

    private void OnDisable() {
        tm.OnInitialize -= myInit;
        tb.OnGenerateTilemap -= GameBegan;
    }

    private void GameBegan(int c, int d, float e) {
        gameStarted = true;
    }

    private void myInit(IntVector2 mapsize) {
        this.mapSize = mapsize;
    }

    private void Update() {
        if(gameStarted) {
            if(Input.GetMouseButtonDown(1)) {
                tm.PlayerFlagRequest(GetScreenPointInIntVector2(Input.mousePosition), "Player");
            }

            if(Input.GetMouseButtonDown(0)) {
                tm.PlayerRevealRequest(GetScreenPointInIntVector2(Input.mousePosition), "Player");
            }
        }
    }

    IntVector2 GetScreenPointInIntVector2(Vector2 screenPoint) {
        Vector2 oldVec = Camera.main.ScreenToWorldPoint(screenPoint);
        IntVector2 newVec = new IntVector2();
        newVec.x = Mathf.RoundToInt(oldVec.x);
        newVec.y = Mathf.RoundToInt(oldVec.y);
        return newVec;
    }
}