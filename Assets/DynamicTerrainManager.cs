using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicTerrainManager : MonoBehaviour
{
    // Start is called before the first frame update
    private DynamicTerrain[] managedTerrains;
    public Terrain lowerLevelTerrain;
    private TerrainData lowerLevel;

    void Start()
    {
        managedTerrains = FindObjectsOfType(typeof(DynamicTerrain)) as DynamicTerrain[];
        lowerLevel = lowerLevelTerrain.terrainData;
        
    }

    // Update is called once per frame
    void Update()
    {

        CalculateMisplacement();
    }

    private void LateUpdate()
    {
        foreach (DynamicTerrain terrain in managedTerrains)
        {
            terrain.currentLayerData.SetHeights(0, 0, terrain.updatedHeightMap);
        }
            
    }

    private void initHeight()
    {
        foreach (DynamicTerrain terrain in managedTerrains)
        {
            for (int x = 0; x < lowerLevel.heightmapResolution; x++)
            {
                for (int y = 0; y < lowerLevel.heightmapResolution; y++)
                {
                    terrain.materialMap[x, y].height = (lowerLevel.GetHeight(x, y) / terrain.heightScale) + terrain.heightPenalty;
                }
            }
        }
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
            if ((x + step.x) > 0 && (x + step.x) < lowerLevel.heightmapResolution && //preventing out of bounds
                (y + step.y) > 0 && (y + step.y) < lowerLevel.heightmapResolution)
                returnValue.Add(new Vector2Int(x + step.x, y + step.y));
        }

        return returnValue;
    }


    public void CalculateMisplacement()
    {
        foreach (DynamicTerrain terrain in managedTerrains)
        {
            terrain.materialMap = terrain.updatedMaterialMap;

        }

        for (int x = 0; x < lowerLevel.heightmapResolution; x++)
        {
            for (int y = 0; y < lowerLevel.heightmapResolution; y++)
            {

                foreach (DynamicTerrain terrain in managedTerrains)
                {
                    float lastHeight = terrain.materialMap[x, y].height;
                    terrain.materialMap[x, y].height = (terrain.bottomLayerData.GetHeight(x, y) / terrain.heightScale) + terrain.heightPenalty;

                    if (terrain.materialMap[x, y].amount > terrain.minimumAmountToCalculate) //only needs to do this calculation if there's a reasonable amount of stuff in the title
                    {
                        List<Vector2Int> neightbours = GetNeighbours(x, y);
                        List<Vector2Int> lowerneightbours = new List<Vector2Int>();
                        float diff = terrain.materialMap[x, y].amount * terrain.percentageDrag;
                        float pointHeight = terrain.materialMap[x, y].height + terrain.materialMap[x, y].amount;
                        foreach (Vector2Int n in neightbours)
                        {
                            float nHeight = terrain.materialMap[n.x, n.y].height + terrain.materialMap[n.x, n.y].amount;
                            if ((nHeight + diff) < (pointHeight) &&
                                Mathf.Abs(nHeight - pointHeight) > terrain.minimumDrop)
                            {
                                lowerneightbours.Add(n);
                            }
                        };
                        foreach (Vector2Int n in lowerneightbours)
                        {
                            terrain.updatedMaterialMap[x, y].amount = terrain.materialMap[x, y].amount - diff;
                            terrain.updatedMaterialMap[n.x, n.y].amount = terrain.materialMap[n.x, n.y].amount + diff;
                            terrain.updatedMaterialMap[n.x, n.y].stable = false;
                        }
                        if (lowerneightbours.Count == 0) terrain.materialMap[x, y].stable = true;
                    }

                    terrain.updatedHeightMap[y, x] = terrain.materialMap[x, y].amount + terrain.materialMap[x, y].height;
                    
                }
                    
                
            }
        }


    }

}
