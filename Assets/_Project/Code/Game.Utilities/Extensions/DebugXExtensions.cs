using DCFApixels;
using UnityEngine;

namespace Game.Utilities.Extensions
{
    public static class DebugXExtensions
    {
        public static DebugX.DrawHandler Collider(this DebugX.DrawHandler drawHandler, Collider collider)
        {
            switch (collider)
            {
                case BoxCollider boxCollider:
                {
                    var center = boxCollider.transform.TransformPoint(boxCollider.center);
                    var size = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);
                    return drawHandler.WireCube(center, boxCollider.transform.rotation, size);
                }
                case SphereCollider sphereCollider:
                {
                    var center = sphereCollider.transform.TransformPoint(sphereCollider.center);
                    var scale = sphereCollider.transform.lossyScale;
                    var radius = sphereCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                    return drawHandler.WireSphere(center, radius);
                }
                case CapsuleCollider capsuleCollider:
                {
                    var center = capsuleCollider.transform.TransformPoint(capsuleCollider.center);
                    var scale = capsuleCollider.transform.lossyScale;
                    Quaternion rotation;
                    float height, radius;
                    switch (capsuleCollider.direction)
                    {
                        case 0: // X
                            rotation = capsuleCollider.transform.rotation * Quaternion.Euler(0f, 0f, -90f);
                            height = capsuleCollider.height * Mathf.Abs(scale.x);
                            radius = capsuleCollider.radius * Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                            break;
                        case 2: // Z
                            rotation = capsuleCollider.transform.rotation * Quaternion.Euler(90f, 0f, 0f);
                            height = capsuleCollider.height * Mathf.Abs(scale.z);
                            radius = capsuleCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
                            break;
                        default: // Y
                            rotation = capsuleCollider.transform.rotation;
                            height = capsuleCollider.height * Mathf.Abs(scale.y);
                            radius = capsuleCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                            break;
                    }
                    return drawHandler.WireCapsule(center, rotation, radius, height);
                }
                default:
                {
                    var bounds = collider.bounds;
                    return drawHandler.WireCube(bounds.center, Quaternion.identity, bounds.size);
                }
            }
        }
    }
}