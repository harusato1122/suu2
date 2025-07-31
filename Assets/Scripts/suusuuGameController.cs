using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System;
using System.Data;

public class suusuuGameController : MonoBehaviour
{
    public GameObject[] tsumPrefabs;
    public Transform spawnPoint;
    public float fiverInterval = 1.0f;
    public float maxConnectDistance = 1.5f;
    public int MaxMino = 40;
    public List<string> valueList = new List<string>();
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI tsumValueText;
    public TextMeshProUGUI timerText;
    public GameObject retryButton;

    private float Fivertimer;
    private float timer = 60f;
    private List<GameObject> tsumList = new List<GameObject>();
    private List<GameObject> selectedTsums = new List<GameObject>();
    private Camera mainCamera;
    private LineRenderer lineRenderer;
    private int fiverPoint = 0;
    private int fiverPointplus = 10;
    private int fiverPointminus = -2;
    private int fiverPointMax = 100;
    private bool isFiver = false;
    public Slider fiverSlider;
    public double Score = 0;
    public bool gameend = false;

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
        if (gameend)
        {
            return;
        }
        Fivertimer += Time.deltaTime;
        timer -= Time.deltaTime;
        if (Fivertimer >= fiverInterval && isFiver)
        {
            fiverPoint += fiverPointminus; 
            if( fiverPoint <= 0)
            {
                isFiver = false;
                fiverPoint = 0;
            }
            Fivertimer = 0f;
        }

        HandleMouseInput();
        UpdateLine();
        tsumValueText.text = GetSelectedString();
        fiverSlider.value = (float)fiverPoint / fiverPointMax;
        scoreText.text = "Score: " + Score.ToString("F2");
        timerText.text = timer.ToString("F0");
        if(timer <= 0)
        {
            retryButton.SetActive(true);
            gameend = true;
            timer = 0f;

        }
    }

    void SpawnTsum()
    {
        if (tsumPrefabs.Length == 0) return;
        int index = UnityEngine.Random.Range(0, tsumPrefabs.Length);
        GameObject tsum = Instantiate(tsumPrefabs[index], spawnPoint.position, Quaternion.identity);
        tsumList.Add(tsum);

        int valueIndex = UnityEngine.Random.Range(0, valueList.Count);
        Mino mino = tsum.GetComponent<Mino>();
        if (mino != null)
        {
            mino.SetText(valueList[valueIndex]);
        }

        Rigidbody2D rb = tsum.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float xForce = UnityEngine.Random.Range(-0.5f, 0.5f);
            float yForce = UnityEngine.Random.Range(-1.0f, -2.0f);
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
                Mino mino = tsum.GetComponent<Mino>();
                if (!selectedTsums.Contains(tsum))
                {
                    if ((selectedTsums.Count == 0 && mino.isNumber) || (selectedTsums.Count != 0 && IsWithinDistance(tsum, selectedTsums[selectedTsums.Count - 1]) && (mino.isNumber || selectedTsums[selectedTsums.Count - 1].GetComponent<Mino>().isNumber)))
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
            if (selectedTsums.Count >= 3 && IsValidEquationChain(GetSelectedString(),isFiver))
            {
                foreach (var tsum in selectedTsums)
                {
                    tsumList.Remove(tsum);
                    Destroy(tsum);
                }
                for (int i = 0; i < selectedTsums.Count; i++)
                {
                    SpawnTsum();
                    FiverPlus();
                }
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

    public string GetSelectedString()
    {
        string result = "";
        foreach (var tsum in selectedTsums)
        {
            result += tsum.GetComponent<Mino>().Value;
        }
        return result;
    }

    public bool IsValidEquationChain(string input, bool allowError = false)
    {
        try
        {
            string[] expressions = input.Split('=');
            if (expressions.Length < 2)
                return false;

            List<double> results = new List<double>();
            foreach (string expr in expressions)
            {
                var value = EvaluateExpression(expr);
                results.Add(value);
            }

            double mean = results[0];
            for (int i = 1; i < results.Count; i++)
            {
                double tolerance = allowError ? 10.0 : 0.00001;
                if (Math.Abs(results[i] - results[i - 1]) > tolerance)
                    return false;
                mean += results[i];
            }

            Score += mean / results.Count;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static double EvaluateExpression(string expression)
    {
        var dt = new DataTable();
        var result = dt.Compute(expression, "");
        return Convert.ToDouble(result);
    }

    public void screw_Mino()
    {
        GameObject[] allMino = GameObject.FindGameObjectsWithTag("Mino");
        
        foreach (GameObject minoObj in allMino)
        {
            Rigidbody2D rb = minoObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float xForce = UnityEngine.Random.Range(-5f, 5f);
                float yForce = UnityEngine.Random.Range(0f, 4f);
                rb.AddForce(new Vector2(xForce, yForce), ForceMode2D.Impulse);
            }
        }
    }

    public void FiverPlus()
    {
        if (isFiver) return;

        fiverPoint += fiverPointplus;
        if(fiverPoint >= fiverPointMax)
        {
            isFiver = true;
            Fivertimer = 0f;
            fiverPoint = fiverPointMax;
        }
    }

    public void OnRetryButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}