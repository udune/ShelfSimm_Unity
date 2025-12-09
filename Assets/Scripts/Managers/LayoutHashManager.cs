using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Core;
using Data;
using UnityEngine;

namespace Managers
{
    public class LayoutHashManager : MonoBehaviour
    {
        [SerializeField] private PathCache pathCache;

        private string lastComputedHash = "";

        public void UpdateLayoutHash(CellsLayoutSO layout)
        {
            if (layout == null)
            {
                Debug.LogWarning("[LayoutHashManager] Layout is null, cannot compute hash.");
                return;
            }

            string newHash = ComputeLayoutHash(layout);

            if (string.IsNullOrEmpty(layout.layout_hash) || layout.layout_hash != newHash)
            {
                layout.layout_hash = newHash;
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(layout);
                #endif
            }

            if (pathCache != null)
            {
                pathCache.SetLayoutHash(newHash);
            }

            lastComputedHash = newHash;
        }

        private string ComputeLayoutHash(CellsLayoutSO layout)
        {
            StringBuilder sb = new StringBuilder();

            var sortedCells = new List<CellDef>(layout.cells);
            sortedCells.Sort((a, b) => string.Compare(a.code, b.code, System.StringComparison.Ordinal));

            foreach (var cell in sortedCells)
            {
                sb.Append($"{cell.code},{cell.X},{cell.Y};");
                sb.Append($"{cell.width},{cell.height};");
                sb.Append($"{cell.orientation};");
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder result = new StringBuilder();
                for (int i = 0; i < 8; i++)
                {
                    result.Append(hash[i].ToString("x2"));
                }

                return $"sha256:{result}";
            }
        }

        public string GetLastComputedHash()
        {
            return lastComputedHash;
        }
    }
}
