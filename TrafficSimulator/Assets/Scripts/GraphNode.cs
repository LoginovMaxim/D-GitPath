using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GraphNode
{
    public int nodeIndex { get; set; }              // Уникальный номер узла
    public Vector3Int nodePosition { get; set; }    // Позиция в мировом пространстве
    public List<GraphNode> neighbors;               // Список соседних узлов
    public List<Vector3> roadsToNeighbors;          // Список векторов до соседних узлов
    public bool isActive { get; set; }                     // Проверка: является ли улез активным? (правда - движение ничего не мешает, ложь - препятствие)
    public bool isGreenTrafficLight { get; set; }   // Проверка: состояние светофора (правда - зелёный, ложь - красный)
    public GameObject roadType;                     // Объект префаба дороги (от него удобно ссылаться на его детей (точки поворота / светофор))

    public GraphNode(int nodeIndex, Vector3Int nodePosition)    // конструктор узла графа (первичная инициализация)
    {
        this.nodeIndex = nodeIndex;
        this.nodePosition = nodePosition;

        neighbors = new List<GraphNode>();
        roadsToNeighbors = new List<Vector3>();

        isActive = true;

        int randTrafficLightState = Random.Range(0, 2);
        if(randTrafficLightState == 0)
            isGreenTrafficLight = true;
        else
            isGreenTrafficLight = false;
    }

    public void CreateAllVectorsToNeighbors(bool repeat)    // Создание множества направлений от узла до каждого соседнего узла
    {
        if (repeat == true) 
            roadsToNeighbors.Clear();

        foreach (GraphNode neighbor in neighbors)
        {
            roadsToNeighbors.Add(neighbor.nodePosition - nodePosition);
        }
    }

    public Vector3Int[] CreateConnectedRoads(GraphNode neighbor)    // Заполнение прастранства между узлом и его соседями соеденительными дорогами
    {
        List<Vector3Int> connectionPos = new List<Vector3Int>();
        int steps;
        steps = Mathf.FloorToInt((neighbor.nodePosition - nodePosition).magnitude / 36);
        for(int j = 1; j < steps; j++)
        {
            Vector3 v3 = (neighbor.nodePosition - nodePosition);
            v3 = v3.normalized;
            connectionPos.Add(nodePosition + Vector3Int.FloorToInt(v3) * j * 36);
        }

        return connectionPos.ToArray();
    }
    public bool IsCrossing(Vector3Int A1, Vector3Int A2, Vector3Int B1, Vector3Int B2)  // Проверка на пересечение двух отрезков
    {
        A1.y = 0;
        A2.y = 0;
        B1.y = 0;
        B2.y = 0;
        //Vector2 A2 = neighbors[indexNeighbor].nodePosition;

        float v1, v2, v3, v4;

        v1 = (B2.x - B1.x) * (A1.z - B1.z) - (B2.z - B1.z) * (A1.x - B1.x);
        v2 = (B2.x - B1.x) * (A2.z - B1.z) - (B2.z - B1.z) * (A2.x - B1.x);
        v3 = (A2.x - A1.x) * (B1.z - A1.z) - (A2.z - A1.z) * (B1.x - A1.x);
        v4 = (A2.x - A1.x) * (B2.z - A1.z) - (A2.z - A1.z) * (B2.x - A1.x);

        return ((v1 * v2 < 0) && (v3 * v4 < 0));
    }

    public bool isStraightConnect(Vector3Int main, Vector3Int v1, Vector3Int v2)    // Проверка на отсутствие поворота узла (для выбора типа префаба дороги)
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

    public float SetAngleForSingleRoad(Vector3Int main, Vector3Int v1)  //Вызывается при создании префаба дорожного узла (возвращает значени угла поворота префаба)
    {
        if (main.x == v1.x && main.z != v1.z) { return 0; }
        if (main.x != v1.x && main.z == v1.z) { return -90; }
        else { return 0; }
    }

    public float SetAngleForDoubleRoadStraight(Vector3Int main, Vector3Int v1, Vector3Int v2)  //Вызывается при создании префаба дорожного узла (возвращает значени угла поворота префаба)
    {
        if (main.x == v1.x && main.z != v1.z && main.x == v2.x && main.z != v2.z) { return 0; }
        if (main.x != v1.x && main.z == v1.z && main.x != v2.x && main.z == v2.z) { return -90; }
        else { return 0; }
    }

    public float SetAngleForDoubleRoad(Vector3Int main, Vector3Int v1, Vector3Int v2)  //Вызывается при создании префаба дорожного узла (возвращает значени угла поворота префаба)
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

    public float SetAngleForTripleRoad(Vector3Int main, Vector3Int v1, Vector3Int v2, Vector3Int v3)  //Вызывается при создании префаба дорожного узла (возвращает значени угла поворота префаба)
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
