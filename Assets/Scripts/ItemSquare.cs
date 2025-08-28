using TMPro;
using UnityEngine;

public class ItemSquare : MonoBehaviour
{
    [SerializeField] public int X { get; set; }
    [SerializeField] public int Y { get; set; }
    public GameManager.SquareType Type { get; private set; }

    public MoveSquare MoveComponent { get; private set; }
    public ClearSquare ClearSquare { get; private set; }
    public HeroExpSquare HeroExpSquare { get; private set; }

    public bool CanMove() => MoveComponent != null;
    public bool IsHeroExpSquare() => HeroExpSquare != null;
    public bool CanClear() => ClearSquare != null;

    private TextMeshPro combineCountText;
    private int combineCountValue = 1;

    public int CombineCount
    {
        get { return combineCountValue; }
        set
        {
            combineCountValue = value;
            if (value == 1)
            {
                combineCountText.text = "";
            }
            else
            {
                combineCountText.text = combineCountValue.ToString();
            }
        }
    }

    private void Awake()
    {
        MoveComponent = GetComponent<MoveSquare>();
        HeroExpSquare = GetComponent<HeroExpSquare>();
        ClearSquare = GetComponent<ClearSquare>();
       
    }

    public void Init(int x, int y, GameManager.SquareType type)

    {
        X = x;
        Y = y;
        Type = type;
        if (Type == GameManager.SquareType.HEROEXP || Type == GameManager.SquareType.ATTACK)
        {
            combineCountText = transform.Find("TextCombine").GetComponent<TextMeshPro>();
            combineCountText.text = "";
            combineCountValue = 1;
        }
    }

    // private void OnMouseEnter()
    // {
    //     GameManager.Instance.EnterSquare(this);
    // }
    //
    // private void OnMouseDown()
    // {
    //     GameManager.Instance.PressSquare(this);
    // }
    //
    // private void OnMouseUp()
    // {
    //     GameManager.Instance.ReleaseSquare();
    // }
}