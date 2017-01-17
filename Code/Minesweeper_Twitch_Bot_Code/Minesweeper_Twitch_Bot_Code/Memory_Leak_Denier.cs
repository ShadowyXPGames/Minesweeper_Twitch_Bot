using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Memory_Leak_Denier : MonoBehaviour{

    public Restart_Controller rc;

    public Transform tileHolder;

    public delegate void CleanUpEventHandler();
    public event CleanUpEventHandler OnCleanUp;

    private void OnEnable() {
        rc.OnRestart += CleanUp;
    }

    private void OnDisable() {
        rc.OnRestart -= CleanUp;
    }

    private void CallOnCleanUp() => OnCleanUp?.Invoke();

    private void CleanUp() {
        foreach(Transform child in tileHolder) {
            Destroy(child.gameObject);
        }
        CallOnCleanUp();
    }
}