using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicTerrain : MonoBehaviour
{
    public Terrain bottomLayer;
    public float percentageDrag = 0.5f; //sand 0.01f; water 0.03f;
    public float minimumDrop = 0f; //sand 0.06f;  water 0.0f;
    public float minimumAmountToCalculate = 0f;
    public float heightPenalty = 0.0f;
    public bool enableSpawner = false;
    public Vector2Int spawner;
    public float spawnAmount = 0.01f;
    

    private TerrainData bottomLayerData;
    private Terrain currentLayer;
    private TerrainData currentLayerData;
    private float[,] updatedHeightMap;
    private MaterialProperties[,] materialMap;
    private MaterialProperties[,] updatedMaterialMap;
    private float heightScale = 600f;

    // Start is called before the first frame update
    void Start()
    {
        
        currentLayer = this.gameObject.GetComponent<Terrain>();
        currentLayerData = currentLayer.terrainData;
        bottomLayerData = bottomLayer.terrainData;
        heightScale = bottomLayerData.heightmapScale.y;


        CreateEmptyMaterialMap();
        GetInitialAmount();
        
        UpdateCurrentHeightMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (enableSpawner) SpawnAtOrigin();
        CalculateMisplacement();
    }

    void LateUpdate() {
        currentLayerData.SetHeights(0, 0, updatedHeightMap);
    }

    public void CalculateMisplacement()
    {
        materialMap = updatedMaterialMap;
        updatedHeightMap = new float[currentLayerData.heightmapResolution, currentLayerData.heightmapResolution];
        for (int x = 0; x < bottomLayerData.heightmapResolution; x++)
        {
            for (int y = 0; y < bottomLayerData.heightmapResolution; y++)
            {
                materialMap[x, y].height = (bottomLayerData.GetHeight(x, y) / heightScale) + heightPenalty;
                if(materialMap[x,y].amount > minimumAmountToCalculate) //only needs to do this calculation if there's a reasonable amount of stuff in the title
                {
                    List<Vector2Int> neightbours = GetNeighbours(x, y);
                    List<Vector2Int> lowerneightbours = new List<Vector2Int>();
                    float diff = materialMap[x, y].amount * percentageDrag;
                    float pointHeight = materialMap[x, y].height + materialMap[x, y].amount;
                    foreach (Vector2Int n in neightbours)
                    {
                        float nHeight = materialMap[n.x, n.y].height + materialMap[n.x, n.y].amount;
                        if ((nHeight + diff) < (pointHeight) &&
                            Mathf.Abs(nHeight - pointHeight) > minimumDrop)
                        {
                            lowerneightbours.Add(n);
                        }
                    };
                    foreach (Vector2Int n in lowerneightbours)
                    {
                        updatedMaterialMap[x, y].amount = materialMap[x, y].amount - diff;
                        updatedMaterialMap[n.x, n.y].amount = materialMap[n.x, n.y].amount + diff;
                    }
                }

                updatedHeightMap[y, x] = materialMap[x, y].amount + materialMap[x, y].height;
            }
        }
        
    }

    public void SpawnAtOrigin ()
    {
        updatedMaterialMap[spawner.x, spawner.y].amount = spawnAmount;
    }

    private List<Vector2Int> GetNeighbours(int x, int y)
    {
        List<Vector2Int> returnValue = new List<Vector2Int>();
        Vector2Int[] steps = {
                    new Vector2Int(-1,-1),
                    new Vector2Int(0,-1),
                    new Vector2Int(1,-1),
                    new Vector2Int(-1,0),
                    new Vector2Int(0,0),
                    new Vector2Int(1,0),
                    new Vector2Int(-1,1),
                    new Vector2Int(0,1),
                    new Vector2Int(1,1)
        };
        foreach (Vector2Int step in steps)
        {
            if ((x + step.x) > 0 && (x + step.x) < currentLayerData.heightmapResolution && //preventing out of bounds
                (y + step.y) > 0 && (y + step.y) < currentLayerData.heightmapResolution)
                returnValue.Add(new Vector2Int(x + step.x, y + step.y));
        }

        return returnValue;
    }

    public void CreateEmptyMaterialMap()
    {
        materialMap = new MaterialProperties[currentLayerData.heightmapResolution, currentLayerData.heightmapResolution];
        updatedMaterialMap = new MaterialProperties[currentLayerData.heightmapResolution, currentLayerData.heightmapResolution];
        for (int x = 0; x < bottomLayerData.heightmapResolution; x++)
        {
            for (int y = 0; y < bottomLayerData.heightmapResolution; y++)
            {
                materialMap[x, y] = new MaterialProperties(0f, 0f);
                updatedMaterialMap[x, y] = new MaterialProperties(0f, 0f);
            }
        }

    }

    public void UpdateCurrentHeightMap()
    {
        
    }

    public void UpdateMaterialMapHeightWithBottom()
    {
        for (int x = 0; x < bottomLayerData.heightmapResolution; x++)
        {
            for (int y = 0; y < bottomLayerData.heightmapResolution; y++)
            {
                updatedMaterialMap[x, y].height = bottomLayerData.GetHeight(x,y) / heightScale;
            }
        }
    }

    public void GetInitialAmount()
    {
        for (int x = 0; x < bottomLayerData.heightmapResolution; x++)
        {
            for (int y = 0; y < bottomLayerData.heightmapResolution; y++)
            {
                if(bottomLayerData.GetHeight(x, y) < currentLayerData.GetHeight(x, y))
                {
                    updatedMaterialMap[x, y].amount = (currentLayerData.GetHeight(x, y) - bottomLayerData.GetHeight(x, y)) / heightScale;
                }
                                  
            }
        }
    }
}
