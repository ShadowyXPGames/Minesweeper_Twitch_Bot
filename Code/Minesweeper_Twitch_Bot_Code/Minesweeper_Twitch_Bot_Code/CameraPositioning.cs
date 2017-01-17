using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class CameraPositioning : MonoBehaviour {

    public TileManager tm;

    private Camera myCam;

    private bool alreadyPositioned;

    private void OnEnable() {
        tm.OnInitialize += PositionCamera;
    }

    private void Start() {
        myCam = this.GetComponent<Camera>();
    }

    private void OnDisable() {
        tm.OnInitialize -= PositionCamera;
    }

    private void PositionCamera(IntVector2 gridXY) {
        if(alreadyPositioned == false) {
            alreadyPositioned = true;
            Vector3 position;
            position.x = (gridXY.x / (float)2) - .5f;
            position.y = (gridXY.y / (float)2) - .5f;
            position.z = -10f;
            this.transform.position = position;
            const float cameraViewSizeWiggleRoom = 0.8f;
            if(gridXY.x > gridXY.y) {
                const float arbitraryIncrease = .15f;
                while(myCam.WorldToViewportPoint(new Vector3(gridXY.x, 0, 0)).x > 1 || myCam.WorldToViewportPoint(new Vector3(gridXY.x, 0, 0)).x < 0) {
                    myCam.orthographicSize += arbitraryIncrease;
                }
                myCam.orthographicSize += cameraViewSizeWiggleRoom;
            } else {
                myCam.orthographicSize = gridXY.y / 2f + cameraViewSizeWiggleRoom;
            }
        }
    }
}