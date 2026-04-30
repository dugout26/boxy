using UnityEngine;
using UnityEngine.UIElements;

namespace Boxy.App.UI
{
    // 첫 실행 시 Gameplay 씬 진입 직후 Onboarding.uxml overlay 표시.
    // boxy-plan §B-3-1 첫 5레벨 D1 retention 70% 결정 핵심 — Tutorial 진입 동기 부여.
    //
    // Inspector 연결:
    //   - GameObject에 UIDocument (Gameplay.uxml 로드)와 OnboardingController 모두 부착
    //   - onboardingAsset 필드에 Onboarding.uxml 드래그
    //
    // ProgressionService.tutorialCompleted == true 시 자동 스킵.
    public sealed class OnboardingController : MonoBehaviour
    {
        [SerializeField] VisualTreeAsset onboardingAsset;

        UIDocument uiDocument;
        VisualElement overlay;
        Button startButton;
        Button skipButton;

        void Start()
        {
            // BoxyBootstrap 진행도 확인 — 튜토리얼 완료한 유저에겐 표시 안 함
            if (BoxyBootstrap.Instance == null) return;
            if (BoxyBootstrap.Instance.Progression.Data.tutorialCompleted) return;
            if (onboardingAsset == null)
            {
                Debug.LogWarning("[OnboardingController] onboardingAsset 미연결. Onboarding 스킵.");
                return;
            }

            ShowOnboarding();
        }

        void OnDestroy()
        {
            if (startButton != null) startButton.clicked -= OnStart;
            if (skipButton != null) skipButton.clicked -= OnSkip;
        }

        void ShowOnboarding()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogWarning("[OnboardingController] UIDocument 컴포넌트 없음. 같은 GameObject에 추가 필요.");
                return;
            }

            var instance = onboardingAsset.Instantiate();
            instance.style.position = Position.Absolute;
            instance.style.left = 0;
            instance.style.top = 0;
            instance.style.right = 0;
            instance.style.bottom = 0;
            uiDocument.rootVisualElement.Add(instance);
            overlay = instance;

            startButton = overlay.Q<Button>("start-tutorial-button");
            skipButton = overlay.Q<Button>("skip-onboarding-button");

            if (startButton != null) startButton.clicked += OnStart;
            if (skipButton != null) skipButton.clicked += OnSkip;
        }

        void OnStart()
        {
            // 시작하기 = onboarding overlay 본 것 = 다시 표시 안 함.
            // 튜토리얼 자체(첫 5레벨)는 LevelClearedEvent 5번 발생 후 완료 처리됨.
            BoxyBootstrap.Instance?.Progression.MarkTutorialCompleted();
            Hide();
        }

        void OnSkip()
        {
            // 건너뛰기 — onboarding 다시 표시 안 함.
            BoxyBootstrap.Instance?.Progression.MarkTutorialCompleted();
            Hide();
        }

        void Hide()
        {
            if (overlay == null) return;
            overlay.RemoveFromHierarchy();
            overlay = null;
        }
    }
}
