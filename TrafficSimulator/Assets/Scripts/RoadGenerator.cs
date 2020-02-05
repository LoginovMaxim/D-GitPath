using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public static List<GraphNode> graphNodes;   // статический список узлов
    public GameObject[] roadSegments;           // массив префабов (дорожные объекты)

    List<Vector3Int> arrayA;                    // список позиций уже созданных векторов (дорог)
    List<Vector3Int> arrayB;                    // для проверки дальнейших соединений на пересечение

    // параметризация дорожной системы
    [Range(2, 101)] [SerializeField]
    private int numberNodes;                    // количество узлов
    [Range(1, 101)] [SerializeField]
    private int sizeMap;                        // размер карты
    [SerializeField]
    private int scaleParameter;                 // характерный размер (от объёма префаба дороги)
    [Range(1, 4)] [SerializeField]
    private int maxConnections;                 // максимально допустимое количество соединений дорог (ограничение на количество соседей любого узла)

    public GameObject car;                      // префаб машины
    public List<GameObject> cars;               // список созданных машин 

    public float speedSimpleCars;               // допустимая скорость движения

    public Transform plane;                     // позиция земли под дорогой (для ограничения по размеру карты на добавление новых узлов)
    void Start()
    {
        graphNodes = new List<GraphNode>();
        arrayA = new List<Vector3Int>();
        arrayB = new List<Vector3Int>();

        cars = new List<GameObject>();

        SecondVersion();                        // вторая версия генератора дорожной системы

        plane.position = new Vector3(sizeMap * 18f, 0f, sizeMap * 18f);
        plane.localScale = new Vector3(sizeMap * 40f, 0f, sizeMap * 40f);

        Camera.main.transform.position = new Vector3(sizeMap * 18f, 100f, sizeMap * 18f);
        Camera.main.orthographicSize = sizeMap * 22f;

               // TimerTrafficLight - секундомер светофоров (общий для всех)
    }

    public void SecondVersion()
    {
        int xx, zz;
        bool isSameNode;

        for (int a = 0; a < numberNodes; a++)   // создание указанного количества дорожных узлов
        {
            isSameNode = false;

            xx = Random.Range(0, sizeMap) * scaleParameter;     // задание рандомной позиции по х с учётом характерного масштаба
            zz = Random.Range(0, sizeMap) * scaleParameter;     // задание рандомной позиции по z с учётом характерного масштаба

            foreach (GraphNode node in graphNodes)  // Проверка: является ли новая позиция - позицией уже существующего узла?
            {
                if (node.nodePosition.x == xx && node.nodePosition.z == zz) { isSameNode = true; a--; break; }
            }

            if (isSameNode == false)
            {
                graphNodes.Add(new GraphNode(a, new Vector3Int(xx, 0, zz))); // Создание дорожного узла с начальной инициализацией уникального номера и позиции
                //yield return new WaitForSeconds(1f);
            }

        }

        Vector3Int path;                        // вектор - копия позиции текущего узла (изменяемый в процессе)
        Vector3Int delta = Vector3Int.zero;     // вектор - шаг по пространству 

        foreach (GraphNode node in graphNodes)  // поиск соседей
        {
            SearchPathToNeighbors(node, false);
        }
    
        foreach (GraphNode node in graphNodes)  // создание префаба дороги, для каждого узла
        {
            CreateRoadType(node, false);
        }
        
        foreach (GraphNode node in graphNodes)  // соединение узлов прямой дорогой
        {
            CreateConnectedRoads(node);
        }

        foreach (GraphNode node in graphNodes)  // соединение узлов прямой дорогой
        {
            node.isActive = true;
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            float correctionTileX;
            float correctionTileZ;

            bool isBusy = false;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForceAtPosition(ray.direction, hit.point);
                }
            }
            correctionTileX = Mathf.Round(Mathf.Round(hit.point.x) / scaleParameter) * scaleParameter;
            correctionTileZ = Mathf.Round(Mathf.Round(hit.point.z) / scaleParameter) * scaleParameter;

            foreach (GraphNode node in graphNodes)
            {
                if (node.nodePosition.x == correctionTileX && node.nodePosition.z == correctionTileZ) { isBusy = true; break; }
            }

            if(isBusy == false)
            {
                graphNodes.Add(new GraphNode(numberNodes++, new Vector3Int(Mathf.RoundToInt(correctionTileX), 0, Mathf.RoundToInt(correctionTileZ))));

                int last = graphNodes.Count - 1;

                SearchPathToNeighbors(graphNodes[last], true);
                CreateRoadType(graphNodes[last], false);
                CreateConnectedRoads(graphNodes[last]);
                graphNodes[last].isActive = true;
            }
        }

        if (Input.GetKeyDown("c"))
        {
            GraphNode randomPositionForCar;
            do
            {
                randomPositionForCar = graphNodes[Random.Range(0, graphNodes.Count)];
            }
            while (randomPositionForCar.neighbors.Count == 0);

            Instantiate(car, randomPositionForCar.nodePosition + new Vector3(0f, 2f, 0f)/* + GetRightOffset(randomPositionForCar.roadsToNeighbors[0])*/, Quaternion.identity).GetComponent<SimpleCar>().currentNode = randomPositionForCar;
        }

        if (Input.GetKeyDown("i"))
        {
            foreach (GraphNode node in graphNodes)
            {
                Debug.Log($"({node.nodeIndex}) in {node.nodePosition} position");
            }
        }


        if (Input.GetKeyDown("n"))
        {
            foreach (GraphNode node in graphNodes)
            {
                for (int n = 0; n < node.neighbors.Count; n++)
                {
                    Debug.Log($"({node.nodeIndex}) and {node.neighbors[n].nodeIndex} are neighbors");
                }
            }
        }

        if (Input.GetKeyDown("t"))
        {
            InvokeRepeating("TTL", 10f, 10f);
        }
    }

    private void CreateConnectedRoads(GraphNode node)
    {
        for (int n = 0; n < node.neighbors.Count; n++)
        {
            if(node.neighbors[n].isActive == true)
            {
                Vector3Int[] connectionPositions = node.CreateConnectedRoads(node.neighbors[n]);

                for (int c = 0; c < connectionPositions.Length; c++)
                {
                    Instantiate(roadSegments[0], connectionPositions[c], Quaternion.Euler(0f, node.SetAngleForSingleRoad(node.nodePosition, node.neighbors[n].nodePosition), 0f));
                }
            }
        }
        node.isActive = false;
    }

    private void SearchPathToNeighbors(GraphNode node, bool isUserAdd)
    {
        bool isCross;                           // условие пересечения
        Vector3Int path;                        // вектор - копия позиции текущего узла (изменяемый в процессе)
        Vector3Int delta = Vector3Int.zero;     // вектор - шаг по пространству 

        for (int DIR = 0; DIR < 4; DIR++)   // поиск по четырём направлениям
        {
            if (node.neighbors.Count < maxConnections)   // Проверка: может ли данный узел иметь ещё одного соседа?
            {
                bool isNeighbor = false;    // условие для поиска соседа

                path = node.nodePosition;                                               // копирование позиции текущего узла
                if (DIR == 0) { delta = new Vector3Int(scaleParameter, 0, 0); }         // задание шага по х
                else if (DIR == 1) { delta = new Vector3Int(0, 0, scaleParameter); }    // задание шага по z
                else if (DIR == 2) { delta = new Vector3Int(-scaleParameter, 0, 0); }   // задание шага по х (в обратном направлении) 
                else if (DIR == 3) { delta = new Vector3Int(0, 0, -scaleParameter); }   // задание шага по z (в обратном направлении)

                while (isNeighbor == false)     // поиск соседа
                {
                    path += delta;  // смешение позиции на заданный шаг

                    // Проверка: шагнули ли мы на пределы карты (спасение от бесконечного поиска соседа)
                    if (path.x < 0 || path.x > sizeMap * scaleParameter || path.z < 0 || path.z > sizeMap * scaleParameter) { break; }

                    foreach (GraphNode probublyNeightbor in graphNodes)     // Список возможных соседей
                    {
                        isCross = false;
                        if (node.nodePosition != probublyNeightbor.nodePosition)    // Проверка: не является ли возможный узел текущим узлом?
                        {
                            if (arrayA.Count > 0)   // Проверка: существует хотя бы один элемент массива уже созданных путей? 
                            {
                                for (int i = 0; i < arrayA.Count; i++)
                                {
                                    // Проверка: пересекается ли путь от текущего узла до возможного с уже существующими путями
                                    if (node.IsCrossing(node.nodePosition, probublyNeightbor.nodePosition, arrayA[i], arrayB[i]) == true) { isCross = true; break; }
                                }
                            }

                            // Проверка: Это возможный сосед? Путь до него не пересекает другие пути?
                            if (path == probublyNeightbor.nodePosition && isCross == false)
                            {
                                node.neighbors.Add(probublyNeightbor);          // занесение нового соседа в список соседей

                                if (isUserAdd)
                                {
                                    probublyNeightbor.neighbors.Add(node);
                                    probublyNeightbor.CreateAllVectorsToNeighbors(true);

                                    CreateRoadType(probublyNeightbor, true);
                                }

                                arrayA.Add(node.nodePosition);                  // занесеним в список 
                                arrayB.Add(probublyNeightbor.nodePosition);     // новой путь
                                isNeighbor = true;                              // подтверждаем, что нашли нового соседа   
                                break;                                          // прекращаем поис других соседей в текущем направлении
                            }
                        }
                    }
                }
            }
        }
        node.CreateAllVectorsToNeighbors(false);    // Создание множества направлений от текущего узла до каждого найденного соседа
    }
    public void CreateRoadType(GraphNode current, bool replacement) 
    {
        if(replacement == true) { Destroy(current.roadType); }
        if (current.neighbors.Count == 0)
        {
        }
        else if (current.neighbors.Count == 1)
        {
            current.roadType = Instantiate(roadSegments[0], current.nodePosition, Quaternion.Euler(0f, current.SetAngleForSingleRoad(current.nodePosition, current.neighbors[0].nodePosition), 0f));
        }
        else if (current.neighbors.Count == 2)
        {
            if (current.isStraightConnect(current.nodePosition, current.neighbors[0].nodePosition, current.neighbors[1].nodePosition) == true)
            { 
                current.roadType = Instantiate(roadSegments[0], current.nodePosition, Quaternion.Euler(0f, current.SetAngleForDoubleRoadStraight(current.nodePosition, current.neighbors[0].nodePosition, current.neighbors[1].nodePosition), 0f));
            }
            else
            {
                current.roadType = Instantiate(roadSegments[1], current.nodePosition, Quaternion.Euler(0f, current.SetAngleForDoubleRoad(current.nodePosition, current.neighbors[0].nodePosition, current.neighbors[1].nodePosition), 0f));
            }
        }
        else if (current.neighbors.Count == 3)
        {
            current.roadType = Instantiate(roadSegments[2], current.nodePosition, Quaternion.Euler(0f, current.SetAngleForTripleRoad(current.nodePosition, current.neighbors[0].nodePosition, current.neighbors[1].nodePosition, current.neighbors[2].nodePosition), 0f));
        }
        else if (current.neighbors.Count == 4)
        {
            current.roadType = Instantiate(roadSegments[3], current.nodePosition, Quaternion.identity);
        }
    }
    
    public void CreateNewCar()
    {
        GraphNode randomPositionForCar;
        do
        {
            randomPositionForCar = graphNodes[Random.Range(0, graphNodes.Count)];
        }
        while (randomPositionForCar.neighbors.Count == 0);

        Instantiate(car, randomPositionForCar.nodePosition + new Vector3(0f, 2f, 0f), Quaternion.identity).GetComponent<SimpleCar>().currentNode = randomPositionForCar;
    }

    private void TTL() // TimerTrafficLight
    {
        foreach (GraphNode node in graphNodes)
        {
            if (node.roadType.name == "1-path" || node.roadType.name == "1-path(Clone)")
            {
                node.isGreenTrafficLight = true;
            }
            else if (node.roadType.name == "2-path" || node.roadType.name == "2-path(Clone)")
            {
                node.isGreenTrafficLight = true;
            }
            else if (node.roadType.name == "3-path" || node.roadType.name == "3-path(Clone)")
            {
                node.isGreenTrafficLight = !node.isGreenTrafficLight;
                if (node.isGreenTrafficLight == true)
                {
                    node.roadType.transform.GetChild(6).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(7).gameObject.SetActive(false);
                }
                else
                {
                    node.roadType.transform.GetChild(6).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(7).gameObject.SetActive(true);
                }
            }
            else if (node.roadType.name == "4-path" || node.roadType.name == "4-path(Clone)")
            {
                node.isGreenTrafficLight = !node.isGreenTrafficLight;
                if (node.isGreenTrafficLight == true)
                {
                    node.roadType.transform.GetChild(8).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(9).gameObject.SetActive(false);
                }
                else
                {
                    node.roadType.transform.GetChild(8).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(9).gameObject.SetActive(true);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // отрисовка вспомогательных элементов

        foreach (GraphNode node in graphNodes)  // обозначение узлов
        {            
            Debug.DrawLine(node.nodePosition, node.nodePosition + new Vector3Int(2, 0, 2), Color.gray, Time.fixedDeltaTime, true);
            Debug.DrawLine(node.nodePosition, node.nodePosition + new Vector3Int(-2, 0, 2), Color.gray, Time.fixedDeltaTime, true);
            Debug.DrawLine(node.nodePosition, node.nodePosition + new Vector3Int(2, 0, -2), Color.gray, Time.fixedDeltaTime, true);
            Debug.DrawLine(node.nodePosition, node.nodePosition + new Vector3Int(-2, 0, -2), Color.gray, Time.fixedDeltaTime, true);
        }

        // обозрачение границ карты

        Debug.DrawLine(new Vector3(-(plane.localScale.x / 2 - plane.position.x), 0f, -(plane.localScale.z / 2 - plane.position.z)),
            new Vector3((plane.localScale.x / 2 + plane.position.x), 0f, -(plane.localScale.z / 2 - plane.position.z)), Color.white, Time.fixedDeltaTime, true);

        Debug.DrawLine(new Vector3((plane.localScale.x / 2 + plane.position.x), 0f, -(plane.localScale.z / 2 - plane.position.z)),
            new Vector3((plane.localScale.x / 2 + plane.position.x), 0f, (plane.localScale.z / 2 + plane.position.z)), Color.white, Time.fixedDeltaTime, true);

        Debug.DrawLine(new Vector3((plane.localScale.x / 2 + plane.position.x), 0f, (plane.localScale.z / 2 + plane.position.z)),
            new Vector3(-(plane.localScale.x / 2 - plane.position.x), 0f, (plane.localScale.z / 2 + plane.position.z)), Color.white, Time.fixedDeltaTime, true);

        Debug.DrawLine(new Vector3(-(plane.localScale.x / 2 - plane.position.x), 0f, (plane.localScale.z / 2 + plane.position.z)),
            new Vector3(-(plane.localScale.x / 2 - plane.position.x), 0f, -(plane.localScale.z / 2 - plane.position.z)), Color.white, Time.fixedDeltaTime, true);
    }
}
