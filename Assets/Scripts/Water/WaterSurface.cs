using UnityEngine;

namespace KayakSimulator.Water
{
    /// <summary>
    /// Manages the visual water surface mesh:
    ///   • Creates a procedural plane mesh at startup (configurable resolution).
    ///   • Each frame updates the mesh vertices on the CPU to match the
    ///     GerstnerWaveSystem output (used for physics accuracy; the GPU
    ///     shader runs the same computation for visuals).
    ///   • Optionally tiles an infinite-ocean illusion by repositioning
    ///     the mesh under the camera.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class WaterSurface : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Mesh Generation")]
        [Tooltip("World size of the water plane on each axis (metres).")]
        [SerializeField] private float   planeSize         = 200f;

        [Tooltip("Number of vertex subdivisions per axis. Higher = smoother waves but more CPU/GPU cost.")]
        [SerializeField] private int     resolution        = 128;

        [Header("Infinite Ocean")]
        [Tooltip("If true, the water plane repositions to stay under the camera (seamless tiling illusion).")]
        [SerializeField] private bool    infiniteOcean     = true;
        [SerializeField] private Transform cameraTransform;

        [Header("CPU Mesh Update")]
        [Tooltip("Update the CPU mesh each frame so buoyancy samples are exactly accurate. " +
                 "Disable if you rely solely on GerstnerWaveSystem for physics.")]
        [SerializeField] private bool    updateMeshEachFrame = true;

        // ---------------------------------------------------------------
        // Private
        // ---------------------------------------------------------------
        private Mesh             _mesh;
        private Vector3[]        _baseVertices;
        private Vector3[]        _vertices;
        private Physics.GerstnerWaveSystem _waveSystem;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Awake()
        {
            BuildMesh();
        }

        private void Start()
        {
            _waveSystem = FindAnyObjectByType<Physics.GerstnerWaveSystem>();

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (infiniteOcean && cameraTransform != null)
                RepositionUnderCamera();

            if (updateMeshEachFrame && _waveSystem != null)
                UpdateVertices();
        }

        // ---------------------------------------------------------------
        // Mesh construction
        // ---------------------------------------------------------------
        private void BuildMesh()
        {
            _mesh = new Mesh { name = "OceanSurface" };
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // support > 65k verts

            int   vertsPerSide = resolution + 1;
            int   vertCount    = vertsPerSide * vertsPerSide;
            float step         = planeSize / resolution;
            float halfSize     = planeSize * 0.5f;

            _baseVertices = new Vector3[vertCount];
            var uvs        = new Vector2[vertCount];

            for (int z = 0; z <= resolution; z++)
            for (int x = 0; x <= resolution; x++)
            {
                int idx = z * vertsPerSide + x;
                _baseVertices[idx] = new Vector3(x * step - halfSize, 0f, z * step - halfSize);
                uvs[idx]           = new Vector2((float)x / resolution, (float)z / resolution);
            }

            _vertices = (Vector3[])_baseVertices.Clone();

            // Triangles
            int[] tris = new int[resolution * resolution * 6];
            int   t    = 0;
            for (int z = 0; z < resolution; z++)
            for (int x = 0; x < resolution; x++)
            {
                int bl = z * vertsPerSide + x;
                int br = bl + 1;
                int tl = bl + vertsPerSide;
                int tr = tl + 1;

                tris[t++] = bl; tris[t++] = tl; tris[t++] = tr;
                tris[t++] = bl; tris[t++] = tr; tris[t++] = br;
            }

            _mesh.SetVertices(_baseVertices);
            _mesh.SetUVs(0, uvs);
            _mesh.SetTriangles(tris, 0);
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        private void UpdateVertices()
        {
            for (int i = 0; i < _baseVertices.Length; i++)
            {
                Vector3 worldBase = transform.TransformPoint(_baseVertices[i]);
                float   y = _waveSystem.GetWaveHeight(worldBase.x, worldBase.z);
                _vertices[i] = new Vector3(_baseVertices[i].x, y, _baseVertices[i].z);
            }

            _mesh.SetVertices(_vertices);
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
        }

        private void RepositionUnderCamera()
        {
            Vector3 camPos  = cameraTransform.position;
            transform.position = new Vector3(
                Mathf.Round(camPos.x),
                transform.position.y,
                Mathf.Round(camPos.z));
        }
    }
}
