using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    public class Cell
    {
        // 자재 재고 정보를 저장하는 내부 클래스
        private class MaterialStock
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
        }

        public string CellCode { get; private set; }
        public int WidthMm { get; private set; }
        public int HeightMm { get; private set; }

        // 여러 종류의 자재를 저장 (materialId -> MaterialStock)
        private Dictionary<string, MaterialStock> materials = new Dictionary<string, MaterialStock>();

        // 하위 호환성을 위한 속성들 (첫 번째 자재 정보 반환)
        public string StoredMaterialId => materials.Keys.FirstOrDefault();
        public string StoredMaterialName => materials.Values.FirstOrDefault()?.Name;
        public int CurrentStock => materials.Values.Sum(m => m.Quantity);
        public int MaxCapacity => 100; // Simplified: fixed capacity per cell

        public bool IsEmpty => materials.Count == 0;
        public bool IsFull => CurrentStock >= MaxCapacity;

        public Cell(string cellCode, int widthMm, int heightMm)
        {
            CellCode = cellCode;
            WidthMm = widthMm;
            HeightMm = heightMm;
        }

        public bool CanAdd(MaterialData material, int quantity, out ErrorCode errorCode)
        {
            errorCode = ErrorCode.NONE;

            if (quantity <= 0)
            {
                errorCode = ErrorCode.INVALID_VALUE;
                return false;
            }

            // Simplified capacity check: total quantity limit
            if (CurrentStock + quantity > MaxCapacity)
            {
                errorCode = ErrorCode.CAPACITY_FULL;
                return false;
            }

            return true;
        }

        public void AddMaterial(MaterialData material, int quantity)
        {
            if (materials.ContainsKey(material.Id))
            {
                // 이미 저장된 자재면 수량만 증가
                materials[material.Id].Quantity += quantity;
            }
            else
            {
                // 새로운 자재면 추가
                materials[material.Id] = new MaterialStock
                {
                    Name = material.Name,
                    Quantity = quantity
                };
            }
        }

        public bool CanRemove(MaterialData material, int quantity, out ErrorCode errorCode)
        {
            errorCode = ErrorCode.NONE;

            if (quantity <= 0)
            {
                errorCode = ErrorCode.INVALID_VALUE;
                return false;
            }

            if (!materials.ContainsKey(material.Id))
            {
                errorCode = ErrorCode.MATERIAL_MISMATCH;
                return false;
            }

            if (materials[material.Id].Quantity < quantity)
            {
                errorCode = ErrorCode.INSUFFICIENT_STOCK;
                return false;
            }

            return true;
        }

        public void RemoveMaterial(MaterialData material, int quantity)
        {
            if (!materials.ContainsKey(material.Id))
            {
                Debug.LogWarning($"Cell {CellCode}에 자재 {material.Name}이 없습니다.");
                return;
            }

            materials[material.Id].Quantity -= quantity;

            // 재고가 0이 되면 제거
            if (materials[material.Id].Quantity <= 0)
            {
                materials.Remove(material.Id);
            }
        }

        // Cell에 저장된 모든 자재 정보 반환 (디버깅/UI용)
        public List<(string materialId, string name, int quantity)> GetAllMaterials()
        {
            return materials.Select(kvp => (kvp.Key, kvp.Value.Name, kvp.Value.Quantity)).ToList();
        }

        // 특정 자재의 재고 조회
        public int GetMaterialQuantity(string materialId)
        {
            return materials.ContainsKey(materialId) ? materials[materialId].Quantity : 0;
        }
    }
}
