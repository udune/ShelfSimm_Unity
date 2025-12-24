using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Visualization3D
{
    public class Simulation3DEnvironment : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject bookshelfPrefab;
        public GameObject robotPrefab;
        public GameObject floorPrefab;

        [Header("Settings")]
        public float gridScale = 2f;

        private Dictionary<string, Bookshelf3DVisual> bookshelves = new Dictionary<string, Bookshelf3DVisual>();
        private Robot3DVisual robot;

        public void Initialize()
        {
            Debug.Log("[Simulation3DEnvironment] Initializing 3D environment...");

            CreateFloor();
            CreateBookshelves();
            CreateRobot();

            Debug.Log($"[Simulation3DEnvironment] Created {bookshelves.Count} bookshelves and 1 robot");
        }

        private void CreateFloor()
        {
            var floor = Instantiate(floorPrefab, Vector3.zero, Quaternion.identity, transform);
            floor.layer = LayerMask.NameToLayer("Simulation3D");
            floor.name = "Floor";
        }

        private void CreateBookshelves()
        {
            foreach (var cell in ConfigManager.Instance.CellsLayout.cells)
            {
                Vector3 pos = GridToWorld(cell.X, cell.Y);
                var shelf = Instantiate(bookshelfPrefab, pos, Quaternion.identity, transform);
                shelf.layer = LayerMask.NameToLayer("Simulation3D");
                shelf.name = $"Bookshelf_{cell.code}";

                var visual = shelf.GetComponent<Bookshelf3DVisual>();
                visual.Initialize(cell.code);
                bookshelves[cell.code] = visual;
            }
        }

        private void CreateRobot()
        {
            var warehouse = ConfigManager.Instance.CellsLayout.warehouse;
            Vector3 pos = GridToWorld(warehouse.x, warehouse.y);

            var robotObj = Instantiate(robotPrefab, pos, Quaternion.identity, transform);
            robotObj.layer = LayerMask.NameToLayer("Simulation3D");
            robotObj.name = "Robot";

            robot = robotObj.GetComponent<Robot3DVisual>();
        }

        private Vector3 GridToWorld(int gridX, int gridY)
        {
            return new Vector3(gridX * gridScale, 0, gridY * gridScale);
        }

        public Robot3DVisual GetRobot()
        {
            return robot;
        }

        public void Cleanup()
        {
            Debug.Log("[Simulation3DEnvironment] Cleaning up 3D environment...");

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            bookshelves.Clear();
            robot = null;
        }
    }
}