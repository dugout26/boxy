using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mound.Core
{
    // CLAUDE.md §11-4 P2 (v1.1 리팩터링): Addressables 도입 시 Boxy.App에서 Resources.Load 대신 IAssetProvider.LoadAsync 사용.
    // SDK 통합 가이드: decisions/2026-04-30-21-v1.1-refactor-plan.md
    //
    // 도입 단계:
    //   1) v1.0: ResourcesAssetProvider 사용 — 기존 Resources/ 폴더 그대로
    //   2) v1.1: Addressables 패키지 추가 → AddressablesAssetProvider 구현 → BoxyBootstrap 교체
    //   3) Asset 폴더 점진적 Addressable 전환 (마스코트 → 아이템 → 사운드 순)
    public interface IAssetProvider
    {
        Task<T> LoadAsync<T>(string key, CancellationToken ct = default) where T : UnityEngine.Object;
        void Release(UnityEngine.Object asset);
    }

    // v1.0 default — Resources.Load 동기 호출을 Task로 감싸 인터페이스 호환.
    // v1.1에서 AddressablesAssetProvider로 교체 시 caller 코드 변경 X.
    public sealed class ResourcesAssetProvider : IAssetProvider
    {
        public Task<T> LoadAsync<T>(string key, CancellationToken ct = default) where T : UnityEngine.Object
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var asset = Resources.Load<T>(key);
                if (asset == null)
                    Debug.LogWarning($"[ResourcesAssetProvider] '{key}' not found (type {typeof(T).Name})");
                return Task.FromResult(asset);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return Task.FromResult<T>(null);
            }
        }

        public void Release(UnityEngine.Object asset)
        {
            // Resources.Load는 Resources.UnloadUnusedAssets()에서 일괄 정리. per-asset release 미지원.
            // Addressables 도입 후엔 정확한 release 필요.
        }
    }

    // v1.1 도입 시 활성. UPM 패키지 com.unity.addressables 설치 후 Addressables.LoadAssetAsync로 교체.
    public sealed class AddressablesAssetProvider : IAssetProvider
    {
        public AddressablesAssetProvider()
        {
            Debug.LogWarning("[AddressablesAssetProvider] v1.1 백로그 — Addressables 패키지 설치 후 활성. 현재 Resources fallback로 동작.");
        }

        public Task<T> LoadAsync<T>(string key, CancellationToken ct = default) where T : UnityEngine.Object
        {
            // SDK 통합 후:
            //   var handle = Addressables.LoadAssetAsync<T>(key);
            //   return handle.Task;
            return new ResourcesAssetProvider().LoadAsync<T>(key, ct);
        }

        public void Release(UnityEngine.Object asset)
        {
            // SDK 통합 후: Addressables.Release(asset);
        }
    }
}
