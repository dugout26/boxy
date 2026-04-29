using NUnit.Framework;
using UnityEngine;
using Boxy.App.Gameplay;
using Boxy.App.Gameplay.Domain;

namespace Boxy.Tests.EditMode
{
    [TestFixture]
    public class UndoSystemTests
    {
        ItemShape Single() => ItemShape.FromCells(new[] { new Vector2Int(0, 0) });

        PlacementSnapshot MakeSnap(string key) =>
            new PlacementSnapshot(key, Single(), new Vector2Int(0, 0));

        [Test]
        public void NewSystem_HasInitialFreeUses()
        {
            var undo = new UndoSystem(initialFreeUses: 3);
            Assert.AreEqual(3, undo.FreeUsesRemaining);
            Assert.AreEqual(0, undo.PaidUsesRemaining);
            Assert.IsFalse(undo.AnyConsumed);
            Assert.IsFalse(undo.HasHistory);
        }

        [Test]
        public void RecordPlacement_BuildsHistory()
        {
            var undo = new UndoSystem();
            undo.RecordPlacement(MakeSnap("a"));
            Assert.IsTrue(undo.HasHistory);
        }

        [Test]
        public void TryConsumeFree_ReturnsFalse_WhenNoHistory()
        {
            var undo = new UndoSystem();
            Assert.IsFalse(undo.TryConsumeFree(out _));
            Assert.AreEqual(3, undo.FreeUsesRemaining);
        }

        [Test]
        public void TryConsumeFree_DecrementsRemaining()
        {
            var undo = new UndoSystem(initialFreeUses: 3);
            undo.RecordPlacement(MakeSnap("a"));
            undo.RecordPlacement(MakeSnap("b"));

            Assert.IsTrue(undo.TryConsumeFree(out var snap1));
            Assert.AreEqual("b", snap1.ItemKey);
            Assert.AreEqual(2, undo.FreeUsesRemaining);
            Assert.IsTrue(undo.AnyConsumed);
        }

        [Test]
        public void TryConsumeFree_ReturnsFalse_AfterAllFreeConsumed()
        {
            var undo = new UndoSystem(initialFreeUses: 1);
            undo.RecordPlacement(MakeSnap("a"));
            undo.RecordPlacement(MakeSnap("b"));

            Assert.IsTrue(undo.TryConsumeFree(out _));
            // free 0개 남음 — 두 번째 시도는 실패해야 함 (history 있어도)
            Assert.IsFalse(undo.TryConsumeFree(out _));
        }

        [Test]
        public void GrantPaidUse_AddsOne()
        {
            var undo = new UndoSystem();
            Assert.AreEqual(0, undo.PaidUsesRemaining);
            undo.GrantPaidUse();
            Assert.AreEqual(1, undo.PaidUsesRemaining);
        }

        [Test]
        public void TryConsumePaid_RequiresPaidUse()
        {
            var undo = new UndoSystem();
            undo.RecordPlacement(MakeSnap("a"));
            // paid 0개 → 실패
            Assert.IsFalse(undo.TryConsumePaid(out _));

            undo.GrantPaidUse();
            Assert.IsTrue(undo.TryConsumePaid(out var snap));
            Assert.AreEqual("a", snap.ItemKey);
            Assert.AreEqual(0, undo.PaidUsesRemaining);
        }

        [Test]
        public void ResetForNewLevel_RestoresFreeAndClearsHistory()
        {
            var undo = new UndoSystem(initialFreeUses: 3);
            undo.RecordPlacement(MakeSnap("a"));
            undo.RecordPlacement(MakeSnap("b"));
            undo.TryConsumeFree(out _);
            undo.GrantPaidUse();

            undo.ResetForNewLevel();

            Assert.AreEqual(3, undo.FreeUsesRemaining);
            Assert.AreEqual(0, undo.PaidUsesRemaining);
            Assert.IsFalse(undo.HasHistory);
            Assert.IsFalse(undo.AnyConsumed);
        }

        [Test]
        public void Constructor_NegativeUses_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new UndoSystem(initialFreeUses: -1));
        }
    }
}
