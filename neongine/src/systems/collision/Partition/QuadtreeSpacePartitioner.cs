using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using neon;

namespace neongine.editor {
    public class QuadtreeSpacePartitioner : ISpacePartitioner, IEditorDrawSystem
    {
        public bool ActiveInPlayMode => true;

        private class Quadtree : IEnumerable<(EntityID, EntityID)> {
            private class Leaf
            {
                public int Index;
                public Rect Bounds;
                public List<EntityID> Entities;
                public Leaf[] Children;
            }

            private Leaf m_Base;

            private int m_Index = 0;

            private List<(EntityID, EntityID)> m_Enumerator;

            public Quadtree(Rect bounds) {
                m_Base = GetBaseLeaf(bounds);
            }

            private Leaf GetBaseLeaf(Rect bounds)
            {
                float smallestBound;
                int squareQuantity;

                if (bounds.Width == bounds.Height)
                {
                    squareQuantity = 1;
                    smallestBound = bounds.Width != 0 ? bounds.Width : 1;
                } else
                {
                    float biggestBound;
                    if (bounds.Width < bounds.Height)
                        (smallestBound, biggestBound) = (bounds.Width, bounds.Height);
                    else
                        (smallestBound, biggestBound) = (bounds.Height, bounds.Width);

                    squareQuantity = (int)(biggestBound / smallestBound) + 1;
                }

                float squareSide = squareQuantity * smallestBound;

                Leaf leaf = new Leaf()
                {
                    Index = m_Index++,
                    Bounds = new Rect(bounds.X, bounds.Y, squareSide, squareSide),
                    Entities = new List<EntityID>(),
                    Children = null
                };

                return leaf;
            }

            public void Add(EntityID id, Vector2 position, Bounds bounds) {
                Rect absoluteBounds = new Rect(position.X + bounds.X, position.Y + bounds.Y, bounds.Width, bounds.Height);
                
                Leaf leaf;
                if (GetBoundingLeaf(m_Base, id, absoluteBounds, out leaf) == false)
                    leaf = m_Base;

                leaf.Entities.Add(id);
            }

            private bool GetBoundingLeaf(Leaf leaf, EntityID id, Rect bounds, out Leaf boundingLeaf)
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

            private bool Contains(Rect parentBound, Rect childBound) {                
                return (parentBound.X <= childBound.X)
                        && (parentBound.X + parentBound.Width >= childBound.X + childBound.Width)
                        && (parentBound.Y <= childBound.Y)
                        && (parentBound.Y - parentBound.Height <= childBound.Y - childBound.Height);
            }

            private Leaf[] CreateSubleaves(Rect parentBound) {
                Leaf[] leaves = new Leaf[4];

                float width = parentBound.Width / 2;
                float height = parentBound.Height / 2;

                for (int i = 0; i < 4; i++) {
                    leaves[i] = new Leaf() {
                        Index = m_Index++,
                        Bounds = new Rect(parentBound.X + (width * (i % 2)), parentBound.Y - height * (int)(i / 2), width, height),
                        Entities = new List<EntityID>(),
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
                if (leaf.Children == null)
                    return;

                foreach (var child in leaf.Children) {
                    PrintEntitiesInternal(child);
                }
            }

            public void BuildEnumerator() {
                List<(EntityID, EntityID)> enumerator = new();

                AddCollisionChecks(m_Base, enumerator, []);

                m_Enumerator = enumerator;
            }

            private void AddCollisionChecks(Leaf leaf, List<(EntityID, EntityID)> enumerator, EntityID[] previousIndices) {                
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

                EntityID[] mergedIndices = [.. previousIndices, .. leaf.Entities];

                foreach (var child in leaf.Children) {
                    AddCollisionChecks(child, enumerator, mergedIndices);
                }
            }

            public IEnumerator<(EntityID, EntityID)> GetEnumerator()
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
                RenderingSystem.DrawRectangle(
                    new Vector2(leaf.Bounds.X, leaf.Bounds.Y),
                    new Vector2(leaf.Bounds.Width, leaf.Bounds.Height),
                    0.0f,
                    Color.LightGoldenrodYellow);

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

        public IEnumerable<(EntityID, EntityID)> Partition(EntityID[] ids, Vector2[] positions, Bounds[] bounds)
        {
            Quadtree tree = BuildTree(ids, positions, bounds);

            tree.PrintEntities();

            m_Quadtree = tree;

            return tree;
        }

        private Quadtree BuildTree(EntityID[] ids, Vector2[] positions, Bounds[] bounds) {
            Quadtree tree = new Quadtree(GetTreeBounds(positions, bounds));
            
            for (int i = 0; i < ids.Length; i++) {
                tree.Add(ids[i], positions[i], bounds[i]);
            }

            tree.BuildEnumerator();

            return tree;
        }

        private Rect GetTreeBounds(Vector2[] positions, Bounds[] bounds)
        {
            float xMin = 10000;
            float yMin = 10000;
            float xMax = -10000;
            float yMax = -10000; 

            for (int i = 0; i < positions.Length; i++)
            {
                float entityMinX = positions[i].X + bounds[i].X;
                float entityMaxX = positions[i].X + bounds[i].X + bounds[i].Width;
                float entityMinY = positions[i].Y + bounds[i].Y - bounds[i].Height;
                float entityMaxY = positions[i].Y + bounds[i].Y;

                if (entityMinX < xMin)
                    xMin = entityMinX;
                if (entityMaxX > xMax)
                    xMax = entityMaxX;
                if (entityMinY < yMin)
                    yMin = entityMinY;
                if (entityMaxY > yMax)
                    yMax = entityMaxY;
            }

            return new Rect(xMin, yMax, xMax - xMin, yMax - yMin);
        }

        public void Draw() {
#if DRAW_PARTITION
            if (m_Quadtree == null)
                return;

            m_Quadtree.Draw(m_SpriteBatch);
#endif
        }
    }
}
