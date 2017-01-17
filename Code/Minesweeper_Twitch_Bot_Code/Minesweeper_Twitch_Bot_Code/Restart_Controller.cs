using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Restart_Controller : MonoBehaviour {

    string username;
    string password;

    public delegate void RestartEventHandler();
    public event RestartEventHandler OnRestart;

    private void CallOnRestart() => OnRestart?.Invoke();

    public void Restart() {
        CallOnRestart();
    }
}