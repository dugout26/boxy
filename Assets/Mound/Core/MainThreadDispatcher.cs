using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Mound.Core
{
    // CLAUDE.md §11-4 P0-4 SDK 콜백 메인스레드 디스패치 강제.
    // AppLovin / Firebase / AppsFlyer 네이티브 콜백은 메인 스레드 보장 X.
    // Unity API (PlayerPrefs, Object.Destroy, UI 갱신 등)는 메인 스레드에서만 호출 가능 — 위반 시 즉시 크래시.
    //
    // 사용:
    //   MainThreadDispatcher.Enqueue(() => SomeUnityApiCall());
    //   await MainThreadDispatcher.SwitchToMainThreadAsync();
    //
    // V1.1 UniTask 도입 시 UniTask.SwitchToMainThread()로 교체 권장.
    [DisallowMultipleComponent]
    public sealed class MainThreadDispatcher : MonoBehaviour
    {
        static readonly ConcurrentQueue<Action> Queue = new ConcurrentQueue<Action>();
        static int mainThreadId;
        static MainThreadDispatcher instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            if (instance != null) return;
            var go = new GameObject("[MainThreadDispatcher]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<MainThreadDispatcher>();
        }

        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == mainThreadId;

        public static void Enqueue(Action action)
        {
            if (action == null) return;
            // 이미 메인스레드면 즉시 실행 (불필요 큐잉 회피)
            if (IsMainThread)
            {
                action();
                return;
            }
            Queue.Enqueue(action);
        }

        // 콜백 패턴 — completionSource 통해 대기.
        // System.Threading.Tasks.TaskCompletionSource로 await 가능.
        public static System.Threading.Tasks.Task SwitchToMainThreadAsync()
        {
            if (IsMainThread) return System.Threading.Tasks.Task.CompletedTask;
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            Enqueue(() => tcs.TrySetResult(true));
            return tcs.Task;
        }

        void Update()
        {
            // 매 프레임 큐 드레인. 너무 많은 액션 누적 방지 위해 한 프레임당 최대 64개만 처리.
            int processed = 0;
            while (processed < 64 && Queue.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                processed++;
            }
        }
    }
}
