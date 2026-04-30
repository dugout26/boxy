using UnityEngine;
using Mound.Core.Audio;
using Mound.Core.Events;
using Mound.Core.Haptic;
using Boxy.App.Gameplay.Events;

namespace Boxy.App.Audio
{
    // EventBus 게임 이벤트 구독 → SFX 재생 + 햅틱 트리거.
    // AudioService가 실제 재생, 매핑은 SfxLibrary. 햅틱은 IHapticService.
    // §11-1 빈 catch 금지 — null 시 조용히 disable (게임플레이 차단 안 함).
    public sealed class GameAudioBindings
    {
        readonly IAudioService audio;
        readonly SfxLibrary library;
        readonly IEventBus eventBus;
        readonly IHapticService haptic;
        bool subscribed;

        public GameAudioBindings(IAudioService audio, SfxLibrary library, IEventBus eventBus, IHapticService haptic = null)
        {
            this.audio = audio;
            this.library = library;
            this.eventBus = eventBus;
            this.haptic = haptic ?? new NullHapticService();
        }

        public void Bind()
        {
            if (audio == null || library == null || eventBus == null) return;
            if (subscribed) return;

            eventBus.Subscribe<ItemPlacedEvent>(OnItemPlaced);
            eventBus.Subscribe<LevelClearedEvent>(OnLevelCleared);
            eventBus.Subscribe<ItemRemovedEvent>(OnItemRemoved);
            subscribed = true;

            // BGM 시작 — 코지 루프 (Setting 슬라이더로 볼륨 조절 가능)
            if (library.bgmCozyLoop != null)
            {
                audio.PlayBgm(library.bgmCozyLoop, loop: true);
            }
        }

        public void Unbind()
        {
            if (eventBus == null || !subscribed) return;
            eventBus.Unsubscribe<ItemPlacedEvent>(OnItemPlaced);
            eventBus.Unsubscribe<LevelClearedEvent>(OnLevelCleared);
            eventBus.Unsubscribe<ItemRemovedEvent>(OnItemRemoved);
            subscribed = false;
        }

        void OnItemPlaced(ItemPlacedEvent _)
        {
            audio.PlaySfx(library.itemPlaced);
            haptic.Trigger(HapticIntensity.Medium);
        }
        void OnLevelCleared(LevelClearedEvent _)
        {
            audio.PlaySfx(library.levelCleared);
            haptic.Trigger(HapticIntensity.Heavy);
        }
        void OnItemRemoved(ItemRemovedEvent _)
        {
            audio.PlaySfx(library.uiTap);
            haptic.Trigger(HapticIntensity.Light);
        }

        public void PlayUiTap()
        {
            audio?.PlaySfx(library?.uiTap);
            haptic.Trigger(HapticIntensity.Light);
        }
        public void PlayDragStart()
        {
            audio?.PlaySfx(library?.dragStart);
            haptic.Trigger(HapticIntensity.Light);
        }
        public void PlayInvalid()
        {
            audio?.PlaySfx(library?.invalid);
            haptic.Trigger(HapticIntensity.Medium);
        }
        public void PlayRotate()
        {
            audio?.PlaySfx(library?.rotate);
            haptic.Trigger(HapticIntensity.Light);
        }
        public void PlayDropHover()
        {
            audio?.PlaySfx(library?.dropHover);
        }
        public void PlayStarCount()
        {
            audio?.PlaySfx(library?.starCount);
            haptic.Trigger(HapticIntensity.Light);
        }
    }
}
