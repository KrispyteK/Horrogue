﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;



public class SimpleLayoutGenerator : MonoBehaviour {

	#region Structs
	[Serializable]
	public struct Range
	{
		public int min;
		public int max;

		public Range(int min, int max)
		{
			this.min = min;
			this.max = max;
		}
	}

	[Serializable]
	public struct SpawnOptions
	{
		public RegionType type;
		public bool enableSpawning;
		public Range amount;
		public Range width;
		public Range length;

		public SpawnOptions(RegionType type)
		{
			this.type = type;
			this.enableSpawning = false;
			this.amount = new Range(0, 1);
			this.width = new Range(0, 1);
			this.length = new Range(0, 1);
		}
	}
	#endregion

	#region Public Variables
	[Header("General")]
	public bool useRandomSeed = false;	// Should a seed be generated
	public string seed = "elementary";  // The current seed used for generation

	// The bounds where and how big the region generation is going to be
	public BoundsInt generationBounds = new BoundsInt(-50, -50, 0, 100, 100, 1);

	// Premade objects and regions
	[Header("Custom Content")]
	public List<CustomRegion> customRegions;

	[Header("Spawning Behaviour")]
	public int additionalCorridorAmount = 2;

	public Range spawnAreaSize;

	public Range corridorWidth;
	public Range corridorLength;

	public Range classAreaSize;


    // Tile prefabs and settings
    [Header("Generator Tiles")]
	public int tileSize = 1;
    public List<TileSprites> tileSprites;
	#endregion

	#region Private Variables
    private List<Region> regions;
	private int[,] map;
    private Furniture[,,] furnituremap;
    private GameObject[,] tilemap;
    private GameObject parent;
    #endregion

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateLayout();
        }
    }

    public void GenerateLayout()
	{
		InitializeRandom();
		InitializeMap();

        SetupCustomRegions();

        ConnectRegions();

        GenerateRandomRegions();

        PlaceRegionContent();

        PlaceTiles();
	}

	private void PlaceRegionContent()
    {

    }

	private void ConnectRegions()
	{
		// Find Spawn Region or create a new one
		Region spawnRegion = regions.Find(x => x.isSpawn);
		if (spawnRegion == null)
		{
			Vector3Int size = new Vector3Int(
				Mathf.RoundToInt(Random.Range(spawnAreaSize.min, spawnAreaSize.max + 1)), 
				Mathf.RoundToInt(Random.Range(spawnAreaSize.min, spawnAreaSize.max + 1)), 
				1);
			Vector3Int position = GetRandomPositionInsideBounds(size);
			customRegions.Add(new CustomRegion(position, size, RegionType.Toilet, true));
			regions.Add(new Region(new BoundsInt(position, size), RegionType.Toilet));
		}
		else
		{
			regions.Remove(spawnRegion);
		}

		// Find Main Corridor or create a new one
		Region mainCorridor = regions.Find(x => x.type == RegionType.MainCorridor);
		if (mainCorridor == null)
		{
			Range vert, hor;
			if (Random.value > 0.5f)
			{
				vert = corridorLength;
				hor = corridorWidth;
			} else
			{
				vert = corridorWidth;
				hor = corridorLength;
			}

			Vector3Int size = new Vector3Int(
				Mathf.RoundToInt(Random.Range(vert.min, vert.max + 1)),
				Mathf.RoundToInt(Random.Range(hor.min, hor.max + 1)),
				1);
			Vector3Int position = GetRandomPositionInsideBounds(size);
		}
		else
		{
			regions.Remove(mainCorridor);
		}


		for (int i = 0; i < regions.Count; i++)
		{
			Region r = regions[i];
			if (!r.isConnected)
			{

			}
		}
	}

	private Vector3Int GetRandomPositionInsideBounds(Vector3Int targetSize)
	{
		Vector2 rand = new Vector2(Random.value, Random.value);
		int x = Mathf.RoundToInt(rand.x * (generationBounds.size.x - targetSize.x) + generationBounds.position.x);
		int y = Mathf.RoundToInt(rand.y * (generationBounds.size.y - targetSize.y) + generationBounds.position.y);

		return new Vector3Int(x, y, 0);
	}

	private void GenerateRandomRegions()
    {
        
    }

	private void SetupCustomRegions()
    {
        for (int i = 0; i < customRegions.Count; i++)
        {
            CustomRegion reg = customRegions[i];
            BoundsInt bounds = new BoundsInt(Vector3Int.RoundToInt(reg.bounds.position + generationBounds.center 
                + Vector3.Scale(generationBounds.size, new Vector3(.5f, .5f))), reg.bounds.size);

			regions.Add(new Region(bounds, reg.type));
        }
    }

    private void PlaceTiles()
    {
        for (int x = 0; x < generationBounds.size.x; x++)
        {
            for (int y = 0; y < generationBounds.size.y; y++)
            {
                Vector3 position = new Vector3(generationBounds.xMin + (x + 0.5f) * tileSize,
                    generationBounds.yMin + (y + 0.5f * tileSize) * tileSize);
                Vector3 size = Vector3.one * tileSize;

                GameObject tilePrefab; 
                TileType tileType = (TileType)map[x, y];
                tilePrefab = tileSprites[(int)tileType].tilePrefabs[0];

                tilemap[x, y] = Instantiate(tilePrefab, position, Quaternion.identity, parent.transform);
            }
        }
    }

    private void InitializeRandom()
	{
		if (useRandomSeed)
		{
			seed = System.DateTime.Now.Ticks.ToString();
		}
		Random.InitState(seed.GetHashCode());
	}

	private void InitializeMap()
	{
        if (parent != null)
        {
            Destroy(parent);
            parent = null;
        }

		parent = new GameObject("Tile Map");

		regions = new List<Region>();

		map = new int[generationBounds.size.x, generationBounds.size.y];
		tilemap = new GameObject[generationBounds.size.x, generationBounds.size.y];

		for (int x = 0; x < generationBounds.size.x; x++)
		{
			for (int y = 0; y < generationBounds.size.y; y++)
			{
				map[x, y] = (int) TileType.Air;
				tilemap[x, y] = null;
            }
		}
	}

	private void MapPremadeRegions()
	{
        for (int i = 0; i < customRegions.Count; i++)
        {
            CustomRegion reg = customRegions[i];
            BoundsInt bounds = new BoundsInt(Vector3Int.RoundToInt(reg.bounds.position + generationBounds.center + Vector3.Scale(generationBounds.size, new Vector3(.5f, .5f))), reg.bounds.size);
            Debug.Log(bounds);
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    TileType tile = TileType.Ground;
                    if ((TileType)map[x, y] != TileType.Ground && (x == bounds.xMin || x == bounds.xMax - 1 || y == bounds.yMin || y == bounds.yMax - 1))
                    {
                        tile = TileType.Wall;
                    }
                    map[x, y] = (int) tile;
                }
            }
        }

	}

	private void MapCorridors()
	{

	}

    private void MapCorridorsOld()
    {
        // First place a main corridor if none are placed
        // Some temporary parameters
        int mainCorridorHalf = 2;
        int numSubCorridors = 4;

        // Pick random spot inside given bounds and extend a corridor in one direction to its maximum length
        // Also give the generation some padding so the corridor generates with it's full width
        Vector2Int mainCorridorSpawn = new Vector2Int(Random.Range(mainCorridorHalf, generationBounds.size.x - mainCorridorHalf), 
            Random.Range(mainCorridorHalf, generationBounds.size.y - mainCorridorHalf));

        BoundsInt[] corridors = new BoundsInt[numSubCorridors + 1];

        // Temporary direction choosing, 0 is horizontal, 1 is vertical
        bool isVertical = Mathf.RoundToInt(Random.value) == 1;

		Vector2Int corridorSpawn = new Vector2Int();
		for (int i = 0; i < numSubCorridors + 1; i++)
        {
			// Place the main corridor first
			// Then determine if the other corridors are supposed to be horizontal or vertical to be orthogonal
			// to the first (main) corridor
			if (i == 0)
			{
				corridorSpawn = mainCorridorSpawn;
			}
			else if (isVertical)
			{
				corridorSpawn.x = Random.Range(mainCorridorHalf, generationBounds.size.x - mainCorridorHalf);
				corridorSpawn.y = mainCorridorSpawn.y;
			}
			else
			{
				corridorSpawn.x = mainCorridorSpawn.x;
				corridorSpawn.y = Random.Range(mainCorridorHalf, generationBounds.size.y - mainCorridorHalf);
			}

			// Calculate the corridor bounds
			corridors[i] = new BoundsInt();
			corridors[i].xMin = (isVertical) ? corridorSpawn.x - mainCorridorHalf
					: mainCorridorHalf;
			corridors[i].yMin = (!isVertical) ? corridorSpawn.y - mainCorridorHalf
				: mainCorridorHalf;
			corridors[i].xMax = (isVertical) ? corridorSpawn.x + mainCorridorHalf
				: generationBounds.size.x - mainCorridorHalf;
			corridors[i].yMax = (!isVertical) ? corridorSpawn.y + mainCorridorHalf
				: generationBounds.size.y - mainCorridorHalf;

			if (i == 0) {
				isVertical = !isVertical;
            }

			for (int x = corridors[i].xMin; x <= corridors[i].xMax; x++)
			{
				for (int y = corridors[i].yMin; y <= corridors[i].yMax; y++)
				{
					TileType tile = TileType.Ground;
					if (map[x,y] != (int)TileType.Ground && (x == corridors[i].xMin || x == corridors[i].xMax || y == corridors[i].yMin || y == corridors[i].yMax))
					{
						tile = TileType.Wall;
					}
					map[x, y] = (int)tile;
				}
			}
		}
    }

    private void MapRooms()
	{
		
	}


    private void OnDrawGizmos()
    {
        if (generationBounds != null)
        {
            
            Gizmos.color = Color.green;

            Gizmos.DrawLine(new Vector3(generationBounds.xMin, generationBounds.yMin), new Vector3(generationBounds.xMin, generationBounds.yMax));
            Gizmos.DrawLine(new Vector3(generationBounds.xMin, generationBounds.yMin), new Vector3(generationBounds.xMax, generationBounds.yMin));
            Gizmos.DrawLine(new Vector3(generationBounds.xMax, generationBounds.yMax), new Vector3(generationBounds.xMin, generationBounds.yMax));
            Gizmos.DrawLine(new Vector3(generationBounds.xMax, generationBounds.yMax), new Vector3(generationBounds.xMax, generationBounds.yMin));

            //if (map != null)
            //{
            //    for (int x = 0; x < generationBounds.size.x; x++)
            //    {
            //        for (int y = 0; y < generationBounds.size.y; y++)
            //        {
            //            Vector3 position = new Vector3(generationBounds.xMin + (x + 0.5f) * tileSize,
            //                generationBounds.yMin + (y + 0.5f) * tileSize);
            //            Vector3 size = Vector3.one * tileSize;
            //            TileType tile = (TileType)map[x, y];
            //            switch (tile)
            //            {
            //                case TileType.Air:
            //                    Gizmos.color = Color.grey;
            //                    break;
            //                case TileType.Wall:
            //                    Gizmos.color = Color.black;
            //                    break;
            //                case TileType.Ground:
            //                    Gizmos.color = Color.green;
            //                    break;
            //                default: break;
            //            }
            //            Gizmos.DrawCube(position, size);
            //        }
            //    }
            //}
        }
    }
}
