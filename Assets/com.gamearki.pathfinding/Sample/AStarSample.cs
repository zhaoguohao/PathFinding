using System.Collections.Generic;
using UnityEngine;
using GameArki.PathFinding.AStar;
using GameArki.PathFinding.Generic;

namespace AStar.Sample
{

    public class AStarSample : MonoBehaviour
    {

        public int width;
        public int height;

        public Transform start;
        public Transform end;

        public bool allowDiagonalMove;

        byte[,] map;
        bool isRunning = false;

        List<AStarNode> path;

        void Awake()
        {
            map = new byte[width, height];
            isRunning = true;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    GetXY(hit.point, out int x, out int y);
                    var lenX = map.GetLength(0);
                    var lenY = map.GetLength(1);
                    if (!(x < 0 || x >= lenX || y < 0 || y >= lenY))
                    {
                        map[x, y] = map[x, y] == 1 ? map[x, y] = 0 : map[x, y] = 1;
                    }
                }
            }
        }

        void FixedUpdate()
        {
            GetXY(start.position, out int startX, out int startY);
            GetXY(end.position, out int endX, out int endY);
            path = AStarUtil.FindPath(map, startX, startY, endX, endY, allowDiagonalMove);
        }

        void OnDrawGizmos()
        {
            if (!isRunning) return;

            Gizmos.color = Color.black;
            DrawMapLine();
            DrawObstacles();

            if (path != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    var p1 = new Vector3(path[i].X, path[i].Y);
                    var p2 = new Vector3(path[i + 1].X, path[i + 1].Y);
                    p1 += new Vector3(0.5f, 0.5f, 0.5f);
                    p2 += new Vector3(0.5f, 0.5f, 0.5f);
                    Gizmos.DrawLine(p1, p2);
                }
            }

        }

        void DrawObstacles()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] != 0)
                    {
                        Gizmos.DrawCube(new Vector3(i + 0.5f, j + 0.5f), new Vector3(0.8f, 0.8f, 1f));
                    }
                }
            }
        }

        void DrawMapLine()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height - 1; j++)
                {
                    Gizmos.DrawLine(new Vector3(i, j, 0), new Vector3(i, j + 1, 0));
                }
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width - 1; j++)
                {
                    Gizmos.DrawLine(new Vector3(j, i, 0), new Vector3(j + 1, i, 0));
                }
            }
        }

        void GetXY(Vector3 pos, out int x, out int y)
        {
            var posX = pos.x;
            var posY = pos.y;
            x = Mathf.FloorToInt(posX);
            y = Mathf.FloorToInt(posY);
        }

    }

}
