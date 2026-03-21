using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Vector3Int CellPos { get; private set; }
    
    private float currentDamage;
    private DiceHandType currentType;

    public void Initialize(Vector3Int cellPos)
    {
        CellPos = cellPos;
    }

    public void SetupTowerStats(TowerHandData handData, float damageMultiplier)
    {
        // 1. 외형 변경
        spriteRenderer.sprite = handData.TowerSprite;
        Debug.Log(handData.TowerSprite.name);
        // 2. 내부 데이터 세팅
        currentType = handData.HandType;
        Debug.Log(currentType.ToString());
        // 3. 최종 데미지 = 족보 기본 데미지 * 주사위 대표값 배율
        currentDamage = handData.BaseDamage * damageMultiplier;
        Debug.Log(currentDamage);
    }

    private void Update()
    {
        // TODO
    }
}