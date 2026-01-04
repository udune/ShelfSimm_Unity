using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Visualization3D
{
    public class Simulation3DEnvironment : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject materialshelfPrefab;
        public GameObject robotPrefab;
        public GameObject floorPrefab;

        [Header("Settings")]
        public float gridScale = 2f;

        private Dictionary<string, Materialshelf3DVisual> materialshelves = new Dictionary<string, Materialshelf3DVisual>();
        private Robot3DVisual robot;
        private Light directionalLight;
        private Light robotPointLight;

        public void Initialize()
        {
            Debug.Log("[Simulation3DEnvironment] Initializing 3D environment...");

            SetupLighting();
            CreateFloor();
            CreateMaterialshelves();
            CreateRobot();
            AttachRobotLight();

            Debug.Log($"[Simulation3DEnvironment] Created {materialshelves.Count} materialshelves and 1 robot with lighting");
        }

        private void CreateFloor()
        {
            var floor = Instantiate(floorPrefab, Vector3.zero, Quaternion.identity, transform);
            floor.layer = LayerMask.NameToLayer("Simulation3D");
            floor.name = "Floor";
        }

        private void CreateMaterialshelves()
        {
            foreach (var cell in ConfigManager.Instance.CellsLayout.cells)
            {
                Vector3 pos = GridToWorld(cell.X, cell.Y);
                var shelf = Instantiate(materialshelfPrefab, pos, Quaternion.identity, transform);
                shelf.layer = LayerMask.NameToLayer("Simulation3D");
                shelf.name = $"Materialshelf_{cell.code}";

                var visual = shelf.GetComponent<Materialshelf3DVisual>();
                visual.Initialize(cell.code);
                materialshelves[cell.code] = visual;
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

        private void SetupLighting()
        {
            // Main Directional Light (Sun-like)
            var lightObj = new GameObject("DirectionalLight_3D");
            lightObj.transform.SetParent(transform);
            lightObj.layer = LayerMask.NameToLayer("Simulation3D");

            directionalLight = lightObj.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.color = new Color(1f, 0.98f, 0.95f); // Warm white
            directionalLight.intensity = 1.2f;
            directionalLight.shadows = LightShadows.Soft;
            directionalLight.shadowStrength = 0.6f;
            directionalLight.shadowBias = 0.05f;
            directionalLight.shadowNormalBias = 0.4f;

            // Light angle: from above and slightly to the side
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ambient light settings
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.32f, 0.35f); // Cool ambient
            RenderSettings.ambientIntensity = 0.8f;

            Debug.Log("[Simulation3DEnvironment] Lighting setup complete");
        }

        private void AttachRobotLight()
        {
            if (robot == null) return;

            // Point Light attached to robot
            var lightObj = new GameObject("RobotLight");
            lightObj.transform.SetParent(robot.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.5f, 0f); // Slightly above robot
            lightObj.layer = LayerMask.NameToLayer("Simulation3D");

            robotPointLight = lightObj.AddComponent<Light>();
            robotPointLight.type = LightType.Point;
            robotPointLight.color = new Color(0f, 0.6f, 1f); // Cyan to match robot color
            robotPointLight.intensity = 2.5f;
            robotPointLight.range = 6f;
            robotPointLight.shadows = LightShadows.Soft;
            robotPointLight.shadowStrength = 0.4f;

            Debug.Log("[Simulation3DEnvironment] Robot light attached");
        }

        public void Cleanup()
        {
            Debug.Log("[Simulation3DEnvironment] Cleaning up 3D environment...");

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            materialshelves.Clear();
            robot = null;
            directionalLight = null;
            robotPointLight = null;
        }
    }
}