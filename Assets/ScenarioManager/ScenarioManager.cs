using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioManager : MonoBehaviour
{
    public List<MinigameShape> minigameShapePrefabs;
    public MinigameSolutions minigameSolutions;

    public List<Slider> frequenzMinigameSliders;
    public void generateMinigameSolutions()
    {
        minigameSolutions.shapeMinigameSolutions = ShapeMinigame.GenerateConfiguration(minigameShapePrefabs);
        minigameSolutions.frequencyMinigameSolutions = FrequencyMinigame.GenerateSolutionForFrequenceMinigame(frequenzMinigameSliders);
        minigameSolutions.symbolMinigameSolutions = SymbolMinigame.GenerateSolutionForSymbolMinigame(new System.Random(), 18);

    }

    public void generateScenario()
    {
        generateMinigameSolutions();
    }
}
