using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    [Order(OrderType.Before, typeof(RenderingSystem))]
    public class GridSystem : IDrawSystem
    {
        public void Draw()
        {
            Point point = neon.Components.GetOwner(Camera.Main).Get<Point>();
            Vector2 dimensions = Camera.Main.WorldDimensions;

            int baseX = (int)(point.WorldPosition.X - (dimensions.X / 2));
            int baseY = (int)(point.WorldPosition.Y - (dimensions.Y / 2));

            for (int i = 0; i < dimensions.X + 1; i++)
            {
                Vector2 startPosition = new Vector2(baseX + i, baseY - 1);
                Vector2 endPosition = new Vector2(baseX + i, baseY + dimensions.Y + 1);
                RenderingSystem.DrawLine(startPosition, endPosition, startPosition.X % 10 == 0 ? Color.DarkGray : Color.DimGray);
            }

            for (int i = 0; i < dimensions.Y + 1; i++)
            {
                Vector2 startPosition = new Vector2(baseX - 1, baseY + i);
                Vector2 endPosition = new Vector2(baseX + dimensions.X + 1, baseY + i);
                RenderingSystem.DrawLine(startPosition, endPosition, startPosition.Y % 10 == 0 ? Color.DarkGray : Color.DimGray);
            }
        }
    }
}

