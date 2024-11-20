using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndWallBehavior : MonoBehaviour {
    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")   //Our player has reached destination. Bring up our menu and level complete stuff
        {

        }
    }
}
