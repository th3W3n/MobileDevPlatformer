using Unity.Cinemachine;
using UnityEngine;

public class ScrollingBG : MonoBehaviour
{
    [System.Serializable]
    private class Layer
    {
        public SpriteRenderer spriteRenderer;
        [Tooltip("0 = fixed to the world (like the ground), 1 = moves with the camera (infinitely far)")]
        public Vector2 parallaxFactor;
        [HideInInspector] public float tileWidth;
    }

    [SerializeField] private Layer[] layers;

    private Transform cam;
    private Vector3 lastCamPos;
    private bool initialized;


    private void OnEnable() => CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    private void OnDisable() => CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    private void Start()
    {
        cam = Camera.main.transform;
        foreach (Layer layer in layers)
            layer.tileWidth = layer.spriteRenderer.sprite.bounds.size.x
                            * layer.spriteRenderer.transform.lossyScale.x;
    }

    private void OnCameraUpdated(CinemachineBrain brain)
    {
        if (cam == null) return;

        if (!initialized)
        {
            lastCamPos = cam.position;
            initialized = true;
            return;
        }

        Vector3 delta = cam.position - lastCamPos;
        lastCamPos = cam.position;

        foreach (Layer layer in layers)
        {
            Transform t = layer.spriteRenderer.transform;

            // 1. Parallax: follow a fraction of the camera's movement
            t.position += new Vector3(delta.x * layer.parallaxFactor.x,
                                      delta.y * layer.parallaxFactor.y, 0f);

            // 2. Looping: keep the layer within one tile of the camera
            while (cam.position.x - t.position.x >= layer.tileWidth)
                t.position += Vector3.right * layer.tileWidth;
            while (t.position.x - cam.position.x >= layer.tileWidth)
                t.position -= Vector3.right * layer.tileWidth;
        }
    }
}