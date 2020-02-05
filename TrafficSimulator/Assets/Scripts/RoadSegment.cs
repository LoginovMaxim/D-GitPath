using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadSegment
{
    [SerializeField]
    public Vector3Int roadPos;
    [SerializeField]
    public List<RoadSegment> neighbors = new List<RoadSegment>();
    public bool isConnected;

    public int indexType;

    public RoadSegment(Vector3Int position)
    {
        roadPos = position;
        indexType = 0;
        isConnected = false;
    }

    public bool IsCrossing(Vector2Int B1, Vector2Int B2, int indexNeighbor)
    {
        Vector2Int A1 = (Vector2Int)roadPos;
        Vector2Int A2 = (Vector2Int)neighbors[indexNeighbor].roadPos;

        float v1, v2, v3, v4;

        v1 = (B2.x - B1.x) * (A1.y - B1.y) - (B2.y - B1.y) * (A1.x - B1.x);
        v2 = (B2.x - B1.x) * (A2.y - B1.y) - (B2.y - B1.y) * (A2.x - B1.x);
        v3 = (A2.x - A1.x) * (B1.y - A1.y) - (A2.y - A1.y) * (B1.x - A1.x);
        v4 = (A2.x - A1.x) * (B2.y - A1.y) - (A2.y - A1.y) * (B2.x - A1.x);

        return (v1 * v2 < 0) && (v3 * v4 < 0);
    }

    public bool isStraightConnect(Vector3Int main, Vector3Int v1, Vector3Int v2)
    {
        if (main.x == v1.x && main.z != v1.z && main.x == v2.x && main.z != v2.z) { return true; }
        if (main.x != v1.x && main.z == v1.z && main.x != v2.x && main.z == v2.z) { return true; }

        if (main.x > v1.x && main.z == v1.z && main.x == v2.x && main.z < v2.z) { return false; }
        if (main.x > v2.x && main.z == v2.z && main.x == v1.x && main.z < v1.z) { return false; }

        if (main.x < v1.x && main.z == v1.z && main.x == v2.x && main.z < v2.z) { return false; }
        if (main.x < v2.x && main.z == v2.z && main.x == v1.x && main.z < v1.z) { return false; }

        if (main.x < v1.x && main.z == v1.z && main.x == v2.x && main.z > v2.z) { return false; }
        if (main.x < v2.x && main.z == v2.z && main.x == v1.x && main.z > v1.z) { return false; }

        if (main.x > v1.x && main.z == v1.z && main.x == v2.x && main.z > v2.z) { return false; }
        if (main.x > v2.x && main.z == v2.z && main.x == v1.x && main.z > v1.z) { return false; }
        else { return false; }
    }

    public float SetAngleForSingleRoad(Vector3Int main, Vector3Int v1)
    {
        if (main.x == v1.x && main.z != v1.z) { return 0; }
        if (main.x != v1.x && main.z == v1.z) { return -90; }
        else { return 0; }
    }

    public float SetAngleForDoubleRoadStraight(Vector3Int main, Vector3Int v1, Vector3Int v2)
    {
        if (main.x == v1.x && main.z != v1.z && main.x == v2.x && main.z != v2.z) { return 0; }
        if (main.x != v1.x && main.z == v1.z && main.x != v2.x && main.z == v2.z) { return -90; }
        else { return 0; }
    }

    public float SetAngleForDoubleRoad(Vector3Int main, Vector3Int v1, Vector3Int v2)
    {
        if (main.x > v1.x && main.z == v1.z && main.x == v2.x && main.z < v2.z) { return 0; }
        if (main.x > v2.x && main.z == v2.z && main.x == v1.x && main.z < v1.z) { return 0; }

        if (main.x < v1.x && main.z == v1.z && main.x == v2.x && main.z < v2.z) { return 90; }
        if (main.x < v2.x && main.z == v2.z && main.x == v1.x && main.z < v1.z) { return 90; }

        if (main.x < v1.x && main.z == v1.z && main.x == v2.x && main.z > v2.z) { return 180; }
        if (main.x < v2.x && main.z == v2.z && main.x == v1.x && main.z > v1.z) { return 180; }

        if (main.x > v1.x && main.z == v1.z && main.x == v2.x && main.z > v2.z) { return 270; }
        if (main.x > v2.x && main.z == v2.z && main.x == v1.x && main.z > v1.z) { return 270; }
        else { return 0; }
    }

    public float SetAngleForTripleRoad(Vector3Int main, Vector3Int v1, Vector3Int v2, Vector3Int v3)
    {
        if (main.x > v1.x && main.z == v1.z && 
            main.x == v2.x && main.z < v2.z && 
            main.x < v3.x && main.z == v3.z) 
        { return 0; }

        if (main.x > v1.x && main.z == v1.z &&
            main.x == v3.x && main.z < v3.z &&
            main.x < v2.x && main.z == v2.z)
        { return 0; }

        if (main.x > v2.x && main.z == v2.z &&
            main.x == v1.x && main.z < v1.z &&
            main.x < v3.x && main.z == v3.z)
        { return 0; }

        if (main.x > v2.x && main.z == v2.z &&
            main.x == v3.x && main.z < v3.z &&
            main.x < v1.x && main.z == v1.z)
        { return 0; }

        if (main.x > v3.x && main.z == v3.z &&
            main.x == v1.x && main.z < v1.z &&
            main.x < v2.x && main.z == v2.z)
        { return 0; }

        if (main.x > v3.x && main.z == v3.z &&
            main.x == v2.x && main.z < v2.z &&
            main.x < v1.x && main.z == v1.z)
        { return 0; }


        if (main.x == v1.x && main.z < v1.z &&
            main.x < v2.x && main.z == v2.z &&
            main.x == v3.x && main.z > v3.z)
        { return 90; }

        if (main.x == v1.x && main.z < v1.z &&
            main.x < v3.x && main.z == v3.z &&
            main.x == v2.x && main.z > v2.z)
        { return 90; }

        if (main.x == v2.x && main.z < v2.z &&
            main.x < v1.x && main.z == v1.z &&
            main.x == v3.x && main.z > v3.z)
        { return 90; }

        if (main.x == v2.x && main.z < v2.z &&
            main.x < v3.x && main.z == v3.z &&
            main.x == v1.x && main.z > v1.z)
        { return 90; }

        if (main.x == v3.x && main.z < v3.z &&
            main.x < v1.x && main.z == v1.z &&
            main.x == v2.x && main.z > v2.z)
        { return 90; }

        if (main.x == v3.x && main.z < v3.z &&
            main.x < v2.x && main.z == v2.z &&
            main.x == v1.x && main.z > v1.z)
        { return 90; }


        if (main.x < v1.x && main.z == v1.z &&
            main.x == v2.x && main.z > v2.z &&
            main.x > v3.x && main.z == v3.z)
        { return 180; }

        if (main.x < v1.x && main.z == v1.z &&
            main.x == v3.x && main.z > v3.z &&
            main.x > v2.x && main.z == v2.z)
        { return 180; }

        if (main.x < v2.x && main.z == v2.z &&
            main.x == v1.x && main.z > v1.z &&
            main.x > v3.x && main.z == v3.z)
        { return 180; }

        if (main.x < v2.x && main.z == v2.z &&
            main.x == v3.x && main.z > v3.z &&
            main.x > v1.x && main.z == v1.z)
        { return 180; }

        if (main.x < v3.x && main.z == v3.z &&
            main.x == v1.x && main.z > v1.z &&
            main.x > v2.x && main.z == v2.z)
        { return 180; }

        if (main.x < v3.x && main.z == v3.z &&
            main.x == v2.x && main.z > v2.z &&
            main.x > v1.x && main.z == v1.z)
        { return 180; }


        if (main.x == v1.x && main.z > v1.z &&
            main.x > v2.x && main.z == v2.z &&
            main.x == v3.x && main.z < v3.z)
        { return 270; }

        if (main.x == v1.x && main.z > v1.z &&
            main.x > v3.x && main.z == v3.z &&
            main.x == v2.x && main.z < v2.z)
        { return 270; }

        if (main.x == v2.x && main.z > v2.z &&
            main.x > v1.x && main.z == v1.z &&
            main.x == v3.x && main.z < v3.z)
        { return 270; }

        if (main.x == v2.x && main.z > v2.z &&
            main.x > v3.x && main.z == v3.z &&
            main.x == v1.x && main.z < v1.z)
        { return 270; }

        if (main.x == v3.x && main.z > v3.z &&
            main.x > v1.x && main.z == v1.z &&
            main.x == v2.x && main.z < v2.z)
        { return 270; }

        if (main.x == v3.x && main.z > v3.z &&
            main.x > v2.x && main.z == v2.z &&
            main.x == v1.x && main.z < v1.z)
        { return 270; }
        else { return 0; }
    }
}
