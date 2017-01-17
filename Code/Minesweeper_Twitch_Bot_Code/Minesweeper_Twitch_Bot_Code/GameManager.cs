using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public Twitch_Bot bot;
    public TileManager tileManager;
    public Restart_Controller restartController;

    Dictionary<string, bool> players;

    public GameObject text;

    public delegate void KillPlayerEventHandler(string player);
    public event KillPlayerEventHandler OnKillPlayer;

    private void CallOnKillPlayer(string player) => OnKillPlayer?.Invoke(player);

    private bool oneJoined = false;

    private void OnEnable() {
        tileManager.OnCheckDead += CheckIfPlayerIsDead;
        tileManager.OnBombFound += KillPlayer;
        tileManager.OnGameWon += WonGame;
        bot.OnPlayerJoin += AddPlayer;
        restartController.OnRestart += Restart;
    }

    private void OnDisable() {
        tileManager.OnCheckDead -= CheckIfPlayerIsDead;
        tileManager.OnBombFound -= KillPlayer;
        tileManager.OnGameWon -= WonGame;
        bot.OnPlayerJoin -= AddPlayer;
        restartController.OnRestart -= Restart;
    }

    private void Restart() {
        oneJoined = false;
        AddPlayer("Player");
    }

    private void Start() {
        text.SetActive(false);
        AddPlayer("Player");
    }
    
    private void WonGame() {
        text.SetActive(true);
    }

    private int CheckIfPlayerIsDead(string player) {
        if(players.ContainsKey(player)) {
            if(players[player]) {
                return 1;
            } else {
                return 0;
            }
        } else {
            return 2;
        }
    }

    private void KillPlayer(string player) {
        CallOnKillPlayer(player);
        players[player] = true;
    }

    public bool AddPlayer(string player) {
        if(oneJoined == false) {
            oneJoined = true;
            players = new Dictionary<string, bool>();
        }
        if(!players.ContainsKey(player)) {
            players.Add(player, false);
            return true;
        } else {
            return false;
        }
    }
}