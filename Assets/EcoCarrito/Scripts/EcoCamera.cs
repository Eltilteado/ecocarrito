using UnityEngine;

namespace EcoCarrito
{
    [RequireComponent(typeof(Camera))]
    public class EcoCamera : MonoBehaviour
    {
        Camera view;
        void OnEnable() { view = GetComponent<Camera>(); Fit(); }
        void Update() { Fit(); }
        void Fit()
        {
            if (!view) view = GetComponent<Camera>();
            float aspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            const float target = 16f / 9f;
            view.rect = aspect >= target
                ? new Rect((1 - target / aspect) * .5f, 0, target / aspect, 1)
                : new Rect(0, (1 - aspect / target) * .5f, 1, aspect / target);
            view.orthographic = true; view.orthographicSize = 5;
        }
    }
}
