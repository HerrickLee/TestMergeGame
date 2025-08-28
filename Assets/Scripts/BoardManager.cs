using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class SquarePrefab
    {
        public GameManager.SquareType type;
        public GameObject prefab;
    }

    [Serializable]
    public class WeightPrefab
    {
        public GameManager.SquareType type;
        public int weight;
    }

    public SquarePrefab[] squarePrefabs;
    public WeightPrefab[] squareWeightPrefabs;
    public int xColumn;
    public int yRow;
    public GameObject gridPrefab;

    private int totalSquareWeight = 0;
    private ItemSquare[,] squareItems;
    private Dictionary<GameManager.SquareType, GameObject> squarePrefabDic;
    private float dropTime = 0.1f;
    private int movingCount = 0;
    private bool isFilling = false;

    private HashSet<(int x, int y)> boomClearSquaresSet = new HashSet<(int x, int y)>();
    private List<ItemSquare> overflowSquares = new List<ItemSquare>();

    private ItemSquare clickDownItem;
    private ItemSquare clickUpItem;
    private float clickTimer = 0f;
    private bool startClick = false;
    private bool isCombine = false;

    public static BoardManager Instance;

    private static readonly int[] dx = { 0, 1, 0, -1 };
    private static readonly int[] dy = { -1, 0, 1, 0 };

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        InitPrefabDictionary();
        InitBoardArray();
        StartFill();
    }

    #region 初始化

    private void InitPrefabDictionary()
    {
        squarePrefabDic = new Dictionary<GameManager.SquareType, GameObject>();
        foreach (var sp in squarePrefabs)
            if (!squarePrefabDic.ContainsKey(sp.type))
                squarePrefabDic[sp.type] = sp.prefab;

        totalSquareWeight = 0;
        foreach (var wp in squareWeightPrefabs) totalSquareWeight += wp.weight;
    }

    private void InitBoardArray()
    {
        squareItems = new ItemSquare[xColumn, yRow];
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                Instantiate(gridPrefab, CalculatePosition(x, y), Quaternion.identity, transform).name = $"EMPTY_{x}_{y}";
                CreateNewSquare(x, y, GameManager.SquareType.EMPTY);
            }
        }
    }

    #endregion

    #region 坐标与方块生成

    public Vector3 CalculatePosition(int x, int y)
    {
        return new Vector3(transform.position.x - xColumn / 2 + x,
                           transform.position.y + yRow / 2f - y, 0);
    }

    private ItemSquare CreateNewSquare(int x, int y, GameManager.SquareType type)
    {
        if (x < 0 || x >= xColumn || y < 0 || y >= yRow) return null;

        GameObject obj = Instantiate(squarePrefabDic[type], CalculatePosition(x, y), Quaternion.identity, transform);
        ItemSquare square = obj.GetComponent<ItemSquare>();
        square.Init(x, y, type);
        squareItems[x, y] = square;
        return square;
    }

    private GameManager.SquareType GetRandomWeightedType()
    {
        int rand = Random.Range(0, totalSquareWeight);
        int cur = 0;
        foreach (var wp in squareWeightPrefabs)
        {
            cur += wp.weight;
            if (rand < cur) return wp.type;
        }
        return GameManager.SquareType.COUNT;
    }

    #endregion

    #region 填充掉落

    public void StartFill()
    {
        if (!isFilling)
            FillStep();
    }

    private void FillStep()
    {
        isFilling = true;
        bool needFill = false;

        for (int y = yRow - 2; y >= 0; y--)
        {
            for (int x = 0; x < xColumn; x++)
            {
                ItemSquare square = squareItems[x, y];
                if (square.CanMove())
                {
                    ItemSquare below = squareItems[x, y + 1];
                    if (below.Type == GameManager.SquareType.EMPTY)
                    {
                        Destroy(below.gameObject);
                        squareItems[x, y + 1] = square;
                        CreateNewSquare(x, y, GameManager.SquareType.EMPTY);

                        movingCount++;
                        square.MoveComponent.Move(x, y + 1, dropTime, OnMoveComplete);
                        needFill = true;
                    }
                }
            }
        }

        // 填充最上行
        for (int x = 0; x < xColumn; x++)
        {
            ItemSquare topSquare = squareItems[x, 0];
            if (topSquare.Type == GameManager.SquareType.EMPTY)
            {
                Destroy(topSquare.gameObject);

                GameManager.SquareType type = GetRandomWeightedType();
                if (type != GameManager.SquareType.COUNT)
                {
                    ItemSquare newSquare = CreateNewSquare(x, 0, type);
                    newSquare.transform.position = CalculatePosition(x, -1);
                    movingCount++;
                    newSquare.MoveComponent.Move(x, 0, dropTime, OnMoveComplete);
                    needFill = true;
                }
            }
        }

        if (!needFill) isFilling = false;
    }

    private void OnMoveComplete()
    {
        movingCount--;
        if (movingCount == 0) FillStep();
    }

    #endregion

    #region 向上挤出

    public void UpBoardSquares()
    {
        overflowSquares.Clear();
        movingCount = 0;

        for (int y = 0; y < yRow; y++)
        {
            for (int x = 0; x < xColumn; x++)
            {
                ItemSquare square = squareItems[x, y];
                if (square.Type != GameManager.SquareType.EMPTY && square.MoveComponent != null)
                {
                    int targetY = y - 1;
                    if (targetY < 0)
                    {
                        overflowSquares.Add(square);
                        targetY = 0;
                    }

                    movingCount++;
                    squareItems[x, targetY] = square;
                    square.MoveComponent.Move(x, targetY, dropTime, OnMoveCompleteUp);
                }
            }
        }
    }

    private void OnMoveCompleteUp()
    {
        movingCount--;
        if (movingCount == 0) HandleOverflowSquares();
    }

    private void HandleOverflowSquares()
    {
        foreach (var square in overflowSquares)
        {
            if (square.Type == GameManager.SquareType.BOOM)
                ClearBoomSquare(square.X, square.Y);

            square.ClearSquare.UpDestory();
        }

        for (int x = 0; x < xColumn; x++)
        {
            ItemSquare newSquare = CreateNewSquare(x, yRow - 1, GameManager.SquareType.HEROEXP);
            newSquare.transform.position = CalculatePosition(x, yRow);
            movingCount++;
            newSquare.MoveComponent.Move(x, yRow - 1, dropTime, OnMoveCompleteUp);
        }

        if (movingCount == 0) StartFill();
    }

    #endregion

    #region Update 点击/长按逻辑

    private void Update()
    {
        HandleMouseInput();
        HandleClickTimer();
    }

    private void HandleMouseInput()
    {
        // 按下
        if (Input.GetMouseButtonDown(0))
        {
#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                return;
#elif UNITY_STANDALONE
            if (EventSystem.current.IsPointerOverGameObject())
                return;
#endif
            clickDownItem = SelectedItemSquare();
            clickTimer = 0f;
            startClick = true;
        }

        // 抬起
        if (Input.GetMouseButtonUp(0))
        {
            startClick = false;

            if (isCombine)
            {
                isCombine = false;
                return;
            }

            clickUpItem = SelectedItemSquare();
            if (clickUpItem != null && clickUpItem == clickDownItem)
            {
                List<ItemSquare> matchList = MatchNeighborSquares(clickUpItem);
                if (clickTimer < 1f)
                {
                    if (matchList.Count >= 3)
                        ClearAllConnectedSquare(matchList);
                    else
                        UIManager.Instance.ShowSingleSquareTip(clickUpItem.transform.position);
                }
            }

            clickTimer = 0f;
        }
    }

    private void HandleClickTimer()
    {
        if (!startClick) return;

        clickTimer += Time.deltaTime;

        if (clickTimer > 1f)
        {
            startClick = false;
            clickTimer = 0f;

            ItemSquare clickCombineItem = SelectedItemSquare();
            if (clickCombineItem == clickDownItem && clickCombineItem != null)
            {
                CheckCombineSquares(clickCombineItem);
                isCombine = true;
            }
        }
    }

    private ItemSquare SelectedItemSquare()
    {
        Vector2 inputPos;
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 0) return null;
        inputPos = Input.touches[0].position;
#else
        inputPos = Input.mousePosition;
#endif

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(inputPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        return hit != null ? hit.GetComponent<ItemSquare>() : null;
    }

    private void CheckCombineSquares(ItemSquare pressSquare)
    {
        List<ItemSquare> matchList = MatchNeighborSquares(pressSquare);
        if (matchList.Count >= 3)
            CombineMatchSquares(matchList, pressSquare);
    }

    #endregion

    #region 匹配/清除/合成逻辑

    public List<ItemSquare> MatchNeighborSquares(ItemSquare target)
    {
        List<ItemSquare> matchList = new List<ItemSquare>();
        List<(int x, int y)> connected = new List<(int x, int y)>();
        FindConnectedSquares(target.X, target.Y, new bool[yRow, xColumn], connected, target);

        foreach ((int x, int y) in connected)
        {
            ItemSquare square = squareItems[x, y];
            if (square.CanClear())
                matchList.Add(square);
        }

        return matchList;
    }

    private void FindConnectedSquares(int x, int y, bool[,] visited, List<(int x, int y)> connected, ItemSquare startSquare)
    {
        if (x < 0 || x >= xColumn || y < 0 || y >= yRow || visited[y, x] || squareItems[x, y].Type != startSquare.Type)
            return;

        if (startSquare.HeroExpSquare != null)
        {
            if (squareItems[x, y].HeroExpSquare == null || 
                squareItems[x, y].HeroExpSquare.CurExpType != startSquare.HeroExpSquare.CurExpType)
                return;
        }

        visited[y, x] = true;
        connected.Add((x, y));

        for (int i = 0; i < 4; i++)
            FindConnectedSquares(x + dx[i], y + dy[i], visited, connected, startSquare);
    }

    public void ClearAllConnectedSquare(List<ItemSquare> matchList)
    {
        bool needRefill = false;
        foreach (var item in matchList)
            if (ClearSquare(item.X, item.Y)) needRefill = true;

        if (needRefill)
        {
            Invoke("StartFill", 1f);//StartFill();
        }
    }

    public bool ClearSquare(int x, int y)
    {
        if (x < 0 || x >= xColumn || y < 0 || y >= yRow) return false;

        if (squareItems[x, y].CanClear() && !squareItems[x, y].ClearSquare.IsClearring)
        {
            squareItems[x, y].ClearSquare.Clear();
            CreateNewSquare(x, y, GameManager.SquareType.EMPTY);
            return true;
        }
        return false;
    }

    public void CombineMatchSquares(List<ItemSquare> matchList, ItemSquare target)
    {
        int combineCount = 0;
        bool needRefill = false;

        foreach (var item in matchList)
        {
            if (item != target)
            {
                combineCount += item.CombineCount;
                if (ClearSquare(item.X, item.Y)) needRefill = true;
            }
        }

        target.CombineCount += combineCount;
        if (needRefill) StartFill();
    }

    #endregion

    #region 炸弹清理

    private void ClearBoomSquare(int x, int y)
    {
        boomClearSquaresSet.Clear();
        ClearRow(y);
        ClearColumn(x);

        foreach (var (bx, by) in boomClearSquaresSet)
        {
            ClearSquare(bx, by);
            CreateNewSquare(bx, by, GameManager.SquareType.EMPTY);
        }
    }

    private void ClearRow(int row)
    {
        for (int x = 0; x < xColumn; x++)
            boomClearSquaresSet.Add((x, row));
    }

    private void ClearColumn(int col)
    {
        for (int y = 0; y < yRow; y++)
            boomClearSquaresSet.Add((col, y));
    }

    #endregion
}
