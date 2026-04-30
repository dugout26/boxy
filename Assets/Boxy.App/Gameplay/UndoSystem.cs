using System;
using System.Collections.Generic;
using UnityEngine;
using Boxy.App.Gameplay.Domain;

namespace Boxy.App.Gameplay
{
    public readonly struct PlacementSnapshot
    {
        public readonly string ItemKey;
        public readonly ItemShape Shape;
        public readonly Vector2Int Anchor;

        public PlacementSnapshot(string itemKey, ItemShape shape, Vector2Int anchor)
        {
            ItemKey = itemKey;
            Shape = shape;
            Anchor = anchor;
        }
    }

    // boxy-plan §B-2-2: 무료 3회 + 광고 시청 추가권
    public sealed class UndoSystem
    {
        readonly Stack<PlacementSnapshot> history;
        readonly int initialFreeUses;
        int freeUsesRemaining;
        int paidUsesRemaining;

        public int FreeUsesRemaining => freeUsesRemaining;
        public int PaidUsesRemaining => paidUsesRemaining;
        public bool AnyConsumed { get; private set; }
        public bool HasHistory => history.Count > 0;

        public UndoSystem(int initialFreeUses = 3)
        {
            if (initialFreeUses < 0)
                throw new ArgumentOutOfRangeException(nameof(initialFreeUses));

            this.initialFreeUses = initialFreeUses;
            history = new Stack<PlacementSnapshot>();
            ResetForNewLevel();
        }

        public void RecordPlacement(in PlacementSnapshot snap)
        {
            history.Push(snap);
        }

        // 광고 시청 후 호출 — 1회 사용권 추가
        public void GrantPaidUse()
        {
            paidUsesRemaining++;
        }

        public bool TryConsumeFree(out PlacementSnapshot snap)
        {
            snap = default;
            if (freeUsesRemaining <= 0 || history.Count == 0) return false;

            freeUsesRemaining--;
            AnyConsumed = true;
            snap = history.Pop();
            return true;
        }

        public bool TryConsumePaid(out PlacementSnapshot snap)
        {
            snap = default;
            if (paidUsesRemaining <= 0 || history.Count == 0) return false;

            paidUsesRemaining--;
            AnyConsumed = true;
            snap = history.Pop();
            return true;
        }

        public void ResetForNewLevel()
        {
            history.Clear();
            freeUsesRemaining = initialFreeUses;
            paidUsesRemaining = 0;
            AnyConsumed = false;
        }
    }
}
