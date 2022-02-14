using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private List<BlockType> types;

    private List<Block> _blockList;
    private List<Node> _nodeList;
    private GameState _state;
    private int round;

    private void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newState)
    {
        _state = newState;
        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private BlockType GetBlockTypeByValue(int value)
    {
        return types.First(t => t.value == value);
    }

    private void GenerateGrid()
    {
        round = 0;
        _nodeList = new List<Node>();
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var node = Instantiate(nodePrefab, new Vector2(x, y), quaternion.identity);
            _nodeList.Add(node);
        }

        var center = new Vector2((float) width / 2 - .5f, (float) height / 2 - .5f);

        var board = Instantiate(boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        if (Camera.main != null) Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        
        ChangeState(GameState.SpawningBlocks);
    }

    private void SpawnBlocks(int amount)
    {
        var freeNodes = _nodeList.Where(n => n.occupiedBlock == null).OrderBy(b => Random.value).ToList();
        foreach (var node in freeNodes.Take(amount))
        {
            var block = Instantiate(blockPrefab, node.Pos, quaternion.identity);
            block.Init(GetBlockTypeByValue(Random.value > 0.8 ? 4 : 2));
        }

        if (freeNodes.Count() == 1)
            //Lost the game
            return;
    }

    [Serializable]
    public struct BlockType
    {
        public int value;
        public Color color;
    }
}

public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}