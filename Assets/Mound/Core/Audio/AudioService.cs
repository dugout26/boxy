using UnityEngine;

namespace Mound.Core.Audio
{
    public enum AudioCategory
    {
        Bgm,
        Sfx
    }

    public interface IAudioService
    {
        void PlayBgm(AudioClip clip, bool loop = true);
        void PlaySfx(AudioClip clip);
        void StopBgm();
        void SetVolume(AudioCategory category, float normalized);
        float GetVolume(AudioCategory category);
    }

    // Inspector 연결: bgmSource + sfxSource (각각 AudioSource 컴포넌트). 둘 다 같은 GameObject 또는 자식.
    // boxy-plan §B-5 UX 핵심: 딱 맞을 때 "탁" / 꽉 채울 때 팡파레 / 클리어 컨페티 — 각 사운드 트리거에서 PlaySfx.
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] AudioSource bgmSource;
        [SerializeField] AudioSource sfxSource;

        float bgmVolume = 0.5f;
        float sfxVolume = 0.8f;

        public void PlayBgm(AudioClip clip, bool loop = true)
        {
            if (bgmSource == null)
            {
                Debug.LogWarning("[AudioService] bgmSource가 Inspector에 연결 안 됨.");
                return;
            }
            if (clip == null) return;
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null)
            {
                Debug.LogWarning("[AudioService] sfxSource가 Inspector에 연결 안 됨.");
                return;
            }
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void StopBgm()
        {
            if (bgmSource != null) bgmSource.Stop();
        }

        public void SetVolume(AudioCategory category, float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            switch (category)
            {
                case AudioCategory.Bgm:
                    bgmVolume = normalized;
                    if (bgmSource != null) bgmSource.volume = bgmVolume;
                    break;
                case AudioCategory.Sfx:
                    sfxVolume = normalized;
                    break;
            }
        }

        public float GetVolume(AudioCategory category) => category switch
        {
            AudioCategory.Bgm => bgmVolume,
            AudioCategory.Sfx => sfxVolume,
            _ => 0f
        };
    }
}
