using SharpityEngine;

namespace TestGame
{
    public class TestScript : BehaviourScript
    {
        // Methods
        public override void OnUpdate(GameTime gameTime)
        {
            Point2 mouse = Input.MousePosition;

            Vector2 nrmMouseCoords = new Vector2(
                (float)(mouse.X * 1 - Window.Width * 0.5f) / Window.Width,
                (float)(mouse.Y * 1 - Window.Height * 0.5f) / Window.Height);

            Matrix4 mat = Matrix4.RotationY(
                        Mathf.RadToDeg * MathF.Sign(nrmMouseCoords.X) * (MathF.Log(Math.Abs(nrmMouseCoords.X) + 1)) * 0.9f
                    ) *
                    Matrix4.RotationX(
                        Mathf.RadToDeg * MathF.Sign(nrmMouseCoords.Y) * (MathF.Log(Math.Abs(nrmMouseCoords.Y) + 1)) * 0.9f
                    ) *
                    Matrix4.UniformScale(
                        (float)(1 + 0.1 * Math.Sin(gameTime.TimeSeconds * 2.0))
                    ) *
                    Matrix4.Translate(0, 0, -5);


            Transform.LocalRotation = Quaternion.Euler(
                Mathf.RadToDeg * MathF.Sign(nrmMouseCoords.Y) * (MathF.Log(Math.Abs(nrmMouseCoords.Y) + 1)) * 0.9f, 
                Mathf.RadToDeg * MathF.Sign(nrmMouseCoords.X) * (MathF.Log(Math.Abs(nrmMouseCoords.X) + 1)) * 0.9f, 
                0f);

            Transform.LocalScale = Vector3.One * (float)(1 + 0.1 * Math.Sin(gameTime.TimeSeconds * 2.0));


        }
    }
}
