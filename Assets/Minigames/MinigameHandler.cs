using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

public class MinigameHandler : MonoBehaviour
{
    [SerializeField] private List<Minigame> minigamePrefabs;
    private List<Vector3Int> checkpointPositions;
    private int currentMinigameIndex = 0;
    [SerializeField] private Checkpoint checkpointPrefab;
    [SerializeField] private OverworldGoal goalPrefab;
    [SerializeField] private Transform worldOrigin;
    public delegate void PlayerLostMinigame(float damageAmount);
    public event PlayerLostMinigame OnPlayerLostMinigame = delegate { };
    private Checkpoint currentCheckpoint;
    private OverworldGoal currentOverworldGoal;

    void Start()
    {
        checkpointPositions = GameManager.Singleton.scenarioManager.terrain.checkpointList;
        SpawnCheckpoint(checkpointPositions[currentMinigameIndex + 1]);
    }

    public void SpawnMinigame()
    {
        var scenarioManager = GameObject.FindObjectOfType<ScenarioManager>();
        var minigame = Instantiate(
            minigamePrefabs[currentMinigameIndex],
            parent: this.transform
        );
        minigame.transform.localPosition = new Vector3Int(8, 0, 0);

        minigame.OnMinigameOver += (bool solved) =>
        {
            GameObject.FindObjectOfType<PilotManager>().score += 100f;
            Destroy(minigame.gameObject);
            if (!solved)
            {
                GameObject.FindObjectOfType<PilotManager>().score -= 100f;
                OnPlayerLostMinigame(damageAmount: 3.0f);
            }

            if (currentCheckpoint != null)
            {
                currentCheckpoint.Activate();
            }
            if (currentOverworldGoal != null)
            {
                currentOverworldGoal.Activate();
            }
        };

        currentMinigameIndex++;
        if (currentMinigameIndex < minigamePrefabs.Count) {
            SpawnCheckpoint(checkpointPositions[currentMinigameIndex + 1]);
        } else {
            SpawnGoal(checkpointPositions[currentMinigameIndex + 1]);
        }
    }

    public void SpawnCheckpoint(Vector3Int newPosition) 
    {
        currentCheckpoint = Instantiate(
            checkpointPrefab,
            parent: worldOrigin.transform
        );
        currentCheckpoint.transform.localPosition = newPosition;
        currentCheckpoint.OnCheckpointReached += SpawnMinigame;
        if (currentMinigameIndex != 0)
        {
            currentCheckpoint.Deactivate();
        }
    }

    public void SpawnGoal(Vector3Int newPosition) 
    {
        currentOverworldGoal = Instantiate(
            goalPrefab,
            parent: worldOrigin.transform
        );
        currentOverworldGoal.transform.localPosition = newPosition;
        currentOverworldGoal.OnCollidedWithSpaceship += () =>
        {
            GameObject.FindObjectOfType<PilotManager>().EndGame(gameEndedSuccessfully: true);
        };
        currentOverworldGoal.Deactivate();
    }
}
