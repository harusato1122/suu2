using UnityEngine;
using System.Collections.Generic;

public class suusuuGameController : MonoBehaviour
{
    public GameObject[] tsumPrefabs;
    public Transform spawnPoint;
    public float spawnInterval = 1.5f;
    public float maxConnectDistance = 1.5f;
    public int MaxMino = 40;

    private float timer;
    private List<GameObject> tsumList = new List<GameObject>();
    private List<GameObject> selectedTsums = new List<GameObject>();
    private Camera mainCamera;
    private LineRenderer lineRenderer;

    void Start()
    {
        mainCamera = Camera.main;
        InitLineRenderer();
        for (int i = 0; i < MaxMino; i++) SpawnTsum();
    }

    void InitLineRenderer()
    {
        GameObject lineObj = new GameObject("LineRenderer");
        lineObj.transform.SetParent(this.transform);
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.sortingOrder = 10;
    }

    void Update()
    {
        timer += Time.deltaTime;
        /*if (timer >= spawnInterval)
        {
            SpawnTsum();
            timer = 0f;
        }*/

        HandleMouseInput();
        UpdateLine();
    }

    void SpawnTsum()
    {
        if (tsumPrefabs.Length == 0) return;
        int index = Random.Range(0, tsumPrefabs.Length);
        GameObject tsum = Instantiate(tsumPrefabs[index], spawnPoint.position, Quaternion.identity);
        tsumList.Add(tsum);

        Rigidbody2D rb = tsum.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float xForce = Random.Range(-0.5f, 0.5f);
            float yForce = Random.Range(-1.0f, -2.0f);
            rb.AddForce(new Vector2(xForce, yForce), ForceMode2D.Impulse);
        }
    }

    void HandleMouseInput()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (Input.GetMouseButtonDown(0))
        {
            ClearSelection();
        }
        else if (Input.GetMouseButton(0))
        {
            if (hit != null && hit.CompareTag("Mino"))
            {
                GameObject tsum = hit.gameObject;
                if (!selectedTsums.Contains(tsum))
                {

                    if (selectedTsums.Count == 0 || IsWithinDistance(tsum, selectedTsums[selectedTsums.Count - 1]))
                    {
                        selectedTsums.Add(tsum);
                        HighlightTsum(tsum, true);
                    }
                }
                else if (selectedTsums.Count >= 2 && tsum == selectedTsums[selectedTsums.Count - 2])
                {
                    GameObject last = selectedTsums[selectedTsums.Count - 1];
                    HighlightTsum(last, false);
                    selectedTsums.RemoveAt(selectedTsums.Count - 1);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectedTsums.Count >= 3)
            {
                foreach (var tsum in selectedTsums)
                {
                    tsumList.Remove(tsum);
                    Destroy(tsum);
                }
                for (int i = 0; i < selectedTsums.Count; i++) SpawnTsum();
            }
            else
            {
                foreach (var tsum in selectedTsums)
                {
                    HighlightTsum(tsum, false);
                }
            }
            selectedTsums.Clear();
        }
    }

    void UpdateLine()
    {
        if (selectedTsums.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = selectedTsums.Count;
        for (int i = 0; i < selectedTsums.Count; i++)
        {
            lineRenderer.SetPosition(i, selectedTsums[i].transform.position);
        }
    }

    bool IsWithinDistance(GameObject a, GameObject b)
    {
        float dist = Vector2.Distance(a.transform.position, b.transform.position);
        return dist <= maxConnectDistance;
    }

    void HighlightTsum(GameObject tsum, bool highlight)
    {
        SpriteRenderer sr = tsum.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = highlight ? new Color(0.3f, 0.3f, 0.3f) : Color.white;
        }
    }

    void ClearSelection()
    {
        foreach (var tsum in selectedTsums)
        {
            HighlightTsum(tsum, false);
        }
        selectedTsums.Clear();
    }
}