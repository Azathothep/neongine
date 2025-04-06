using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using neon;

namespace neongine {
    public class QuadtreeSpacePartitioner : ISpacePartitioner, IDrawSystem
    {
        private class Quadtree : IEnumerable<(int, int)> {
            private class Leaf
            {
                public int Index;
                public Bounds Bounds;
                public List<int> Entities;
                public Leaf[] Children;
            }

            private Leaf m_Base;

            private int m_Index = 0;

            private List<(int, int)> m_Enumerator;

            public Quadtree(Bounds bounds) {
                m_Base = new Leaf() {
                    Index = m_Index++,
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
                        Index = m_Index++,
                        Bounds = new Bounds(parentBound.X + (width * (i % 2)), parentBound.Y + height * (int)(i / 2), width, height),
                        Entities = new List<int>(),
                        Children = null
                    };
                }

                return leaves;
            }

            public void PrintEntities() {
                //Debug.WriteLine("---------");
                PrintEntitiesInternal(m_Base);
            }

            private void PrintEntitiesInternal(Leaf leaf) {
                //Debug.WriteLine($"[{leaf.Index}] : {leaf.Entities.Count} entities, bounds = {leaf.Bounds.X}, {leaf.Bounds.Y}, {leaf.Bounds.Width}, {leaf.Bounds.Height}");

                if (leaf.Children == null)
                    return;

                foreach (var child in leaf.Children) {
                    PrintEntitiesInternal(child);
                }
            }

            // public int[][] BuildPartition() {
            //     List<int[]> partition = new();

            //     FillPartition(m_Base, partition, []);

            //     return partition.ToArray();
            // }

            // private void FillPartition(Leaf leaf, List<int[]> partition, int[] storage) {
            //     int[] indices = [.. storage, .. leaf.Entities];

            //     if (leaf.Children == null) {
            //         if (indices.Length > 0) {
            //             partition.Add(indices);
            //             string indicesString = "";
            //             foreach (var indice in indices) indicesString += indice.ToString() + ", ";
            //             Debug.WriteLine("Added partition index " + (partition.Count - 1) + " with indices " + indicesString);
            //         }

            //         return;
            //     }

            //     for (int i = 0; i < leaf.Children.Length; i++) {
            //         FillPartition(leaf.Children[i], partition, indices);
            //     }
            // }

            public void BuildEnumerator() {
                List<(int, int)> enumerator = new();

                AddCollisionChecks(m_Base, enumerator, new int[0]);

                m_Enumerator = enumerator;
            }

            private void AddCollisionChecks(Leaf leaf, List<(int, int)> enumerator, int[] previousIndices) {                
                // Add pairs on current leaf
                for (int i = 0; i < leaf.Entities.Count - 1; i++) {
                    for (int j = i + 1; j < leaf.Entities.Count; j++) {
                        enumerator.Add((leaf.Entities[i], leaf.Entities[j]));
                    }
                }

                // Adding pairs of parent leaves
                for (int i = 0; i < leaf.Entities.Count; i++) {
                    for (int j = 0; j < previousIndices.Length; j++) {
                        enumerator.Add((leaf.Entities[i], previousIndices[j]));
                    }
                }

                if (leaf.Children == null || leaf.Children.Length == 0)
                    return;

                int[] mergedIndices = [.. previousIndices, .. leaf.Entities];

                foreach (var child in leaf.Children) {
                    AddCollisionChecks(child, enumerator, mergedIndices);
                }
            }

            public IEnumerator<(int, int)> GetEnumerator()
            {
                if (m_Enumerator == null)
                    BuildEnumerator();

                return m_Enumerator.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Draw(SpriteBatch spriteBatch) {
                DrawLeaf(m_Base, spriteBatch);
            }

            private void DrawLeaf(Leaf leaf, SpriteBatch spriteBatch) {
                MonoGame.Primitives2D.DrawRectangle(spriteBatch,
                new Rectangle((int)leaf.Bounds.X, (int)leaf.Bounds.Y, (int)leaf.Bounds.Width, (int)leaf.Bounds.Height),
                0.0f,
                Color.Red);

                if (leaf.Children == null)
                    return;

                foreach (var child in leaf.Children)
                    DrawLeaf(child, spriteBatch);
            }
        }

        private Quadtree m_Quadtree;

        private SpriteBatch m_SpriteBatch;

        public QuadtreeSpacePartitioner(SpriteBatch spriteBatch) {
            m_SpriteBatch = spriteBatch;
        }

        public IEnumerable<(int, int)> Partition(Vector3[] positions, ColliderBounds[] colliderBounds)
        {
            // Debug.WriteLine("Starting new partition");
            
            Quadtree tree = BuildTree(positions, colliderBounds);

            tree.PrintEntities();

            // int[][] partition = tree.BuildPartition();

            m_Quadtree = tree;

            return tree;
        }

        private Quadtree BuildTree(Vector3[] positions, ColliderBounds[] colliderBounds) {
            Quadtree tree = new Quadtree(new Bounds(0, 0, 800, 500));
            
            for (int i = 0; i < positions.Length; i++) {
                tree.Add(i, positions[i], colliderBounds[i].Bounds);
            }

            tree.BuildEnumerator();

            return tree;
        }

        public void Draw() {
            // m_SpriteBatch.Begin();

            // if (m_Quadtree == null)
            //     return;

            // m_Quadtree.Draw(m_SpriteBatch);

            // m_SpriteBatch.End();
        }
    }
}
