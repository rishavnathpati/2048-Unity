using System;
using System.Collections.Generic;
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
    private int _round;
    private GameState _state;

    private void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    private void Update()
    {
        if (_state != GameState.WaitingInput)
            return;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
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
                SpawnBlocks(_round++ == 0 ? 2 : 1);
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

    private void Shift(Vector2 dir)
    {
        var orderedBlocks = _blockList.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.node;
            do
            {
                block.SetBlock(next);
                var possibleNode = GetNodeAtPosition(next.Pos + dir);
                if (possibleNode != null)
                {
                    if (possibleNode.occupiedBlock == null) next = possibleNode;
                }
            } while (next!=block.node);

            block.transform.position = block.node.Pos;
        }
    }

    private Node GetNodeAtPosition(Vector2 pos)
    {
        return _nodeList.FirstOrDefault(n => n.Pos == pos);
    }

    private BlockType GetBlockTypeByValue(int value)
    {
        return types.First(t => t.value == value);
    }

    private void GenerateGrid()
    {
        _round = 0;
        _nodeList = new List<Node>();
        _blockList = new List<Block>();
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
            block.SetBlock(node);
            _blockList.Add(block);
        }

        if (freeNodes.Count() == 1)
            //Lost the game
            return;


        ChangeState(GameState.WaitingInput);
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