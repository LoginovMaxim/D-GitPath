using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCheck : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if(other.gameObject.name == "car" || other.gameObject.name == "car(Clone)")
        {
            transform.parent.gameObject.GetComponent<SimpleCar>().isNearCar = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "car" || other.gameObject.name == "car(Clone)")
        {
            transform.parent.gameObject.GetComponent<SimpleCar>().isNearCar = false;
        }
    }
}
