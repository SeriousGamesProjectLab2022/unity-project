using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShapeMinigame : Minigame
{
    private ShapeMinigameSolution solution;
    private ScenarioManager scenarioManager;
    private List<MinigameShape> shapes = new List<MinigameShape>();
    [SerializeField] private MinigameShapeController minigameShapeController;

    public static ShapeMinigameSolutions GenerateConfiguration(List<MinigameShape> shapePrefabs)
    {
        var grid = Instantiate(new GameObject(), position: new Vector3(0, 0, -1), rotation: Quaternion.identity);
        grid.AddComponent<Grid>();

        // Shuffle prefabs
        var shuffledShapePrefabs = shapePrefabs.OrderBy(x => Random.value).ToList();
        var centerShape = shuffledShapePrefabs[0];

        var combinedShape = Instantiate(centerShape, parent: grid.transform);
        var shapeIndices = new List<int>(shapePrefabs.Count);
        var relativePositions = new List<Vector2>(shapePrefabs.Count);
        for (int i = 0; i < shuffledShapePrefabs.Count; i++)
        {
            shapeIndices.Add(i);
            relativePositions.Add(Vector2.zero);
        }

        foreach (var currentShape in shuffledShapePrefabs.Skip(1))
        {
            var currentShapeClone = Instantiate(currentShape, parent: grid.transform);
            currentShapeClone.Start();
            var overlapping =
                currentShapeClone.DoesCollideWithOtherShapeWhenMovingTo(currentShapeClone.transform.position);
            while (overlapping)
            {
                var randomCoordinateIndex = Random.Range(0, 2);
                var randomOffset = Vector2.zero;
                randomOffset[randomCoordinateIndex] = Random.Range(-1, 2);
                var newPosition = currentShapeClone.transform.position + (Vector3) randomOffset;
                overlapping = currentShapeClone.DoesCollideWithOtherShapeWhenMovingTo(newPosition);
                
                currentShapeClone.gameObject.SetActive(false);
                currentShapeClone.transform.position = newPosition;
                currentShapeClone.gameObject.SetActive(true);
            }
            var index = shapePrefabs.IndexOf(currentShape);
            relativePositions[index] = currentShapeClone.transform.position;
            combinedShape.gameObject.SetActive(false);
            Merge(
                currentShapeClone.GetComponent<Tilemap>(),
                into: combinedShape.GetComponent<Tilemap>(),
                offset: new Vector2Int(
                    (int) currentShapeClone.transform.position.x,
                    (int) currentShapeClone.transform.position.y
                )
            );
            combinedShape.gameObject.SetActive(true);

        }
        // Compute relative positions between all shapes
        for (int i = relativePositions.Count - 1; i >= 0; i--)
        {
            relativePositions[i] -= relativePositions[0];
        }
        // Clean-up the generation process
        Destroy(grid.gameObject);
        return new ShapeMinigameSolutions() {
            solutions = // new [] {
                new ShapeMinigameSolution() {
                    relativePositions = relativePositions.ToArray(),
                    shapeIndices = shapeIndices.ToArray(),
                // }
            }
        };
    }

    private static void Merge(Tilemap sourceTilemap, Tilemap into, Vector2Int offset)
    {
        var targetTilemap = into;

        foreach (var cellPosition in sourceTilemap.cellBounds.allPositionsWithin)
        {
            var currentTile = sourceTilemap.GetTile(cellPosition);
            var currentTargetTile = targetTilemap.GetTile(cellPosition + (Vector3Int) offset);
            if (currentTile == null || currentTargetTile != null)
            {
                continue;
            }

            targetTilemap.SetTile(cellPosition + (Vector3Int) offset, currentTile);
        }
    }

    public override void GetSolution()
    {
        solution = scenarioManager.minigameSolutions.shapeMinigameSolutions.solutions;
    }

    public override void CheckSolution()
    {
        var currentRelativePositions = new List<Vector2>();
        foreach (var shape in shapes)
        {
            currentRelativePositions.Add(
                shape.transform.localPosition - shapes[0].transform.localPosition
            );
        }

        var solved = currentRelativePositions.SequenceEqual(solution.relativePositions);
        EmitEndedEvent(solved);
    }

    private void PlaceShapePrefabs()
    {
        if (solution == null)
        {
            throw new UnityException("Shape minigame does not have a solution");
        }

        var cumulativeOffset = 0;
        foreach (var index in solution.shapeIndices)
        {
            var shapePrefab = scenarioManager.minigameShapePrefabs[index];
            var shape = Instantiate(shapePrefab, parent: minigameShapeController.transform);
            shape.transform.localPosition = new Vector3(cumulativeOffset, 0, 0);
            shapes.Add(shape);

            var shapeTilemap = shape.GetComponent<Tilemap>();
            shapeTilemap.CompressBounds();

            cumulativeOffset += (shapeTilemap.cellBounds.xMax - shapeTilemap.cellBounds.xMin);
        }

        minigameShapeController.SetShapes(shapes);
    }

    void Awake()
    {
        scenarioManager = FindObjectOfType<ScenarioManager>();
    }

    protected override void Start()
    {
        base.Start();
        minigameShapeController.SetCanMoveShapes(takeInput);
        PlaceShapePrefabs();
    }

    protected override void Update()
    {
        base.Update();
        minigameShapeController.SetCanMoveShapes(takeInput);
    }
}
