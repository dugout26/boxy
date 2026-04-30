using System;
using System.Collections.Generic;
using UnityEngine;
using Mound.Core.Save;

namespace Boxy.App.Save
{
    [Serializable]
    public sealed class BoxySaveData : SaveData
    {
        public int currentLevel = 1;
        public int unlockedLevel = 1;
        public bool tutorialCompleted;
        public bool adsRemoved;
        public int hintsOwned;
        public int paidUndosOwned;

        // CLAUDE.md §11-4 보안 / 안정성: 세이브 무결성 메타데이터.
        // checksum/HMAC은 SaveSystem에서 별도 PlayerPrefs key로 분리 저장 — 여기엔 안 들어감.
        public long createdAtUnixSec;          // 첫 저장 시각 (이주 ID 추적)
        public long updatedAtUnixSec;          // 마지막 저장 시각
        // 적용된 마이그레이션 히스토리 (예: "v0->v1@2026-05-01"). debug + 사후 추적용.
        [SerializeField] List<string> migrationHistory = new();
        public IReadOnlyList<string> MigrationHistory => migrationHistory;

        // 영수증 검증 후 성공한 IAP 트랜잭션 ID 목록 — 중복 지급 방지.
        [SerializeField] List<string> consumedIapTransactionIds = new();
        public IReadOnlyList<string> ConsumedIapTransactionIds => consumedIapTransactionIds;

        // CLAUDE.md §6-1: List public 노출 X. [SerializeField] private + IReadOnlyList view + internal 변경자
        [SerializeField] List<int> levelStars = new();
        public IReadOnlyList<int> LevelStars => levelStars;

        // 같은 어셈블리(Boxy.App)의 ProgressionService만 변경 가능. 외부 무단 수정 차단.
        internal void EnsureStarSlot(int index)
        {
            while (levelStars.Count <= index) levelStars.Add(0);
        }

        internal void SetStarAt(int index, int stars)
        {
            levelStars[index] = stars;
        }

        internal void Touch()
        {
            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (createdAtUnixSec == 0) createdAtUnixSec = now;
            updatedAtUnixSec = now;
        }

        internal void RecordMigration(string entry)
        {
            if (string.IsNullOrEmpty(entry)) return;
            migrationHistory.Add(entry);
        }

        internal bool IsIapConsumed(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId)) return false;
            return consumedIapTransactionIds.Contains(transactionId);
        }

        internal void MarkIapConsumed(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId)) return;
            if (!consumedIapTransactionIds.Contains(transactionId))
                consumedIapTransactionIds.Add(transactionId);
        }
    }
}
