using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCar : MonoBehaviour
{
    private Transform transform;
    private RoadGenerator RoadGen;
    public GraphNode currentNode;
    public GraphNode targetNode;

    private Vector3 target;
    private Vector3 oldTarget;
    private Vector3 antiTarget;
    private Vector3 beforePosition;

    Vector3 beforeDirection;
    Vector3 afterDirection;

    private Vector3 offsetCenterNode;

    public float speed;
    private bool isMove;
    private bool isChoose;
    private bool isOnCurve;
    private int currentIndexNode;
    private int numberNieghbor;

    private int firstTouch;
    private int seed;
    GameObject pointTarget;

    public bool isNearCar;
    private bool isSlowMotion;

    Vector3 firstStep;
    Vector3 secondStep;
    void Start()
    {
        firstTouch = 1;
        transform = gameObject.transform;
        RoadGen = GameObject.Find("RoadGenerator").GetComponent<RoadGenerator>();

        isMove = false;
        isChoose = true;
        isOnCurve = false;
        isNearCar = false;
        isSlowMotion = false;

        numberNieghbor = 0;
        targetNode = currentNode.neighbors[numberNieghbor];
        transform.position += GetRightOffset(currentNode.roadsToNeighbors[numberNieghbor], 2);
        target = currentNode.roadsToNeighbors[numberNieghbor];
        seed = 1;

        antiTarget = Vector3.zero;
        RotateCar(target);
    }

    void Update()
    {

        if (isChoose == true)
        {
            if(isNearCar) 
                antiTarget = target.normalized * -1.0f;
            else
                antiTarget = Vector3.zero;
            
            if (targetNode.isGreenTrafficLight == true)
                transform.Translate((target.normalized + antiTarget) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
            else
                transform.Translate((target.normalized + antiTarget) * Time.deltaTime * RoadGen.speedSimpleCars * (Vector3.Distance(transform.position, targetNode.nodePosition) - 12) / target.magnitude, Space.World);
        }

        if(isChoose == false && isOnCurve == true)
        {
            transform.position = Vector3.MoveTowards(firstStep, secondStep, 0.5f * Time.deltaTime);
        }

        //transform.position = new Vector3(transform.position.x, 2f, transform.position.z);

        Debug.DrawLine(transform.position, currentNode.nodePosition, Color.blue, Time.deltaTime, true);
        Debug.DrawLine(transform.position, targetNode.nodePosition, Color.yellow, Time.deltaTime, true);

        Debug.DrawLine(currentNode.nodePosition, targetNode.nodePosition, Color.red, Time.deltaTime, true);
    }

    private void RotateCar(Vector3 targetV)
    {
        if (targetV.x == 0 && targetV.z > 0)
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        else if (targetV.x == 0 && targetV.z < 0)
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        else if (targetV.x > 0 && targetV.z == 0)
            transform.eulerAngles = new Vector3(0f, 90f, 0f);
        else if (targetV.x < 0 && targetV.z == 0)
            transform.eulerAngles = new Vector3(0f, 270f, 0f);
    }

    private void OnTriggerStay(Collider other)
    {
        string name = other.gameObject.name;

        if (isOnCurve == false)
        {
            oldTarget = target;
            //Debug.Log(other.gameObject.name);

            if (targetNode.neighbors.Count == 2)
            {
                beforePosition = other.gameObject.transform.position;

                if (name == "point1")
                {
                    GameObject parent = other.gameObject.transform.parent.gameObject;
                    isOnCurve = true;
                    isChoose = false;
                    ChooseOtherPath();

                    pointTarget = parent.transform.GetChild(1).gameObject;
                    target = pointTarget.transform.position - beforePosition;

                    offsetCenterNode = parent.transform.position + GetRightOffset(target, 7.5f) + new Vector3(0f, 2f, 0f);

                    StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                }
                else if (name == "point3")
                {
                    GameObject parent = other.gameObject.transform.parent.gameObject;
                    isOnCurve = true;
                    isChoose = false;
                    ChooseOtherPath();

                    pointTarget = parent.transform.GetChild(3).gameObject;
                    target = pointTarget.transform.position - beforePosition;

                    offsetCenterNode = parent.transform.position + GetRightOffset(target, 1.5f) + new Vector3(0f, 2f, 0f);

                    StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                }

                return;
            }
            else if (targetNode.neighbors.Count == 3)
            {
                beforePosition = other.gameObject.transform.position;

                if (name == "point1")
                {
                    GameObject parent = other.gameObject.transform.parent.gameObject;
                    isChoose = false;
                    if (targetNode.isGreenTrafficLight == true)
                    {
                        isOnCurve = true;
                        beforeDirection = currentNode.roadsToNeighbors[numberNieghbor];
                        ChooseOtherPath();
                        afterDirection = currentNode.roadsToNeighbors[numberNieghbor];

                        if (Vector3.Dot(beforeDirection, afterDirection) != 0)
                        {
                            pointTarget = parent.transform.GetChild(1).gameObject;
                            target = pointTarget.transform.position - beforePosition;

                            //offsetCenterNode = parent.transform.transform.position + GetRightOffset(target, 1.5f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], true, RoadGen.speedSimpleCars));
                        }
                        else
                        {
                            pointTarget = parent.transform.GetChild(3).gameObject;
                            target = pointTarget.transform.position - beforePosition;

                            offsetCenterNode = parent.transform.position + GetRightOffset(target, 7.5f) + new Vector3(0f, 2f, 0f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                        }

                    }
                    else
                    {
                        transform.Translate((-target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                        transform.Translate((target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                    }
                    return;

                }
                else if (name == "point3")
                {
                    GameObject parent = other.gameObject.transform.parent.gameObject;
                    isChoose = false;
                    if (targetNode.isGreenTrafficLight == true)
                    {
                        isOnCurve = true;
                        beforeDirection = currentNode.roadsToNeighbors[numberNieghbor];
                        ChooseOtherPath();
                        afterDirection = currentNode.roadsToNeighbors[numberNieghbor];

                        if (Vector3.Dot(beforeDirection, afterDirection) != 0)
                        {
                            pointTarget = parent.transform.GetChild(5).gameObject;
                            target = pointTarget.transform.position - beforePosition;

                            //offsetCenterNode = parent.transform.transform.position + GetRightOffset(target, 1.5f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], true, RoadGen.speedSimpleCars));
                        }
                        else
                        {
                            pointTarget = parent.transform.GetChild(3).gameObject;
                            target = pointTarget.transform.position - beforePosition;

                            offsetCenterNode = parent.transform.position + GetRightOffset(target, 1.5f) + new Vector3(0f, 2f, 0f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                        }

                    }
                    else
                    {
                        transform.Translate((-target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                        transform.Translate((target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                    }
                    return;
                }
                else if (name == "point5")
                {
                    GameObject parent = other.gameObject.transform.parent.gameObject;
                    isChoose = false;

                    
                    if (targetNode.isGreenTrafficLight == false)
                    {

                        isOnCurve = true;
                        beforeDirection = currentNode.roadsToNeighbors[numberNieghbor];
                        ChooseOtherPath();
                        afterDirection = currentNode.roadsToNeighbors[numberNieghbor];

                        GameObject roadTypeT = parent.transform.gameObject;

                        bool isRightTurn = false;

                        Vector3 directionTurn = beforeDirection + afterDirection;

                        if (Mathf.RoundToInt(roadTypeT.transform.eulerAngles.y) == 0)
                        {
                            if (directionTurn.x < 0 && directionTurn.z < 0) { isRightTurn = true; }
                            else if (directionTurn.x > 0 && directionTurn.z < 0) { isRightTurn = false; }
                        }
                        else if (Mathf.RoundToInt(roadTypeT.transform.eulerAngles.y) == 90)
                        {
                            if (directionTurn.x < 0 && directionTurn.z > 0) { isRightTurn = true; }
                            else if (directionTurn.x < 0 && directionTurn.z < 0) { isRightTurn = false; }
                        }
                        else if (Mathf.RoundToInt(roadTypeT.transform.eulerAngles.y) == 180)
                        {
                            if (directionTurn.x > 0 && directionTurn.z > 0) { isRightTurn = true; }
                            else if (directionTurn.x < 0 && directionTurn.z > 0) { isRightTurn = false; }
                        }
                        else if (Mathf.RoundToInt(roadTypeT.transform.eulerAngles.y) == 270)
                        {
                            if (directionTurn.x > 0 && directionTurn.z < 0) { isRightTurn = true; }
                            else if (directionTurn.x > 0 && directionTurn.z > 0) { isRightTurn = false; }
                        }

                        if (isRightTurn == true)
                        {
                            pointTarget = parent.transform.GetChild(5).gameObject;
                            target = pointTarget.transform.position - beforePosition;

                            offsetCenterNode = parent.transform.position + GetRightOffset(target, 1.5f) + new Vector3(0f, 2f, 0f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                        }
                        else
                        {
                            pointTarget = parent.transform.GetChild(1).gameObject;
                            target = pointTarget.transform.position - beforePosition;

                            offsetCenterNode = parent.transform.position + GetRightOffset(target, 7.5f) + new Vector3(0f, 2f, 0f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                        }

                    }
                    else
                    {
                        transform.Translate((-target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                        transform.Translate((target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                    }
                    return;
                }
            }
            else if (targetNode.neighbors.Count == 4)
            {
                //Debug.Log(targetNode.neighbors.Count);

                if (name == "point1")
                {
                    isChoose = false;
                    if (targetNode.isGreenTrafficLight == true)
                    {
                        beforePosition = other.gameObject.transform.position;
                        GameObject parent = other.gameObject.transform.parent.gameObject;
                        isOnCurve = true;

                        beforeDirection = currentNode.roadsToNeighbors[numberNieghbor];
                        ChooseOtherPath();
                        afterDirection = currentNode.roadsToNeighbors[numberNieghbor];

                        if (Vector3.Dot(beforeDirection, afterDirection) != 0)
                        {
                            pointTarget = parent.transform.GetChild(3).gameObject;
                            target = pointTarget.transform.position - beforePosition;

                            //offsetCenterNode = parent.transform.transform.position + GetRightOffset(target, 1.5f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], true, RoadGen.speedSimpleCars));

                            return;
                        }
                        else
                        {
                            GameObject roadTypeT = parent.transform.gameObject;

                            bool isRightTurn = false;

                            Vector3 directionTurn = beforeDirection + afterDirection;

                            if (directionTurn.x > 0 && directionTurn.z > 0) { isRightTurn = false; }
                            else if (directionTurn.x > 0 && directionTurn.z < 0) { isRightTurn = true; }

                            if (isRightTurn == true)
                            {
                                pointTarget = parent.transform.GetChild(1).gameObject;
                                target = pointTarget.transform.position - beforePosition;

                                offsetCenterNode = parent.transform.position + GetRightOffset(target, 1.5f) + new Vector3(0f, 2f, 0f);

                                StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                            }
                            else
                            {
                                pointTarget = parent.transform.GetChild(5).gameObject;
                                target = pointTarget.transform.position - beforePosition;

                                offsetCenterNode = parent.transform.position + GetRightOffset(target, 7.5f) + new Vector3(0f, 2f, 0f);

                                StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                            }
                        }
                    }
                    else
                    {
                        transform.Translate((-target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                        transform.Translate((target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                    }
                    return;
                    
                }
                else if (name == "point3")
                {
                    isChoose = false;
                    if (targetNode.isGreenTrafficLight == false)
                    {
                        GameObject parent = other.gameObject.transform.parent.gameObject;
                        isOnCurve = true;
                        beforeDirection = currentNode.roadsToNeighbors[numberNieghbor];
                        ChooseOtherPath();
                        afterDirection = currentNode.roadsToNeighbors[numberNieghbor];

                        if (Vector3.Dot(beforeDirection, afterDirection) != 0)
                        {
                            pointTarget = parent.transform.GetChild(5).gameObject;
                            target = pointTarget.transform.position - other.gameObject.transform.position;

                            //offsetCenterNode = parent.transform.transform.position + GetRightOffset(target, 1.5f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], true, RoadGen.speedSimpleCars));

                            return;
                        }
                        else
                        {
                            GameObject roadTypeT = parent.transform.gameObject;

                            bool isRightTurn = false;

                            Vector3 directionTurn = beforeDirection + afterDirection;

                            if (directionTurn.x > 0 && directionTurn.z > 0) { isRightTurn = true; }
                            else if (directionTurn.x < 0 && directionTurn.z > 0) { isRightTurn = false; }

                            if (isRightTurn == true)
                            {
                                pointTarget = parent.transform.GetChild(3).gameObject;
                                target = pointTarget.transform.position - beforePosition;

                                offsetCenterNode = parent.transform.position + GetRightOffset(target, 1.5f) + new Vector3(0f, 2f, 0f);

                                StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                            }
                            else
                            {
                                pointTarget = parent.transform.GetChild(7).gameObject;
                                target = pointTarget.transform.position - beforePosition;

                                offsetCenterNode = parent.transform.position + GetRightOffset(target, 7.5f) + new Vector3(0f, 2f, 0f);

                                StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                            }

                        }
                    }
                    else
                    {
                        transform.Translate((-target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                        transform.Translate((target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                    }
                    return;
                }
                
                else if (name == "point5")
                {
                    isChoose = false;
                    if (targetNode.isGreenTrafficLight == true)
                    {
                        GameObject parent = other.gameObject.transform.parent.gameObject;
                        isOnCurve = true;

                        beforeDirection = currentNode.roadsToNeighbors[numberNieghbor];
                        ChooseOtherPath();
                        afterDirection = currentNode.roadsToNeighbors[numberNieghbor];

                        if (Vector3.Dot(beforeDirection, afterDirection) != 0)
                        {
                            pointTarget = parent.transform.GetChild(7).gameObject;
                            target = pointTarget.transform.position - other.gameObject.transform.position;

                            //offsetCenterNode = parent.transform.transform.position + GetRightOffset(target, 1.5f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], true, RoadGen.speedSimpleCars));

                            return;
                        }
                        else
                        {
                            GameObject roadTypeT = parent.transform.gameObject;

                            bool isRightTurn = false;

                            Vector3 directionTurn = beforeDirection + afterDirection;

                            if (directionTurn.x < 0 && directionTurn.z > 0) { isRightTurn = true; }
                            else if (directionTurn.x < 0 && directionTurn.z < 0) { isRightTurn = false; }

                            if (isRightTurn == true)
                            {
                                pointTarget = parent.transform.GetChild(5).gameObject;
                                target = pointTarget.transform.position - beforePosition;

                                offsetCenterNode = parent.transform.position + GetRightOffset(target, 1.5f) + new Vector3(0f, 2f, 0f);

                                StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                            }
                            else
                            {
                                pointTarget = parent.transform.GetChild(1).gameObject;
                                target = pointTarget.transform.position - beforePosition;

                                offsetCenterNode = parent.transform.position + GetRightOffset(target, 7.5f) + new Vector3(0f, 2f, 0f);

                                StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                            }
                        }
                    }
                    else
                    {
                        transform.Translate((-target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                        transform.Translate((target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                    }
                    return;
                }
                
                else if (name == "point7")
                {
                    isChoose = false;
                    if (targetNode.isGreenTrafficLight == false)
                    {
                        GameObject parent = other.gameObject.transform.parent.gameObject;
                        isOnCurve = true;
                        beforeDirection = currentNode.roadsToNeighbors[numberNieghbor];
                        ChooseOtherPath();
                        afterDirection = currentNode.roadsToNeighbors[numberNieghbor];

                        if (Vector3.Dot(beforeDirection, afterDirection) != 0)
                        {
                            pointTarget = parent.transform.GetChild(1).gameObject;
                            target = pointTarget.transform.position - other.gameObject.transform.position;

                            //offsetCenterNode = parent.transform.transform.position + GetRightOffset(target, 1.5f);

                            StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], true, RoadGen.speedSimpleCars));

                            return;
                        }
                        else
                        {
                            GameObject roadTypeT = parent.transform.gameObject;

                            bool isRightTurn = false;

                            Vector3 directionTurn = beforeDirection + afterDirection;

                            if (directionTurn.x < 0 && directionTurn.z < 0) { isRightTurn = true; }
                            else if (directionTurn.x > 0 && directionTurn.z < 0) { isRightTurn = false; }

                            if (isRightTurn == true)
                            {
                                pointTarget = parent.transform.GetChild(7).gameObject;
                                target = pointTarget.transform.position - beforePosition;

                                offsetCenterNode = parent.transform.position + GetRightOffset(target, 1.5f) + new Vector3(0f, 2f, 0f);

                                StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                            }
                            else
                            {
                                pointTarget = parent.transform.GetChild(3).gameObject;
                                target = pointTarget.transform.position - beforePosition;

                                offsetCenterNode = parent.transform.position + GetRightOffset(target, 7.5f) + new Vector3(0f, 2f, 0f);

                                StartCoroutine(WaitForMovementCurve(currentNode.roadsToNeighbors[numberNieghbor], false, RoadGen.speedSimpleCars));
                            }

                        }
                    }
                    else
                    {
                        transform.Translate((-target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                        transform.Translate((target.normalized * 1f) * Time.deltaTime * RoadGen.speedSimpleCars, Space.World);
                    }
                     return;
                }
                
            }
        }
    }

    private void ChooseOtherPath()
    {
        currentIndexNode = currentNode.nodeIndex;


        currentNode = targetNode;

        if (currentNode.neighbors.Count == 1)
        {
            targetNode = currentNode.neighbors[0];
        }
        else if (currentNode.neighbors.Count == 2)
        {
            numberNieghbor = 0;
            if (currentNode.neighbors[numberNieghbor].nodeIndex == currentIndexNode)
            {
                numberNieghbor = 1;
                targetNode = currentNode.neighbors[numberNieghbor];
            }
            else
            {
                targetNode = currentNode.neighbors[numberNieghbor];
            }
        }
        else
        {
            while (true)
            {
                seed++;
                Random.InitState(seed);
                numberNieghbor = Random.Range(0, currentNode.neighbors.Count);
                targetNode = currentNode.neighbors[numberNieghbor];

                if (targetNode.nodeIndex != currentIndexNode) { break; }
            }
        }

        //
        //Debug.Log("  index(to) " + currentNode.neighbors[numberNieghbor].nodeIndex);
    }

    private IEnumerator WaitForMovementCurve(Vector3 roadToNeighbor, bool isLinear, float countSteps)
    {
        countSteps *= 2;

        float currentRot = transform.eulerAngles.y;
        float rotationAngle = 0;
        
        
        if (oldTarget.x == 0 && oldTarget.z > 0)
        {
            if (roadToNeighbor.x > 0)
            {
                rotationAngle = 90f;
            }
            if (roadToNeighbor.x == 0)
            {
                rotationAngle = 0f;
            }
            if (roadToNeighbor.x < 0)
            {
                rotationAngle = -90f;
            }
        }
        else if(oldTarget.x == 0 && oldTarget.z < 0)
        {

            if (roadToNeighbor.x > 0)
            {
                rotationAngle = -90f;
            }
            if (roadToNeighbor.x == 0)
            {
                rotationAngle = 0f;
            }
            if (roadToNeighbor.x < 0)
            {
                rotationAngle = 90f;
            }
        }
        else if (oldTarget.x > 0 && oldTarget.z == 0)
        {

            if (roadToNeighbor.z > 0)
            {
                rotationAngle = -90f;
            }
            if (roadToNeighbor.z == 0)
            {
                rotationAngle = 0f;
            }
            if (roadToNeighbor.z < 0)
            {
                rotationAngle = 90f;
            }
        }
        else if (oldTarget.x < 0 && oldTarget.z == 0)
        {

            if (roadToNeighbor.z > 0)
            {
                rotationAngle = 90f;
            }
            if (roadToNeighbor.z == 0)
            {
                rotationAngle = 0f;
            }
            if (roadToNeighbor.z < 0)
            {
                rotationAngle = -90f;
            }
        }

        isChoose = false;

        if (isLinear == true)
        {
            for (int i = 0; i < countSteps; i++)
            {
                firstStep = Vector3.Lerp(beforePosition, pointTarget.transform.position, (1f / countSteps) * i);
                secondStep = Vector3.Lerp(beforePosition, pointTarget.transform.position, (1f / countSteps) * (i + 1));

                transform.eulerAngles = new Vector3(0f, Mathf.Lerp(currentRot, currentRot + rotationAngle, (1f / countSteps) * i), 0f);
                
                yield return new WaitForEndOfFrame();
                //yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            for (int i = 0; i < countSteps; i++)
            {
                firstStep = QuadraticLerp(beforePosition, offsetCenterNode, pointTarget.transform.position, (1f / countSteps) * i);
                secondStep = QuadraticLerp(beforePosition, offsetCenterNode, pointTarget.transform.position, (1f / countSteps) * (i + 1));

                transform.eulerAngles = new Vector3(0f, Mathf.Lerp(currentRot, currentRot + rotationAngle, (1f / countSteps) * i), 0f);

                yield return new WaitForEndOfFrame();
                //yield return new WaitForSeconds(0.2f);
            }
        }
        target = roadToNeighbor;

        isChoose = true;

        isOnCurve = false;

        RotateCar(target);
    }

    private Vector3 QuadraticLerp(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 v1 = Vector3.Lerp(a, b, t);
        Vector3 v2 = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(v1, v2, t);
    }

    public Vector3 GetRightOffset(Vector3 target, float offset)
    {
        Vector3 offsetVector;
        offsetVector.x = -target.z * (-1);
        offsetVector.z = target.x * (-1);
        offsetVector.y = 0f;

        return offsetVector.normalized * offset;
        //return Vector3Int.zero;
    }
}
