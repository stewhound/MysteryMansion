﻿using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	// This class makes it easier to specify a range in the inspector.
	[Serializable]
	public class FloatCount
	{
		public float minimum;
		public float maximum;
		
		public FloatCount(float min, float max)
		{
			minimum = min;
			maximum = max;
		}
	}

	[HideInInspector]
	public static GameManager instance = null;

	public Text scoreText;
	public Text timerText;

	[HideInInspector]
	public Button accuseButton;
	public bool accuseMode = false;

	public GameObject[] Npcs;
	public GameObject killer;
	public int initialScore = 1000;
	public float killRadius = 1f;
	public int killPenalty = 100;
	public int falseAccusePenalty = 200;
	public int accusations = 3;
	public float gameTimeInMinutes = 10;

	public FloatCount spawnRangeX = new FloatCount(-4f, 4f);
	public FloatCount spawnRangeZ = new FloatCount(-2.5f, 2.5f);

	private int score;
	private float initializationTimer = 2f;
	private float timer;

	void Awake()
	{
		if(instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);

	}

	public void StartGame()
	{
		foreach(GameObject npc in Npcs)
		{
			Instantiate(npc, new Vector3(Random.Range(spawnRangeX.minimum, spawnRangeX.maximum), 0, 
			                             Random.Range(spawnRangeZ.minimum, spawnRangeZ.maximum)), Quaternion.identity);
		}

		Invoke("SetKiller", initializationTimer);

		score = initialScore;
		timer = gameTimeInMinutes * 60;

		Text[] texts = (Text[])FindObjectsOfType(typeof(Text));
		foreach(Text t in texts)
		{
			if(t.name.StartsWith("Score"))
				scoreText = t;
			else if(t.name.StartsWith("Timer"))
				timerText = t;
		}

		accuseButton = (Button)FindObjectOfType(typeof(Button));
		if(accuseButton.name != "Accuse")
		{
			accuseButton = null;
			Debug.Log("Accuse button not found!");
		}
		else
		{
			accuseButton.onClick.AddListener(() => accuse());
		}

		scoreText.text = "Score: " + score;
		timerText.text = "Time: " + DisplayTime();
	}

	public void accuse()
	{
		accuseMode = !accuseMode;
		if(accuseMode)
			accuseButton.image.color = Color.red;
		else
			accuseButton.image.color = Color.blue;
	}

	void Update()
	{
		if(timerText)
		{
			timer -= Time.deltaTime;
			timerText.text = "Time: " + DisplayTime();
		}
	}

	string DisplayTime()
	{
		string minutes = Mathf.Floor(timer / 60).ToString("00");
		string seconds = (timer % 60).ToString("00");
		if(seconds == "00")
			killer.GetComponent<AIController>().killingMood = true;
		return minutes + ":" + seconds;
	}

	void SetKiller()
	{
		GameObject[] characters = GameObject.FindGameObjectsWithTag("Victim");
		int randomIndex = Random.Range(0, Npcs.Length);
		characters[randomIndex].tag = "Killer";
		Debug.Log(characters[randomIndex].name + " is the killer.");
		killer = characters[randomIndex];
	}

	int GetScore()
	{
		return score;
	}

	void AdjustScore(int adj)
	{
		score += adj;
		scoreText.text = "Score: " + score;
		if(score <= 0)
			GameOver();
	}

	public void killCharacter(GameObject killed)
	{
		killed.GetComponent<AudioSource>().Play();
		if(killed.tag == "Player")
		{
			Destroy(killed);
			GameOver();
		}
		Destroy(killed);
		AdjustScore(-killPenalty);
		Debug.Log("Character killed. New score: " + score);
	}

	void GameOver()
	{
		Debug.Log("Player killed. Game Over! :(");
		Debug.Log("Final Score: " + score);
		LoadLevel("GameOver");
	}

	public void LoadLevel(string levelName)
	{
		if(levelName.StartsWith("Mansion"))
			Invoke("StartGame", initializationTimer);
		Application.LoadLevel(levelName);
	}
	
	public void QuitGame()
	{
		Application.Quit();
	}
}
