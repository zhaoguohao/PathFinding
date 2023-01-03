using System;
using System.Collections.Generic;
using System.Linq;
using GameArki.PathFinding.Generic;

namespace GameArki.PathFinding.AStar
{

    public static class AStarUtil
    {

        static readonly int MOVE_DIAGONAL_COST = 141;
        static readonly int MOVE_STRAIGHT_COST = 100;

        public static List<AStarNode> FindPath(byte[,] map, int startX, int startY, int endX, int endY, bool allowDiagonalMove)
        {

            // 初始化起点和终点
            AStarNode startNode = new AStarNode()
            {
                X = startX,
                Y = startY,
                G = 0,
                H = GetManhattanDistance(new AStarNode() { X = startX, Y = startY }, new AStarNode() { X = endX, Y = endY }),
                F = 0
            };
            AStarNode endNode = new AStarNode()
            {
                X = endX,
                Y = endY
            };

            // 创建开启列表和关闭列表
            var lenX = map.GetLength(0);
            var lenY = map.GetLength(1);
            List<AStarNode> openList = new List<AStarNode>();
            byte[,] closeList = new byte[lenX, lenY];
            byte[,] openListInfo = new byte[lenX, lenY];

            if (!IsInBoundary(map, startNode))
            {
                return null;
            }
            if (!IsInBoundary(map, endNode))
            {
                return null;
            }

            // 将起点添加到开启列表中
            openList.Add(startNode);
            AStarNode currentNode = startNode;

            int count = 0;
            while (openList.Count > 0)
            {
                count++;
                if (count > 1000) return null;
                // 找到开启列表中F值
                currentNode = GetLowestFNode(openList, endNode);
                // 从开启列表中移除当前节点，并将其添加到关闭列表中
                openList.Remove(currentNode);
                openListInfo[currentNode.X, currentNode.Y] = 0;
                closeList[currentNode.X, currentNode.Y] = 1;

                // 如果当前节点为终点，则找到了最短路径
                if (currentNode.X == endNode.X && currentNode.Y == endNode.Y)
                {
                    // 使用栈来保存路径
                    Stack<AStarNode> path = new Stack<AStarNode>();
                    while (currentNode != null)
                    {
                        path.Push(currentNode);
                        currentNode = currentNode.Parent;
                    }
                    return path.ToList();
                }

                // 获取当前节点的周围节点
                List<AStarNode> neighbours = GetRealNeighbours(map, currentNode, closeList, allowDiagonalMove);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    var neighbour = neighbours[i];
                    // 计算新的G值
                    var g_offset = GetDistance(currentNode, neighbour, allowDiagonalMove);
                    int newG = currentNode.G + g_offset;

                    // 如果新的G值比原来的G值小,计算新的F值 
                    if (openListInfo[neighbour.X, neighbour.Y] == 0 || newG < neighbour.G)
                    {
                        neighbour.G = newG;
                        neighbour.H = GetDistance(neighbour, endNode, allowDiagonalMove);
                        neighbour.F = neighbour.G + neighbour.H;
                        neighbour.Parent = currentNode;
                    }

                    // 如果节点不在开启列表中，则将其添加到开启列表中 
                    if (openListInfo[neighbour.X, neighbour.Y] == 0)
                    {
                        openList.Add(neighbour);
                        openListInfo[neighbour.X, neighbour.Y] = 1;
                    }
                }

            }

            // 如果开启列表为空，则无法找到路径
            return null;
        }

        static AStarNode GetLowestFNode(List<AStarNode> openList, AStarNode endNode)
        {
            AStarNode lowestFNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                var node = openList[i];
                if (node.F < lowestFNode.F)
                {
                    lowestFNode = node;
                }
                else if (node.F == lowestFNode.F && GetManhattanDistance(endNode, node) < GetManhattanDistance(endNode, lowestFNode))
                {
                    lowestFNode = node;
                }
            }

            return lowestFNode;
        }

        static List<AStarNode> GetRealNeighbours(byte[,] map, AStarNode currentNode, byte[,] closeList, bool allowDiagonalMove)
        {
            List<AStarNode> neighbours = new List<AStarNode>();
            // 获取当前节点的位置
            int x = currentNode.X;
            int y = currentNode.Y;

            // 获取四周的节点
            AStarNode top = new AStarNode() { X = x, Y = y + 1 };
            AStarNode bottom = new AStarNode() { X = x, Y = y - 1 };
            AStarNode left = new AStarNode() { X = x - 1, Y = y };
            AStarNode right = new AStarNode() { X = x + 1, Y = y };

            if (CanPass(map, top, closeList)) neighbours.Add(top);
            if (CanPass(map, bottom, closeList)) neighbours.Add(bottom);
            if (CanPass(map, left, closeList)) neighbours.Add(left);
            if (CanPass(map, right, closeList)) neighbours.Add(right);

            if (allowDiagonalMove)
            {
                AStarNode top_left = new AStarNode() { X = x - 1, Y = y + 1 };
                AStarNode bottom_left = new AStarNode() { X = x - 1, Y = y - 1 };
                AStarNode top_right = new AStarNode() { X = x + 1, Y = y + 1 };
                AStarNode bottom_right = new AStarNode() { X = x + 1, Y = y - 1 };
                if (CanPass(map, top_left, closeList)) neighbours.Add(top_left);
                if (CanPass(map, bottom_left, closeList)) neighbours.Add(bottom_left);
                if (CanPass(map, top_right, closeList)) neighbours.Add(top_right);
                if (CanPass(map, bottom_right, closeList)) neighbours.Add(bottom_right);
            }

            return neighbours;
        }

        static int GetDistance(AStarNode node1, AStarNode node2, bool allowDiagonalMove)
        {
            if (allowDiagonalMove)
            {
                int xDistance = Math.Abs(node1.X - node2.X);
                int yDistance = Math.Abs(node1.Y - node2.Y);
                int remaining = Math.Abs(xDistance - yDistance);
                return MOVE_DIAGONAL_COST * Math.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
            }
            else
            {
                return GetManhattanDistance(node1, node2);
            }
        }

        static int GetManhattanDistance(AStarNode node1, AStarNode node2)
        {
            return Math.Abs(node1.X - node2.X) + Math.Abs(node1.Y - node2.Y);
        }

        static bool CanPass(byte[,] map, AStarNode node, byte[,] closeList)
        {
            if (!IsInBoundary(map, node)) return false;

            var x = node.X;
            var y = node.Y;

            if (map[x, y] != 0)
            {
                return false;
            }
            if (closeList[x, y] != 0)
            {
                return false;
            }

            return true;
        }

        static bool IsInBoundary(byte[,] map, AStarNode node)
        {
            var x = node.X;
            var y = node.Y;
            var lenX = map.GetLength(0);
            var lenY = map.GetLength(1);
            if (x >= lenX || x < 0 || y >= lenY || y < 0)
            {
                return false;
            }

            return true;
        }

    }

}

