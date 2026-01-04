using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using API;

namespace Core
{
    // 자재 데이터를 관리하는 클래스
    public class MaterialRegistry : MonoBehaviour
    {
        public static MaterialRegistry Instance { get; private set; }

        private Dictionary<string, MaterialData> materialDatabase = new Dictionary<string, MaterialData>();
        private List<MaterialData> availableMaterials = new List<MaterialData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public MaterialData GetMaterialById(string materialId)
        {
            if (string.IsNullOrEmpty(materialId))
            {
                return null;
            }

            materialDatabase.TryGetValue(materialId, out MaterialData material);
            return material;
        }

        public MaterialData[] GetAllAvailableMaterials()
        {
            return availableMaterials?.ToArray();
        }

        public MaterialData GetMaterialByName(string name)
        {
            return availableMaterials.Find(x => x.Name.Equals(name));
        }

        public MaterialData GetMaterialByIndex(int index)
        {
            if (index < 0 || index >= availableMaterials.Count)
            {
                return null;
            }

            return availableMaterials[index];
        }

        public MaterialData GetDefaultMaterial()
        {
            if (availableMaterials != null && availableMaterials.Count > 0)
            {
                return availableMaterials[0];
            }
            return null;
        }

        public void LoadMaterialsFromApi(List<MaterialDto> materialDtos)
        {
            if (materialDtos == null || materialDtos.Count == 0)
            {
                return;
            }

            materialDatabase.Clear();
            availableMaterials.Clear();

            foreach (var dto in materialDtos)
            {
                var materialData = new MaterialData(
                    id: dto.id,
                    name: dto.name,
                    vendor: dto.vendor,
                    lotId: dto.lotId,
                    stockQty: dto.stockQty,
                    type: dto.type,
                    expiryDate: dto.expiryDate,
                    category: "일반"
                );

                if (materialDatabase != null)
                {
                    materialDatabase[materialData.Id] = materialData;
                }

                if (availableMaterials != null)
                {
                    availableMaterials.Add(materialData);
                }
            }
        }
    }
}
