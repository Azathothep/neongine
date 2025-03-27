using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace neongine {
    public class QuadtreeSpacePartitioner : ISpacePartitioner
    {
        private struct Leaf {
            public Bounds Bounds;
            public List<int> Entities;
            public Leaf[] Children;
        }

        public int[][] Partition(Point[] points, ColliderBounds[] colliderBounds)
        {
            Leaf[] tree = BuildTree(points, colliderBounds);

            int[][] partition = BuildPartition(tree);

            return partition;
        }

        private Leaf[] BuildTree(Point[] points, ColliderBounds[] colliderBounds) {
            Leaf[] baseTree = CreateChildrenLeaves(new Bounds(0, 0, 1000, 1000));

            for (int i = 0; i < points.Length; i++) {
                AddTree(baseTree, points[i].WorldPosition, colliderBounds[i].Bounds);
            }

            return baseTree;
        }

        private void AddTree(Leaf[] tree, Vector3 position, Bounds bounds) {

        }

        private Leaf[] CreateChildrenLeaves(Bounds parentBound) {
            Leaf[] leaves = new Leaf[4];

            float width = parentBound.Width / 2;
            float height = parentBound.Height / 2;

            for (int i = 0; i < 4; i++) {
                leaves[i] = new Leaf() {
                    Bounds = new Bounds(parentBound.X + width * i % 2, parentBound.Y + height * i % 2, width, height),
                    Entities = new List<int>(),
                    Children = null
                };
            }

            return leaves;
        }

        private int[][] BuildPartition(Leaf[] tree) {
            List<List<int>> partition = new List<List<int>>();

            // Fill in lists

            int[][] array = new int[partition.Count][];

            for (int i = 0; i < partition.Count; i++) {
                array[i] = new int[partition[i].Count];
                for (int j = 0; j < partition[i].Count; j++) {
                    array[i][j] = partition[i][j];
                }
            }

            return array;
        }
    }
}
