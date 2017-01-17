using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Main_Menu_UI_Controller : MonoBehaviour {

    //Login info fields
    public InputField userField;
    public InputField oauthField;

    //Board specs fields
    public InputField widthField;
    public InputField heightField;
    public InputField percentBombsField;

    public Text errorText;

    public GameObject mainMenu;
    public GameObject gameBeganStuff;

    public delegate int GameBeginEventHandler(string username, string oauth, int width, int height, float percentBombs, bool singlePlayer);
    public event GameBeginEventHandler OnGameBegin;

    private int CallOnGameBegin(string username, string oauth, int width, int height, float percentBombs, bool singlePlayer) {
        if(OnGameBegin != null) {
            return OnGameBegin.Invoke(username, oauth, width, height, percentBombs, singlePlayer);
        } else {
            errorText.text += "No twitch_bot listening for OnGameBegin\r\n";
            return 0;
        }
    }

    public void BeginGame(bool singlePlayer) {
        errorText.text = "";

        int width;
        int height;
        float percentBombs;

        if(!singlePlayer) {
            #region Error Checking
            if(userField.text.Length == 0) {
                errorText.text += "Username not given!\r\n";
                return;
            }
            if(oauthField.text.Length == 0) {
                errorText.text += "Oauth not given!\r\n";
                return;
            } else if(!oauthField.text.StartsWith("oauth:")) {
                errorText.text += "Oauth needs to start with oauth:\r\n";
                return;
            }
        }
        if(widthField.text.Length == 0) {
            errorText.text += "Need a width, suggested is 50\r\n";
            return;
        } else if(!Int32.TryParse(widthField.text, out width)) {
            errorText.text += "The width is either not a number, or is not an integer. Get rid of any . or try again!\r\n";
            return;
        } else if(width < 0) {
            errorText.text += "The width is less than zero, try again\r\n";
            return;
        }
        if(heightField.text.Length == 0) {
            errorText.text += "Need a height, suggested is 25\r\n";
            return;
        } else if(!Int32.TryParse(heightField.text, out height)) {
            errorText.text += "The height is either not a number, or is not an integer. Get rid of any . or try again!\r\n";
            return;
        } else if(height < 0) {
            errorText.text += "The height is less than zero, try again\r\n";
            return;
        }
        if(percentBombsField.text.Length == 0) {
            errorText.text += "Need a percentage of bombs, suggested is 0.20\r\n";
            return;
        } else if(!float.TryParse(percentBombsField.text, out percentBombs)) {
            errorText.text += "The percentage bombs is not written as a number.\r\n";
            return;
        } else if(percentBombs < 0) {
            errorText.text += "The percentage of bombs is less than zero, try again\r\n";
            return;
        } else if(percentBombs > 1) {
            errorText.text += "The percentage of bombs is greater than 1, try again\r\n";
            return;
        }
        #endregion
        //failed to connect
        if(CallOnGameBegin(userField.text.ToLower(), oauthField.text.ToLower(), width, height, percentBombs, singlePlayer) == 1) {
            errorText.text += "Could not connect to that account, make sure your user and oauth are correct!";
        } else {
            mainMenu.SetActive(false);
            gameBeganStuff.SetActive(true);
        }
    }
}