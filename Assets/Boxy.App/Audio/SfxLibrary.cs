using UnityEngine;

namespace Boxy.App.Audio
{
    // 게임 SFX 매핑. 5종 placeholder는 Day 11~12에 실제 사운드 디자인으로 교체.
    // 사용: GameAudioBindings가 이벤트별로 PlaySfx(library.itemPlaced) 호출.
    [CreateAssetMenu(menuName = "Boxy/SFX Library", fileName = "SfxLibrary")]
    public sealed class SfxLibrary : ScriptableObject
    {
        public AudioClip uiTap;
        public AudioClip dragStart;
        public AudioClip itemPlaced;
        public AudioClip invalid;
        public AudioClip levelCleared;
        public AudioClip rotate;
        public AudioClip dropHover;
        public AudioClip starCount;
        public AudioClip bgmCozyLoop;
    }
}
