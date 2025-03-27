using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;

namespace neongine {
    public class QuadtreeSpacePartitioner : ISpacePartitioner
    {
        private class Quadtree {
            private struct Leaf
            {
                public Bounds Bounds;
                public List<int> Entities;
                public Leaf[] Children;
            }

            private Leaf m_Base;

            public Quadtree(Bounds bounds) {
                m_Base = new Leaf() {
                    Bounds = bounds,
                    Entities = new List<int>(),
                    Children = null
                };
            }

            public void Add(int id, Vector3 position, Bounds bounds) {
                Bounds absoluteBounds = new Bounds(bounds.X + position.X, bounds.Y + position.Y, bounds.Width, bounds.Height);
                
                Leaf leaf;
                if (GetBoundingLeaf(m_Base, id, absoluteBounds, out leaf) == false)
                    leaf = m_Base;

                leaf.Entities.Add(id);
            }

            private bool GetBoundingLeaf(Leaf leaf, int id, Bounds bounds, out Leaf boundingLeaf)
            {                
                if (Contains(leaf.Bounds, bounds)) {
                    
                    if (leaf.Children == null)
                        leaf.Children = CreateSubleaves(leaf.Bounds);

                    for (int i = 0; i < leaf.Children.Length; i++) {
                        if (GetBoundingLeaf(leaf.Children[i], id, bounds, out boundingLeaf))
                            return true;
                    }

                    boundingLeaf = leaf;
                    return true;

                }

                boundingLeaf = default(Leaf);
                return false;
            }

            private bool Contains(Bounds parentBound, Bounds childBound) {                
                return (parentBound.X < childBound.X)
                        && (parentBound.X + parentBound.Width > childBound.X + childBound.Width)
                        && (parentBound.Y < childBound.Y)
                        && (parentBound.Y + parentBound.Height > childBound.Y + childBound.Height);
            }

            private Leaf[] CreateSubleaves(Bounds parentBound) {
                Leaf[] leaves = new Leaf[4];

                float width = parentBound.Width / 2;
                float height = parentBound.Height / 2;

                for (int i = 0; i < 4; i++) {
                    leaves[i] = new Leaf() {
                        Bounds = new Bounds(parentBound.X + width * i % 2, parentBound.Y + height * (int)(i / 2), width, height),
                        Entities = new List<int>(),
                        Children = null
                    };
                }

                return leaves;
            }

            public int[][] BuildPartition() {
                List<int[]> partition = new();

                FillPartition(m_Base, partition, []);

                return partition.ToArray();
            }

            private void FillPartition(Leaf leaf, List<int[]> partition, int[] storage) {
                int[] indices = [.. storage, .. leaf.Entities];

                if (leaf.Children == null) {
                    if (indices.Length > 0) {
                        partition.Add(indices);
                        string indicesString = "";
                        foreach (var indice in indices) indicesString += indice.ToString() + ", ";
                        Debug.WriteLine("Added partition index " + (partition.Count - 1) + " with indices " + indicesString);
                    }

                    return;
                }

                for (int i = 0; i < leaf.Children.Length; i++) {
                    FillPartition(leaf.Children[i], partition, indices);
                }
            }
        }

        public int[][] Partition(Point[] points, ColliderBounds[] colliderBounds)
        {
            Debug.WriteLine("Starting new partition");
            Quadtree tree = BuildTree(points, colliderBounds);

            int[][] partition = tree.BuildPartition();

            return partition;
        }

        private Quadtree BuildTree(Point[] points, ColliderBounds[] colliderBounds) {
            Quadtree tree = new Quadtree(new Bounds(0, 0, 1000, 1000));
            
            for (int i = 0; i < points.Length; i++) {
                tree.Add(i, points[i].WorldPosition, colliderBounds[i].Bounds);
            }

            return tree;
        }
    }
}
