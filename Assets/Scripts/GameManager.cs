using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private const int WinCondition = 2048;
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private List<BlockType> types;
    [SerializeField] private float travelTime = 0.3f;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

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
        if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
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
                winScreen.SetActive(true);
                break;
            case GameState.Lose:
                loseScreen.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);
        var orderedBlocks = _blockList.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.node;
            do
            {
                block.SetBlock(next);
                var possibleNode = GetNodeAtPosition(next.Pos + dir);
                if (possibleNode == null) continue;

                if (possibleNode.occupiedBlock != null && possibleNode.occupiedBlock.value == block.value)
                    block.MergeBlock(possibleNode.occupiedBlock);
                else if (possibleNode.occupiedBlock == null)
                    next = possibleNode;
            } while (next != block.node);
        }

        var sequence = DOTween.Sequence();
        foreach (var block in orderedBlocks)
        {
            var movePoint = block.mergingBlock != null ? block.mergingBlock.node.Pos : block.node.Pos;
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

        sequence.OnComplete(() =>
        {
            foreach (var block in orderedBlocks.Where(b => b.mergingBlock != null))
                MergeBlocks(block.mergingBlock, block);
            ChangeState(GameState.SpawningBlocks);
        });
    }

    private void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnBlock(baseBlock.node, baseBlock.value * 2);

        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    private void RemoveBlock(Block block)
    {
        _blockList.Remove(block);
        Destroy(block.gameObject);
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
        foreach (var node in freeNodes.Take(amount)) SpawnBlock(node, Random.value > 0.8f ? 4 : 2);

        if (freeNodes.Count() == 1)
        {
            //Lost the game
            ChangeState(GameState.Lose);
            return;
        }


        ChangeState(_blockList.Any(b => b.value == WinCondition) ? GameState.Win : GameState.WaitingInput);
    }

    private void SpawnBlock(Node node, int value)
    {
        var block = Instantiate(blockPrefab, node.Pos, quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        block.SetBlock(node);
        _blockList.Add(block);
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