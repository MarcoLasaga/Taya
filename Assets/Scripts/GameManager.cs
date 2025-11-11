using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public List<GameObject> friends;

    [Header("UI")]
    public TextMeshProUGUI gameMessageText; // "Press Y to Start"
    public TextMeshProUGUI tayaNameText;    // shows "Taya: <name>"
    public TextMeshProUGUI timerText;       // countdown timer

    [Header("Game Settings")]
    public float gameDuration = 300f; // 5 minutes = 300 seconds

    [HideInInspector] public bool gameRunning;
    [HideInInspector] public GameObject currentTaya;

    private float remainingTime;

    void Start()
    {
        gameRunning = false;
        remainingTime = gameDuration;

        gameMessageText.gameObject.SetActive(true);
        tayaNameText.text = "Taya: None";
        timerText.text = "Time: 05:00";

        PickInitialTaya();
    }

    void Update()
    {
        if (!gameRunning && Input.GetKeyDown(KeyCode.Y))
        {
            StartGame();
        }

        if (gameRunning)
        {
            UpdateTimer();
        }
    }

    void StartGame()
    {
        gameRunning = true;
        gameMessageText.gameObject.SetActive(false);
        remainingTime = gameDuration;
        Debug.Log("Game Started!");
    }

    void UpdateTimer()
    {
        if (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime < 0f) remainingTime = 0f;

            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
        else
        {
            EndGame();
        }
    }

    void EndGame()
    {
        gameRunning = false;
        timerText.text = "Time: 00:00";
        gameMessageText.text = "Game Over! You survived!";
        gameMessageText.gameObject.SetActive(true);

        if (currentTaya != null)
        {
            Destroy(currentTaya.GetComponent<TayaAI>());
        }
    }

    void PickInitialTaya()
    {
        if (friends.Count == 0) return;

        int index = Random.Range(0, friends.Count);
        currentTaya = friends[index];

        var tayaAI = currentTaya.AddComponent<TayaAI>();
        tayaAI.manager = this;

        currentTaya.GetComponent<Renderer>().material.color = Color.black;
        tayaNameText.text = $"Taya: {currentTaya.name}";
    }

    public void SwapTaya(GameObject newTaya)
    {
        if (currentTaya != null)
        {
            Destroy(currentTaya.GetComponent<TayaAI>());
            currentTaya.GetComponent<Renderer>().material.color = Color.white;
        }

        currentTaya = newTaya;

        var tayaAI = newTaya.GetComponent<TayaAI>();
        if (tayaAI == null) tayaAI = newTaya.AddComponent<TayaAI>();
        tayaAI.manager = this;

        newTaya.GetComponent<Renderer>().material.color = Color.black;
        tayaNameText.text = $"Taya: {newTaya.name}";

        Debug.Log($"{newTaya.name} is now the new Taya!");
    }
}
