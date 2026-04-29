using UnityEngine;
using UnityEngine.UIElements;

namespace Mound.UI
{
    // iPhone notch / Dynamic Island, Android punch-hole, iPad bezel safe area를 UI Toolkit에 자동 적용.
    // 표준 패턴 — Screen.safeArea 픽셀 → RuntimePanelUtils.ScreenToPanel로 panel 좌표 변환.
    // 직접 비율 계산은 panel scaleMode/match에 따라 동작 다름 → API 사용이 정답.
    //
    // decisions/2026-04-29-11-responsive-layout.md §3 표준 채택.
    //
    // 사용:
    //   1. 자동 부착 (RuntimeInitializeOnLoadMethod): 모든 씬의 UIDocument에 자동
    //   2. 수동 부착: UIDocument GameObject에 컴포넌트 추가
    [RequireComponent(typeof(UIDocument))]
    [DefaultExecutionOrder(-50)]
    public sealed class SafeAreaController : MonoBehaviour
    {
        UIDocument doc;
        VisualElement target;

        // 모든 씬 로드 후 UIDocument에 SafeAreaController 자동 부착
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoAttach()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => AttachToAll();
            AttachToAll();
        }

        static void AttachToAll()
        {
            foreach (var d in Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            {
                if (d.GetComponent<SafeAreaController>() == null)
                    d.gameObject.AddComponent<SafeAreaController>();
            }
        }

        void OnEnable()
        {
            doc = GetComponent<UIDocument>();
        }

        void Update()
        {
            // root 준비됐고 아직 등록 안 됐으면 등록.
            if (target == null && doc != null && doc.rootVisualElement != null)
            {
                // padding을 UIDocument의 root 자식(UXML root)에 적용 — 자식이 background-color 가지므로 padding 영역도 색칠됨.
                target = doc.rootVisualElement.childCount > 0
                    ? doc.rootVisualElement[0]
                    : doc.rootVisualElement;
                target.RegisterCallback<GeometryChangedEvent>(_ => Apply());
                Apply();
            }
        }

        void Apply()
        {
            if (target == null || target.panel == null) return;

            Rect safe = Screen.safeArea;
            float h = Screen.height;

            // 픽셀 좌표 (Unity는 좌하단 원점) → panel 좌표 (UI Toolkit은 좌상단 원점) 변환.
            // ScreenToPanel은 PanelSettings의 scaleMode/match 모드를 자동 반영.
            Vector2 lt = RuntimePanelUtils.ScreenToPanel(
                target.panel,
                new Vector2(safe.xMin, h - safe.yMax));
            Vector2 rb = RuntimePanelUtils.ScreenToPanel(
                target.panel,
                new Vector2(Screen.width - safe.xMax, safe.yMin));

            target.style.paddingLeft = lt.x;
            target.style.paddingTop = lt.y;
            target.style.paddingRight = rb.x;
            target.style.paddingBottom = rb.y;
        }
    }
}
