using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using neon;

namespace neongine {
    public class VelocityCollisionResolver : ICollisionResolver
    {
        private struct OneMovementResolution {
            public Penetration[] PenetrationDatas;
            public int Index;
            public Vector2 Velocity;

            public OneMovementResolution(Collision collision, int index, Vector2 velocity) {
                PenetrationDatas = collision.Penetration;
                Index = index;
                Velocity = velocity;
            }
        }

        private struct TwoMovementsResolution {
            public Penetration[] PenetrationDatas;
            public (int, int) Indices;
            public (Vector2, Vector2) Velocities;
            public TwoMovementsResolution(Collision collision, int index1, Vector2 velocity1, int index2, Vector2 velocity2) {
                PenetrationDatas = collision.Penetration;
                Indices = (index1, index2);
                Velocities = (velocity1, velocity2);
            }
        }

        private struct Solution {
            public Vector2 Velocity;
            public float Length;
            public Solution(Vector2 velocity, float length) {
                Velocity = velocity;
                Length = length;
            }
        }

        public void Resolve(CollisionData[] collisionDatas, EntityID[] entityIDs, Velocity[] velocities, bool[] isStatic)
        {
            List<OneMovementResolution> oneMovementResolutions = new();
            List<TwoMovementsResolution> twoMovementsResolutions = new();

            for (int i = 0; i < collisionDatas.Length; i++) {
                int id1 = Array.FindIndex(entityIDs, id => id == collisionDatas[i].Entities.Item1);
                int id2 = Array.FindIndex(entityIDs, id => id == collisionDatas[i].Entities.Item2);
                bool movingEntity1 = !isStatic[id1] && velocities[id1] != null;
                bool movingEntity2 = !isStatic[id2] && velocities[id2] != null;
                if (movingEntity1 && movingEntity2)
                    twoMovementsResolutions.Add(
                        new TwoMovementsResolution(collisionDatas[i].Collision, id1, velocities[id1].Value, id2, velocities[id2].Value)
                    );
                else if (movingEntity1)
                    oneMovementResolutions.Add(
                        new OneMovementResolution(collisionDatas[i].Collision, id1, velocities[id1].Value)
                    );
                else if (movingEntity2)
                    oneMovementResolutions.Add(
                        new OneMovementResolution(collisionDatas[i].Collision, id2, velocities[id2].Value)
                    );
            }

            Dictionary<int, Solution> oneSolutions = new();

            foreach (var omr in oneMovementResolutions) {
                Solution solution = Solve(omr);
                if (oneSolutions.TryGetValue(omr.Index, out Solution currentSolution)) {
                    if (Priority(solution, currentSolution))
                        oneSolutions[omr.Index] = solution;
                    continue;
                }

                oneSolutions.Add(omr.Index, solution);
            }

            // NOT SAFE
            foreach (var tmr in twoMovementsResolutions) {
                (Solution solution1, Solution solution2) = Solve(tmr);
                oneSolutions.Add(tmr.Indices.Item1, solution1);
                oneSolutions.Add(tmr.Indices.Item2, solution2);
            }

            // Dictionary<int, List<(Solution, int)>> twoSolutions = new();

            // foreach (var tmr in twoMovementsResolutions) {
            //     (Solution solution1, Solution solution2) = Solve(tmr);
            //     if (!twoSolutions.ContainsKey(tmr.Index1) && !twoSolutions.ContainsKey(tmr.Index2)) {
            //         twoSolutions.Add(tmr.Index1, new List<(Solution, int)> {
            //             (solution1, tmr.Index2)
            //         });

            //         twoSolutions.Add(tmr.Index2, new List<(Solution, int)> {
            //             (solution2, tmr.Index1)
            //         });
            //     } else if (twoSolutions.TryGetValue(tmr.Index1, out var currentSolutions1) && !twoSolutions.ContainsKey(tmr.Index2)) {
            //         if (!Priority(solution1, currentSolutions1[0].Item1)) {
            //             currentSolutions1.Add((solution1, tmr.Index2));
            //             twoSolutions.Add(tmr.Index2, new List<(Solution, int)> {
            //                 (solution2, tmr.Index1)
            //             });
            //         } else  {

            //         }
            //     } else if ((!twoSolutions.TryGetValue(tmr.Index1, out var currentSolutions1) || Priority(solution1, currentSolutions1[0].Item1))
            //         && (!twoSolutions.TryGetValue(tmr.Index2, out var currentSolutions2) || Priority(solution2, currentSolutions2[0].Item1))) {
            //         currentSolutions1.Insert(0, (solution1, tmr.Index2));

            //         int otherIndex = currentSolutions1
            //     }


            //     if (oneSolutions.TryGetValue(tmr.Index1, out Solution currentSolution1) && Priority(solution1, currentSolution1)) {
            //         oneSolutions[tmr.Index1] = solution1;
            //         oneSolutions.Remove(tmr.Index2); // No, need to rollback to previous solution if there was one
            //         // + Keep the other one in current Solution ! Currently have only the new one
            //         continue;
            //     }

            //     oneSolutions.Add(tmr.Index, solution);
            // }

            foreach (var solution in oneSolutions) {
                velocities[solution.Key].Value = solution.Value.Velocity;
            }

            // Case 1 : it they have no Bounce component
                // If only 1 has velocity (& no static, of couse) -> update it linearly
                // If both have velocity -> find where to relocate them based on their velocity (& velocity "speed")
            
            
            // Case 2 : one of them has a bounce component
                // If the other one is static : update the first according to its velocity
                // If the other one is not static : ???
                // If the other one also has a bounce component : ???
            
            // Make sure to calculate all the results before applying them : we still have to check for multiple collisions happening at the same time for the same object
        }

        private bool Priority(Solution s1, Solution s2) => s1.Length < s2.Length;

        private Solution Solve(OneMovementResolution datas) {
            Vector2 correctedVelocity = GetCorrectedVelocity(datas.PenetrationDatas, datas.Velocity);
            return new Solution(correctedVelocity, correctedVelocity.Length());
        }

        private (Solution, Solution) Solve(TwoMovementsResolution datas) {            
            (Vector2 velocity1, Vector2 velocity2) = datas.Velocities;
            
            float speed1 = velocity1.Length();
            float speed2 = velocity2.Length();

            if (speed1 == 0 || speed2 == 0)
                return (new Solution(Vector2.Zero, 0.0f), new Solution(Vector2.Zero, 0.0f));

            float ratio = speed1 / (speed1 + speed2);

            Vector2 relativeVelocity = velocity2 - velocity1;

            Vector2 correctedRelativeVelocity = GetCorrectedVelocity(datas.PenetrationDatas, relativeVelocity);

            Vector2 correctedRelativeVelocityRatioed1 = correctedRelativeVelocity * ratio;
            Vector2 correctedRelativeVelocityRatioed2 = correctedRelativeVelocity * (1 - ratio);

            float correctedVelocity1Length = Vector2.Dot(- correctedRelativeVelocityRatioed1, Vector2.Normalize(velocity1));
            float correctedVelocity2Length = Vector2.Dot(correctedRelativeVelocityRatioed2, Vector2.Normalize(velocity2));

            Vector2 correctedVelocity1 = Vector2.Normalize(velocity1) * correctedVelocity1Length;
            Vector2 correctedVelocity2 = Vector2.Normalize(velocity2) * correctedVelocity2Length;

            return (new Solution(correctedVelocity1, correctedVelocity1Length), new Solution(correctedVelocity2, correctedVelocity2Length));
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

