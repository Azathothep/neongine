using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using neon;

namespace neongine {
    public class VelocityCollisionResolver : ICollisionResolver
    {
        private struct OneMovementData {
            public Penetration[] PenetrationDatas;
            public int Index;
            public Vector2 Velocity;

            public OneMovementData(Collision collision, int index, Vector2 velocity) {
                PenetrationDatas = collision.Penetration;
                Index = index;
                Velocity = velocity;
            }
        }

        private struct TwoMovementsData {
            public Penetration[] PenetrationDatas;
            public (int, int) Indices;
            public (Vector2, Vector2) Velocities;
            public TwoMovementsData(Collision collision, int index1, Vector2 velocity1, int index2, Vector2 velocity2) {
                PenetrationDatas = collision.Penetration;
                Indices = (index1, index2);
                Velocities = (velocity1, velocity2);
            }
        }

        private class Solution {
            public int[] Entities;
            public Correction[] Corrections;

            public Solution(int[] entities, Correction[] corrections) {
                Entities = entities;
                Corrections = corrections;
            }
        }

        private struct Correction {
            public Vector2 Velocity;
            public float Length;
            public Correction(Vector2 velocity, float length) {
                Velocity = velocity;
                Length = length;
            }

            public bool HasPriorityAgainst(Correction other) => this.Length < other.Length;
        }

        public void Resolve(CollisionData[] collisionDatas, EntityID[] entityIDs, Velocity[] velocities, bool[] isStatic, float deltaTime)
        {
            List<OneMovementData> oneMovementResolutions;
            List<TwoMovementsData> twoMovementsResolutions;

            FillResolutionDatas(collisionDatas, entityIDs, velocities, isStatic, deltaTime, out oneMovementResolutions, out twoMovementsResolutions);

            Dictionary<int, List<(Solution, int)>> entityToSolutions = GetSolutions(oneMovementResolutions, twoMovementsResolutions);

            Dictionary<int, Correction> entityToCorrections = GetSingleCorrections(entityToSolutions);
            
            foreach (var correction in entityToCorrections) {
                velocities[correction.Key].Value = correction.Value.Velocity / deltaTime;
            }
        }

        private void FillResolutionDatas(CollisionData[] collisionDatas, EntityID[] entityIDs, Velocity[] velocities, bool[] isStatic, float deltaTime, out List<OneMovementData> oneMovementResolutions, out List<TwoMovementsData> twoMovementsResolutions) {
            oneMovementResolutions = new();
            twoMovementsResolutions = new();

            for (int i = 0; i < collisionDatas.Length; i++) {
                int id1 = Array.FindIndex(entityIDs, id => id == collisionDatas[i].Entities.Item1);
                int id2 = Array.FindIndex(entityIDs, id => id == collisionDatas[i].Entities.Item2);

                bool movingEntity1 = !isStatic[id1] && velocities[id1] != null;
                bool movingEntity2 = !isStatic[id2] && velocities[id2] != null;
                
                if (movingEntity1 && movingEntity2)
                    twoMovementsResolutions.Add(
                        new TwoMovementsData(collisionDatas[i].Collision, id1, velocities[id1].Value * deltaTime, id2, velocities[id2].Value * deltaTime)
                    );
                else if (movingEntity1)
                    oneMovementResolutions.Add(
                        new OneMovementData(collisionDatas[i].Collision, id1, velocities[id1].Value * deltaTime)
                    );
                else if (movingEntity2)
                    oneMovementResolutions.Add(
                        new OneMovementData(collisionDatas[i].Collision, id2, velocities[id2].Value * deltaTime)
                    );
            }
        }

        private Dictionary<int, List<(Solution, int)>> GetSolutions(List<OneMovementData> oneMovementResolutions, List<TwoMovementsData> twoMovementsResolutions) {
            Dictionary<int, List<(Solution, int)>> entityToSolutions = new();
            
            foreach (var omr in oneMovementResolutions) {
                Correction correction = Solve(omr);
                List<(Solution, int)> entitySolutions = entityToSolutions.GetOrCreateValue(omr.Index);
                Solution solution = new Solution([omr.Index], [correction]);
                entitySolutions.Add((solution, 0));
            }

            foreach (var tmr in twoMovementsResolutions) {
                (Correction c1, Correction c2) = Solve(tmr);
                
                (int id1, int id2) = tmr.Indices;
                Solution solution = new Solution([id1, id2], [c1, c2]);
                
                List<(Solution, int)> entity1Solutions = entityToSolutions.GetOrCreateValue(tmr.Indices.Item1);
                List<(Solution, int)> entity2Solutions = entityToSolutions.GetOrCreateValue(tmr.Indices.Item2);

                entity1Solutions.Add((solution, 0));
                entity2Solutions.Add((solution, 1));
            }

            return entityToSolutions;
        }

        private Dictionary<int, Correction> GetSingleCorrections(Dictionary<int, List<(Solution, int)>> entityToSolutions) {
            Dictionary<int, Correction> entityToCorrections = new();
            HashSet<int> blockedEntities = new();
            
            foreach (var entityToSolution in entityToSolutions) {
                int id = entityToSolution.Key;

                if (blockedEntities.Contains(id))
                    continue;

                if (entityToSolution.Value.Count == 0)
                    continue;
                
                (Solution solution, int correctionIndex) = entityToSolution.Value[0];
                if (entityToSolution.Value.Count() == 1) { // if hit only 1 entity, moving or not : include
                    entityToCorrections.Add(id, solution.Corrections[correctionIndex]);
                    continue;
                }

                // More than 2 collisions
                bool hitMultipleEntitiesWithAtLeastOneMoving = false;
                Correction currentCorrection = solution.Corrections[correctionIndex];
                for (int i = 0; i < entityToSolution.Value.Count(); i++) {
                    (Solution evaluatedSolution, int evaluatedCorrectionIndex) = entityToSolution.Value[i];

                    if (evaluatedSolution.Entities.Length > 1) { // if collision involves another moving entity
                        hitMultipleEntitiesWithAtLeastOneMoving = true;
                        BlockEntity(id);
                        break;
                    }

                    Correction evaluatedCorrection = evaluatedSolution.Corrections[evaluatedCorrectionIndex];
                    if (evaluatedCorrection.HasPriorityAgainst(currentCorrection))
                        currentCorrection = evaluatedCorrection;

                }

                if (!hitMultipleEntitiesWithAtLeastOneMoving)
                    entityToCorrections.Add(id, currentCorrection);
            }

            void BlockEntity(int keyID) {
                blockedEntities.Add(keyID);

                if (!entityToCorrections.TryAdd(keyID, new Correction()))
                    entityToCorrections[keyID] = new Correction();

                List<(Solution, int)> solutionsList = entityToSolutions[keyID];
                entityToSolutions.Remove(keyID);

                foreach ((Solution solution, int _) in solutionsList) {
                    foreach (var collidingEntity in solution.Entities) {
                        if (blockedEntities.Contains(collidingEntity))
                            continue;

                        int solutionID = entityToSolutions[collidingEntity].FindIndex(x => x.Item1 == solution);
                        entityToSolutions[collidingEntity].RemoveAt(solutionID);

                        BlockEntity(collidingEntity);
                    }
                }
            }

            return entityToCorrections;
        }

        private Correction Solve(OneMovementData datas) {
            Vector2 correctedVelocity = GetCorrectedVelocity(datas.PenetrationDatas, datas.Velocity);
            return new Correction(correctedVelocity, correctedVelocity.Length());
        }

        private (Correction, Correction) Solve(TwoMovementsData datas) {            
            (Vector2 velocity1, Vector2 velocity2) = datas.Velocities;
            
            float speed1 = velocity1.Length();
            float speed2 = velocity2.Length();

            if (speed1 == 0 || speed2 == 0)
                return (new Correction(Vector2.Zero, 0.0f), new Correction(Vector2.Zero, 0.0f));

            float ratio = speed1 / (speed1 + speed2);

            Vector2 relativeVelocity = velocity2 - velocity1;

            Vector2 correctedRelativeVelocity = GetCorrectedVelocity(datas.PenetrationDatas, relativeVelocity);

            Vector2 correctedRelativeVelocityRatioed1 = correctedRelativeVelocity * ratio;
            Vector2 correctedRelativeVelocityRatioed2 = correctedRelativeVelocity * (1 - ratio);

            float correctedVelocity1Length = Vector2.Dot(- correctedRelativeVelocityRatioed1, Vector2.Normalize(velocity1));
            float correctedVelocity2Length = Vector2.Dot(correctedRelativeVelocityRatioed2, Vector2.Normalize(velocity2));

            Vector2 correctedVelocity1 = Vector2.Normalize(velocity1) * correctedVelocity1Length;
            Vector2 correctedVelocity2 = Vector2.Normalize(velocity2) * correctedVelocity2Length;

            return (new Correction(correctedVelocity1, correctedVelocity1Length), new Correction(correctedVelocity2, correctedVelocity2Length));
        }

        private Vector2 GetCorrectedVelocity(Penetration[] penetrationDatas, Vector2 velocity) {
            float biggestProjectedVelocityOnPenetration = 1.0f;

            for (int i = 0; i < penetrationDatas.Length; i++) {
                float projectedVelocityOnPenetration;
                projectedVelocityOnPenetration = Vector2.Dot(velocity, penetrationDatas[i].Axis);
                projectedVelocityOnPenetration = Math.Abs(projectedVelocityOnPenetration);
                projectedVelocityOnPenetration /= penetrationDatas[i].Length;
                if (projectedVelocityOnPenetration > biggestProjectedVelocityOnPenetration) {
                    biggestProjectedVelocityOnPenetration = projectedVelocityOnPenetration;
                }
            }

            float velocityRatioToKeep = 1.0f - 1.0f / biggestProjectedVelocityOnPenetration;
            return velocity * velocityRatioToKeep;
        }
    }
}

