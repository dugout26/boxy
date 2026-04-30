using System;
using UnityEngine;
using UnityEngine.UIElements;
using Mound.Core.Scenes;
using Mound.Localization;
using Boxy.App.Levels;

namespace Boxy.App.UI
{
    public sealed class ResultPopupController
    {
        readonly VisualElement overlay;
        readonly Label title;
        readonly Label[] stars;
        readonly Button nextButton;
        readonly Button retryButton;
        readonly Button menuButton;
        readonly ISceneLoader sceneLoader;

        // 외부에서 다음 레벨 / 재시도 처리 (GameplayUI가 핸들러 등록)
        public event Action OnNext;
        public event Action OnRetry;

        public ResultPopupController(VisualElement overlay, ISceneLoader sceneLoader)
        {
            this.overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
            this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));

            title = overlay.Q<Label>("result-title");
            stars = new[]
            {
                overlay.Q<Label>("result-star-1"),
                overlay.Q<Label>("result-star-2"),
                overlay.Q<Label>("result-star-3")
            };
            nextButton = overlay.Q<Button>("result-next-button");
            retryButton = overlay.Q<Button>("result-retry-button");
            menuButton = overlay.Q<Button>("result-menu-button");

            if (nextButton != null) nextButton.clicked += FireNext;
            if (retryButton != null) retryButton.clicked += FireRetry;
            if (menuButton != null) menuButton.clicked += GoToMenu;

            Hide();
        }

        public void Detach()
        {
            if (nextButton != null) nextButton.clicked -= FireNext;
            if (retryButton != null) retryButton.clicked -= FireRetry;
            if (menuButton != null) menuButton.clicked -= GoToMenu;
        }

        public void ShowCleared(int starsAwarded)
        {
            var strings = Boxy.App.BoxyBootstrap.Instance?.Strings;
            if (title != null)
                title.text = strings?.Get(StringKey.ResultClearedTitle) ?? "레벨 클리어!";
            // 버튼 라벨도 갱신
            if (nextButton != null && strings != null) nextButton.text = strings.Get(StringKey.NextLevelButton);
            if (retryButton != null && strings != null) retryButton.text = strings.Get(StringKey.RetryButton);
            if (menuButton != null && strings != null) menuButton.text = strings.Get(StringKey.MenuButton);
            UpdateStars(starsAwarded);
            SetMascotExpression("Mascot_cheer");
            if (nextButton != null) nextButton.style.display = DisplayStyle.Flex;
            overlay.style.display = DisplayStyle.Flex;
            AnimateStarsIn(starsAwarded);
            AnimateMascotBounce();
        }

        // 별 stagger 등장 — 0ms / 200ms / 400ms 차이로 pop-in.
        // USS @keyframes 미지원이라 Schedule.Execute로 transition 트리거.
        void AnimateStarsIn(int starCount)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] == null) continue;
                var star = stars[i];
                int idx = i;
                bool visible = idx < starCount;
                // 시작 상태 — 작게 + 투명
                star.style.scale = new StyleScale(new Scale(new Vector3(0f, 0f, 1f)));
                star.style.opacity = 0f;
                // 200ms 간격으로 등장
                star.schedule.Execute(() =>
                {
                    star.style.scale = new StyleScale(new Scale(new Vector3(1.2f, 1.2f, 1f)));
                    star.style.opacity = visible ? 1f : 0.25f;
                    // 0.2s 후 1.0으로 settle
                    star.schedule.Execute(() =>
                    {
                        star.style.scale = new StyleScale(new Scale(new Vector3(1f, 1f, 1f)));
                    }).StartingIn(200);
                }).StartingIn(idx * 200);
            }
        }

        void AnimateMascotBounce()
        {
            var mascot = overlay.Q<VisualElement>("result-mascot");
            if (mascot == null) return;
            mascot.style.scale = new StyleScale(new Scale(new Vector3(0.6f, 0.6f, 1f)));
            mascot.schedule.Execute(() =>
            {
                mascot.style.scale = new StyleScale(new Scale(new Vector3(1.1f, 1.1f, 1f)));
                mascot.schedule.Execute(() =>
                {
                    mascot.style.scale = new StyleScale(new Scale(new Vector3(1f, 1f, 1f)));
                }).StartingIn(300);
            }).StartingIn(50);
        }

        public void ShowFailed()
        {
            var strings = Boxy.App.BoxyBootstrap.Instance?.Strings;
            if (title != null)
                title.text = strings?.Get(StringKey.ResultFailedTitle) ?? "다시 도전!";
            if (retryButton != null && strings != null) retryButton.text = strings.Get(StringKey.RetryButton);
            if (menuButton != null && strings != null) menuButton.text = strings.Get(StringKey.MenuButton);
            UpdateStars(0);
            SetMascotExpression("Mascot_sad");
            if (nextButton != null) nextButton.style.display = DisplayStyle.None;
            overlay.style.display = DisplayStyle.Flex;
        }

        // 마스코트 PNG 동적 교체 — Resources나 Addressables 대신 직접 path → Texture 로딩.
        // 미존재 시 silent (이미 UXML에 default Mascot.png 박혀있음).
        void SetMascotExpression(string expressionName)
        {
            var mascot = overlay.Q<VisualElement>("result-mascot");
            if (mascot == null) return;
            var tex = Resources.Load<Texture2D>($"Mascot/{expressionName}");
            if (tex == null)
            {
                // Resources 폴더 외부의 Assets/Boxy.App/Icons/ 경로는 런타임에 로드 못 함.
                // 따라서 ResourcesAssetLoader를 통한 로드 또는 Addressables 필요.
                // v1.0: UXML default Mascot.png 그대로 사용 (단일 표정 fallback) — 5종 사용은 v1.1에서 Addressables.
                return;
            }
            mascot.style.backgroundImage = new StyleBackground(tex);
        }

        public void Hide() => overlay.style.display = DisplayStyle.None;

        void UpdateStars(int starCount)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] == null) continue;
                stars[i].style.opacity = i < starCount ? 1f : 0.25f;
            }
        }

        void FireNext() => OnNext?.Invoke();
        void FireRetry() => OnRetry?.Invoke();
        void GoToMenu() => sceneLoader.LoadScene(BoxySceneNames.MainMenu);
    }
}
