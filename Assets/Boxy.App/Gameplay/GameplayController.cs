using System;
using UnityEngine;
using Mound.Core.Events;
using Boxy.App.Gameplay.Domain;
using Boxy.App.Gameplay.Events;
using Boxy.App.Levels;

namespace Boxy.App.Gameplay
{
    // 게임플레이 루트 컨트롤러. 그리드 + Undo + EventBus 통합. 입력 처리는 별도 컴포넌트가 호출.
    // 사용 순서: Bind(eventBus) → StartLevel(level) → TryPlace/TryUndo*
    public sealed class GameplayController : MonoBehaviour
    {
        BoxyGrid grid;
        UndoSystem undo;
        IEventBus eventBus;
        LevelData currentLevel;
        bool usedHint;

        public BoxyGrid Grid => grid;
        public UndoSystem Undo => undo;
        public LevelData CurrentLevel => currentLevel;

        public void Bind(IEventBus bus)
        {
            eventBus = bus;
        }

        public void StartLevel(LevelData level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));

            // CLAUDE.md §11-2 조용한 실패 회피 — Bind() 누락 시 즉시 경고. 게임 진행 자체는 막지 않음 (이벤트만 손실)
            if (eventBus == null)
            {
                Debug.LogWarning("[GameplayController] EventBus not bound — events won't fire. Call Bind(IEventBus) before StartLevel.");
            }

            currentLevel = level;
            grid = new BoxyGrid(level.GridWidth, level.GridHeight);
            undo = new UndoSystem(initialFreeUses: 3);
            usedHint = false;

            eventBus?.Publish(new LevelStartedEvent(level.Id.Value));
        }

        public bool TryPlace(string itemKey, ItemShape shape, Vector2Int anchor)
        {
            if (grid == null) return false;
            if (PlacementValidator.Check(grid, shape, anchor) != PlacementResult.Valid) return false;

            grid.Place(itemKey, shape, anchor);
            undo.RecordPlacement(new PlacementSnapshot(itemKey, shape, anchor));
            eventBus?.Publish(new ItemPlacedEvent(itemKey, anchor));

            CheckClearCondition();
            return true;
        }

        public bool TryUndoFree()
        {
            if (!undo.TryConsumeFree(out var snap)) return false;
            grid.Remove(snap.ItemKey);
            eventBus?.Publish(new ItemRemovedEvent(snap.ItemKey));
            return true;
        }

        public bool TryUndoPaid()
        {
            if (!undo.TryConsumePaid(out var snap)) return false;
            grid.Remove(snap.ItemKey);
            eventBus?.Publish(new ItemRemovedEvent(snap.ItemKey));
            return true;
        }

        // 광고 시청 후 호출
        public void GrantPaidUndo() => undo?.GrantPaidUse();

        public void MarkHintUsed() => usedHint = true;

        void CheckClearCondition()
        {
            if (!grid.IsFull()) return;

            int stars = ComputeStars();
            eventBus?.Publish(new LevelClearedEvent(
                currentLevel.Id.Value, stars, usedHint, undo.AnyConsumed));
        }

        // boxy-plan §B-2-1 단순 부울 체크 2개
        int ComputeStars()
        {
            bool noHint = !usedHint;
            bool noUndo = !undo.AnyConsumed;
            if (noHint && noUndo) return 3;
            if (noHint) return 2;
            return 1;
        }
    }
}
