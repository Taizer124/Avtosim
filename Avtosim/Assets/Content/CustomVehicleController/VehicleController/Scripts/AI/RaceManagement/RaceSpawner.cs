using UnityEngine;

namespace Assets.VehicleController
{
    public class RaceSpawner : MonoBehaviour
    {
        [SerializeField]
        private RaceManager _raceManager;

        public SpawnConditions SpawnCondition;
        public bool BeginCountdownAfterSpawn = true;
        public bool SpawnPlayer = true;
        [Tooltip("Spawn only 1 instance for every vehicle prefab from RacerAIVehiclePrefabArray.")]
        public bool UniqueOnly = true;
        public bool RandomizePlayerPos = true;
        [Min(0)]
        public int PlayerPosIndex;
        private int _playerStartIndex;
        public GameObject PlayerVehiclePrefab;
        public GameObject[] RacerAIVehiclePrefabArray;

        private GameObject[] _spawnedVehicles;

        [SerializeField, Separator]
        private Transform StartPosition;
        [Min(1)]
        public int Rows = 1;
        [Min(1)]
        public int Columns = 1;
        [Min(1)]
        public float HorizontalDistanceBetweenSpawnPoints = 3;
        [Min(1)]
        public float VerticalDistanceBetweenSpawnPoints = 3;

        private void OnValidate()
        {
            if (Rows < 1)
                Rows = 1;
            if(Columns < 1)
                Columns = 1;
            ClampPlayerPosIndex();
        }

        private void ClampPlayerPosIndex()
        {
            if (UniqueOnly)
            {
                if (PlayerPosIndex >= RacerAIVehiclePrefabArray.Length + 1)
                    PlayerPosIndex = RacerAIVehiclePrefabArray.Length;
            }
            else
            {
                if (PlayerPosIndex >= Rows * Columns)
                    PlayerPosIndex = Rows * Columns - 1;
            }

            if(PlayerPosIndex < 0)
                PlayerPosIndex = 0;
        }


        private void Awake()
        {
            if (SpawnCondition == SpawnConditions.OnAwake)
                Spawn();
        }

        public void Spawn()
        {
            ClampPlayerPosIndex();

            if (UniqueOnly)
                _spawnedVehicles = new GameObject[RacerAIVehiclePrefabArray.Length + (SpawnPlayer ? 1 : 0)];
            else
                _spawnedVehicles = new GameObject[Rows * Columns];

            if(SpawnPlayer)
                InstantiatePlayer();

            if (UniqueOnly)
                InstantiateUnique();
            else
                InstantiateRandom();

            PositionVehicles();

            if(BeginCountdownAfterSpawn)
                _raceManager.BeginCountdown();
        }

        private void InstantiateUnique()
        {
            int size = _spawnedVehicles.Length;
            int index = 0;
            for (int i = 0; i < size;i++)
            {
                if (SpawnPlayer && i == _playerStartIndex)
                    continue;

                _spawnedVehicles[i] = Instantiate(RacerAIVehiclePrefabArray[index]);
                index++;
            }
        }

        private void InstantiateRandom()
        {
            int size = _spawnedVehicles.Length;

            for (int i = 0; i < size; i++)
            {
                if (SpawnPlayer && i == _playerStartIndex)
                    continue;

                _spawnedVehicles[i] = Instantiate(RacerAIVehiclePrefabArray[Random.Range(0, RacerAIVehiclePrefabArray.Length)]);
            }
        }

        private void InstantiatePlayer()
        {
            if (RandomizePlayerPos)
                _playerStartIndex = Random.Range(0, _spawnedVehicles.Length - 1);
            else
                _playerStartIndex = PlayerPosIndex;

            _spawnedVehicles[_playerStartIndex] = Instantiate(PlayerVehiclePrefab);
        }

        private void PositionVehicles()
        {
            int index = 0;
            int size = _spawnedVehicles.Length;

            Vector3 right = StartPosition.right;
            Vector3 forward = StartPosition.forward;
            Vector3 pos = StartPosition.position;

            float totalWidth = (Columns - 1) * HorizontalDistanceBetweenSpawnPoints;
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (size == index)
                        break;

                    Vector3 horizontalOffset = right * (HorizontalDistanceBetweenSpawnPoints * col - totalWidth / 2);
                    Vector3 verticalOffset = forward * VerticalDistanceBetweenSpawnPoints * row;
                    _spawnedVehicles[index].transform.position = pos + horizontalOffset + verticalOffset;
                    _spawnedVehicles[index].transform.forward = forward;
                    index++;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 right = StartPosition.right;
            Vector3 forward = StartPosition.forward;
            Vector3 pos = StartPosition.position;
            Vector3 size = new Vector3(4,3,6);

            Vector3[] vertices = {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 0, 1),
        };

            int[] triangles = {
            0, 2, 1, //face front
			0, 3, 2,
            2, 3, 4, //face top
			2, 4, 5,
            1, 2, 5, //face right
			1, 5, 6,
            0, 7, 4, //face left
			0, 4, 3,
            5, 4, 7, //face back
			5, 7, 6,
            0, 6, 7, //face bottom
			0, 1, 6
        };

            float totalWidth = (Columns - 1) * HorizontalDistanceBetweenSpawnPoints;

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Vector3 horizontalOffset = right * (HorizontalDistanceBetweenSpawnPoints * col - totalWidth / 2 - size.x / 2);
                    Vector3 verticalOffset = forward * (VerticalDistanceBetweenSpawnPoints * row  - size.z / 2);
                    Mesh mesh = new Mesh();
                    mesh.vertices = vertices;
                    mesh.triangles = triangles;
                    mesh.Optimize();
                    mesh.RecalculateNormals();
                    Gizmos.color = new Color(0.33f, 0.33f, 1, 0.1f);
                    Gizmos.DrawMesh(mesh, pos + horizontalOffset + verticalOffset, Quaternion.LookRotation(forward), size);
                }
            }
        }

        public enum SpawnConditions
        {
            OnAwake,
            AfterExternalCall,
        }
    }
}
