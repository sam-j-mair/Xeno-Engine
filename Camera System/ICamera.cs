
using Microsoft.Xna.Framework;

namespace XenoEngine
{
    /// <summary>
    /// Camera Object interface.
    /// </summary>
    public interface ICamera
    {
        Matrix ViewMatrix { get; }
        Matrix ProjectionMatrix { get; }
        Matrix WorldMatrix { get; }
        BoundingFrustum Frustrum { get; }
        Vector3 Position { get; set; }
        Vector3 TargetPosition { get; }
        Vector3 Forward { get; }
        Vector3 SideVector { get; }
        Vector3 UpVector { get; }
        float UpDownRot { get; set; }
        float LeftRightRot { get; set; }
        bool FixedMousePosition { get; set; }
        float CameraMoveSpeed { get; set; }
    }
}
