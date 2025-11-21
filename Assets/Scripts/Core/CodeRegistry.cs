using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class CodeRegistry : MonoBehaviour
    {
        [Header("등록된 칸 코드 목록")]
        [SerializeField]
        private string[] registeredCodes = { "D20", "A15", "B03", "C10", "E05" };

        private HashSet<string> codeSet;

        public void Start()
        {
            Init();
        }

        private void Init()
        {
            codeSet = new HashSet<string>();

            foreach (string code in registeredCodes)
            {
                string normalizedCode = code.ToUpper().Trim();
                codeSet.Add(normalizedCode);
            }
        }

        public bool IsValidCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            string normalizedCode = code.ToUpper().Trim();
            return codeSet.Contains(normalizedCode);
        }

        public HashSet<string> GetAllCodes()
        {
            return new HashSet<string>(codeSet);
        }

        public void AddCode(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                string normalizedCode = code.ToUpper().Trim();
                codeSet.Add(normalizedCode);
            }
        }
    }
}