using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Twitch_Bot : MonoBehaviour {

    #region networking vars
    TcpClient tcpClient;
    StreamReader reader;
    StreamWriter writer;
    #endregion

    #region login info
    private string username, password;
    public string chatMessagePrefix { get; private set; }
    #endregion

    int width, height;
    float percentBombs;
    
    public GameManager gameManager;
    public Main_Menu_UI_Controller gameStarter;
    public Restart_Controller restartController;

    public delegate int RevealEventHandler(IntVector2 spot, string player);
    public event RevealEventHandler OnPlayerRevealRequest;
    public event RevealEventHandler OnPlayerFlagRequest;

    public delegate bool PlayerJoinEventHandler(string player);
    public event PlayerJoinEventHandler OnPlayerJoin;

    public delegate void GenerateTilemapEventHandler(int width, int height, float percentBombs);
    public event GenerateTilemapEventHandler OnGenerateTilemap;

    private bool started = false;
    private bool isSinglePlayer;

    private void OnEnable() {
        gameStarter.OnGameBegin += Initialize;
        gameManager.OnKillPlayer += NotifyOfKilledPlayer;
        restartController.OnRestart += Restart;
    }

    private void OnDisable() {
        gameStarter.OnGameBegin -= Initialize;
        gameManager.OnKillPlayer -= NotifyOfKilledPlayer;
        restartController.OnRestart -= Restart;
    }

    private void CallOnGenerateTilemap(int width, int height, float percentBombs) => OnGenerateTilemap?.Invoke(width, height, percentBombs);

    private void Restart() {
        CallOnGenerateTilemap(width, height, percentBombs);
    }

    private int Initialize(string user, string oauth, int _width, int _height, float _percentBombs, bool singlePlayer) {
        width = _width;
        height = _height;
        percentBombs = _percentBombs;
        isSinglePlayer = singlePlayer;
        if(!isSinglePlayer) {
            username = user;
            password = oauth;
            Reconnect();
            if(tcpClient.Connected == false) {
                return 1;
                Debug.Log("could not connect");
            }
            chatMessagePrefix = $":{username}!{username}@{username}.tmi.twitch.tv PRIVMSG #{username} :";
        }
        started = true;
        CallOnGenerateTilemap(width, height, percentBombs);
        return 0;
    }

    private void NotifyOfKilledPlayer(string player) {
        SendChatMessage($"{player} was tragically blown up by a bomb! Pity...");
    }

    private int CallOnPlayerRevealRequest(IntVector2 spot, string player) {
        if(OnPlayerRevealRequest != null) {
            return OnPlayerRevealRequest.Invoke(spot, player);
        } else {
            Debug.Log("No tilemanager listening for onplayerrevealrequest!");
            return 3;
        }
    }

    private int CallOnPlayerFlagRequest(IntVector2 spot, string player) {
        if(OnPlayerFlagRequest != null) {
            return OnPlayerFlagRequest.Invoke(spot, player);
        } else {
            Debug.Log("No tilemanager listening for onplayerflagrequest!");
            return 3;
        }
    }

    private bool CallOnPlayerJoin(string player) {
        if(OnPlayerJoin != null) {
            return OnPlayerJoin.Invoke(player);
        } else {
            Debug.Log("nobody listening for players joining!");
            return false;
        }
    }

    private void Update() {
        if(started) {
            if(!isSinglePlayer) {
                if(!tcpClient.Connected) {
                    Reconnect();
                    Debug.Log("Am not connected... Reconnecting now");
                }

                if(tcpClient.Available > 0) {
                    var message = reader.ReadLine();
                    if(message == "PING :tmi.twitch.tv") {
                        Debug.Log("Responding to ping");
                        writer.WriteLine("PONG: tmi.twitch.tv");
                        writer.Flush();
                    }
                    var iCollon = message.IndexOf(":", 1);
                    if(iCollon > 0) {
                        var command = message.Substring(1, iCollon);
                        if(command.Contains("PRIVMSG")) {
                            var iBang = command.IndexOf("!");
                            if(iBang > 0) {
                                var speaker = command.Substring(0, iBang);
                                var chatMessage = message.Substring(iCollon + 1);

                                RecieveMessage(speaker, chatMessage);
                            }
                        }
                    }
                }
            }
        }
    }

    private void Reconnect() {
        tcpClient = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(tcpClient.GetStream());
        writer = new StreamWriter(tcpClient.GetStream());

        writer.WriteLine(String.Format("PASS {0}\r\nNICK {1}\r\nUser {1} 8 * :{1}", password, username));
        writer.WriteLine("JOIN #" + username);
        writer.Flush();
    }

    void RecieveMessage(string speaker, string chatMessage) {
        const string revealCommand = "!r";
        const string flagCommand = "!f";
        if(chatMessage.StartsWith("!hi")) {
            SendChatMessage($"Hello, {speaker}!");
        }
        #region revealCommand
        if(chatMessage.StartsWith(revealCommand)) {
            string xySpot = chatMessage.Substring(revealCommand.Length, (chatMessage.Length - revealCommand.Length));
            int spaceIndex = xySpot.IndexOf(" ", 1);
            string x;
            string y;
            try {
                x = xySpot.Substring(1, spaceIndex - 1);
                y = xySpot.Substring(spaceIndex + 1, (xySpot.Length - (spaceIndex + 1)));
            } catch (ArgumentOutOfRangeException e) {
                SendChatMessage("Your request was formatted wrong @" + speaker + " to reveal a tile, use command !r x y");
                return;
            }
            int x_Pos;
            try {
                x_Pos = Convert.ToInt32(x);
            } catch(FormatException e) {
                SendChatMessage("That is not a number @" + speaker);
                return;
            }
            int y_Pos;
            try {
                y_Pos = Convert.ToInt32(y);
            } catch(FormatException e) {
                SendChatMessage("That is not a number @" + speaker);
                return;
            }
            int returnMessage = CallOnPlayerRevealRequest(new IntVector2(x_Pos, y_Pos), speaker);
            if(returnMessage == 1) {
                SendChatMessage($"You are dead! @{speaker} wait until the end of the game before joining!");
            } else if (returnMessage == 2) {
                SendChatMessage($"You have not joined yet @{speaker}. Use the command !join to get in the game!");
            } else if (returnMessage == 3) {
                SendChatMessage($"Tile not found, you may have typed a position outside of the grid @{speaker}.");
            } else if (returnMessage == 4) {
                SendChatMessage($"That tile is flagged @{speaker}. Use !f x y to unflag said tile or move on.");
            }
        }
        #endregion
        #region flagCommand
        if(chatMessage.StartsWith(flagCommand)) {
            string xySpot = chatMessage.Substring(flagCommand.Length, (chatMessage.Length - flagCommand.Length));
            int spaceIndex = xySpot.IndexOf(" ", 1);
            string x;
            string y;
            try {
                x = xySpot.Substring(1, spaceIndex - 1);
                y = xySpot.Substring(spaceIndex + 1, (xySpot.Length - (spaceIndex + 1)));
            } catch(ArgumentOutOfRangeException e) {
                SendChatMessage("Your request was formatted wrong @" + speaker + " to flag a tile, use command !f x y");
                return;
            }
            int x_Pos;
            try {
                x_Pos = Convert.ToInt32(x);
            } catch(FormatException e) {
                SendChatMessage("That is not a number @" + speaker);
                return;
            }
            int y_Pos;
            try {
                y_Pos = Convert.ToInt32(y);
            } catch(FormatException e) {
                SendChatMessage("That is not a number @" + speaker);
                return;
            }
            int returnMessage = CallOnPlayerFlagRequest(new IntVector2(x_Pos, y_Pos), speaker);
            if(returnMessage == 1) {
                SendChatMessage($"You are dead! @{speaker} wait until the end of the game before joining!");
            } else if(returnMessage == 2) {
                SendChatMessage($"You have not joined yet @{speaker}. Use the command !join to get in the game!");
            } else if(returnMessage == 3) {
                SendChatMessage($"Tile not found, you may have typed a position outside of the grid @{speaker}.");
            } else if(returnMessage == 4) {
                SendChatMessage($"No tiles have been revealed @{speaker}.");
            } else if(returnMessage == 5) {
                SendChatMessage($"That tile has already been revealed, no flag necesary @{speaker}.");
            }
        }
        #endregion
        #region joinCommand
        if(chatMessage.StartsWith("!join")) {
            if(CallOnPlayerJoin(speaker)) {
                SendChatMessage($"{speaker} has now joined the game! Use \"!r x y\" to reveal a tile!");
            } else {
                SendChatMessage($"You have already joined the game @{speaker} use !r x y to reveal a tile, or if your dead, wait until the end of the round!");
            }
        }
        #endregion
    }

    private void SendChatMessage(string message) {
        if(!isSinglePlayer) {
            writer.WriteLine($"{chatMessagePrefix}{message}");
            writer.Flush();
        }
    }
}
