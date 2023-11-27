using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
public class ARSessionOriginAlignment : Singleton<ARSessionOriginAlignment>
{
    public ARSessionOrigin arSessionOrigin;
    private Vector3 sharedAnchorPosition; // 从共享锚点获取的位置
    private Quaternion sharedAnchorRotation; // 从共享锚点获取的旋转

    public void InitSharedAnchor(ARAnchor anchor)
    {
        sharedAnchorPosition = anchor.transform.position;
        sharedAnchorRotation = anchor.transform.rotation;
    }

    public void AlignWithSharedAnchor()
    {
        if (arSessionOrigin == null)
            return;

        // 计算 AR Session Origin 相对于共享锚点的偏移
        Vector3 positionOffset = sharedAnchorPosition - arSessionOrigin.transform.position;
        Quaternion rotationOffset = sharedAnchorRotation * Quaternion.Inverse(arSessionOrigin.transform.rotation);

        // 应用偏移来校准 AR Session Origin
        arSessionOrigin.transform.position += positionOffset;
        arSessionOrigin.transform.rotation *= rotationOffset;
    }
}

