using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public struct TowerHandData
{
    public DiceHandType HandType;       // 기준 족보
    public Sprite TowerSprite;          // 족보에 따른 타워 이미지
    public float BaseDamage;            // 족보 고유의 기본 데미지
}

public class TowerPlacementManager : PhaseListener
{
    private Camera mainCamera;

    [SerializeField] private Tilemap buildableTilemap;
    [SerializeField] private GameObject baseTowerPrefab;
    [SerializeField] private List<TowerHandData> towerSettings;

    private readonly Dictionary<Vector3Int, Tower> placedTowers = new();

    private bool isBuildPhase = false;

    private Vector3Int pendingCellPos;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    protected override void HandlePhaseEntered(InGamePhase phase)
    {
        if (phase == InGamePhase.BuildSelect)
        {
            isBuildPhase = true;
            MouseInputHandler.Instance.OnMouseClick += HandleMouseClick;
        }
    }

    protected override void HandlePhaseExited(InGamePhase phase)
    {
        if (phase == InGamePhase.BuildSelect)
        {
            isBuildPhase = false;
            MouseInputHandler.Instance.OnMouseClick -= HandleMouseClick;
        }
    }

    private void HandleMouseClick(Vector3 mouseScreenPos)
    {
        if (!isBuildPhase) return;

        // ← 여기서 드래그 중이면 클릭 무시
        //if (MouseInputHandler.Instance.IsDragging)
        //    return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3Int cellPos = buildableTilemap.WorldToCell(mouseWorldPos);

        if (!CanPlaceAt(cellPos))
            return;

        pendingCellPos = cellPos;
        InGameFlowManager.Instance.GoToNextPhase();
    }

    public bool CanPlaceAt(Vector3Int cellPos)
    {
        if (!buildableTilemap.HasTile(cellPos))
        {
            return false;
        }

        if (placedTowers.ContainsKey(cellPos))
        {
            return false;
        }

        return true;
    }

    public void ConfirmAndPlaceTower(DiceHandResult result)
    {
        Vector3 spawnWorldPos = buildableTilemap.GetCellCenterWorld(pendingCellPos);
        
        // 1. 데이터 찾기.
        TowerHandData handData = towerSettings.Find(x => x.HandType == result.HandType);

        GameObject towerObject = Instantiate(baseTowerPrefab, spawnWorldPos, Quaternion.identity);
        Tower tower = towerObject.GetComponent<Tower>();

        if (tower != null)
        {
            tower.Initialize(pendingCellPos);

            // 2. 데미지 배율 계산
            float multiplier = 1.0f + (result.PrimaryValue * 0.2f);
            
            // 3. 타워에게 '족보 데이터'와 '추가 배율'을 함께 전달
            tower.SetupTowerStats(handData, multiplier);

            placedTowers.Add(pendingCellPos, tower);
            Debug.Log($"[{pendingCellPos}] {result.HandType} 타워 생성! (최종 데미지: {handData.BaseDamage * multiplier})");
        }
    }

    public bool RemoveTower(Vector3Int cellPos)
    {
        if (!placedTowers.TryGetValue(cellPos, out Tower tower))
        {
            return false;
        }

        placedTowers.Remove(cellPos);
        Destroy(tower.gameObject);
        return true;
    }

    public bool HasTowerAt(Vector3Int cellPos)
    {
        return placedTowers.ContainsKey(cellPos);
    }

    public Tower GetTowerAt(Vector3Int cellPos)
    {
        if (placedTowers.TryGetValue(cellPos, out Tower tower))
            return tower;

        return null;
    }
}