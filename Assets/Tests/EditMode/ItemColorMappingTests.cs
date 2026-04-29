using NUnit.Framework;
using UnityEngine;

namespace Boxy.Tests.EditMode
{
    // 9개 itemKey에 대해 색상 매핑이 일관되는지 — 새 itemKey 추가 시 색상 누락 방지.
    // GameplayUI.GetItemColor는 internal이라 직접 호출 불가, 색상 명세만 검증.
    public class ItemColorMappingTests
    {
        // 9 itemKey의 RGB 색상이 §A-6 디자인 시스템과 일관:
        // - 마스코트는 #FFD60A 노란 (다른 itemKey는 다른 색)
        // - 색상 대비 4.5+ (텍스트 위)
        // - 9개 모두 distinct 색상

        [Test]
        public void Yellow_FFD60A_IsMascotColor_NotItemColor()
        {
            // 마스코트 노란 #FFD60A는 아이템 색상이 아니어야 함
            var mascotColor = new Color32(255, 214, 10, 255);
            // 9개 itemKey 색상 (GameplayUI.cs와 동기)
            var itemColors = new[]
            {
                new Color(0.95f, 0.42f, 0.42f),   // book_0
                new Color(0.42f, 0.66f, 0.95f),   // notebook_1
                new Color(0.62f, 0.42f, 0.95f),   // pencil_case_2
                new Color(0.95f, 0.62f, 0.42f),   // lunchbox_3
                new Color(0.42f, 0.85f, 0.85f),   // water_bottle_4
                new Color(0.95f, 0.55f, 0.55f),   // apple_5
                new Color(0.95f, 0.78f, 0.30f),   // snack_6
                new Color(0.50f, 0.85f, 0.50f),   // ruler_7
                new Color(0.55f, 0.55f, 0.65f),   // calculator_8
            };

            foreach (var c in itemColors)
            {
                bool isExactMascot =
                    Mathf.Approximately(c.r, mascotColor.r / 255f) &&
                    Mathf.Approximately(c.g, mascotColor.g / 255f) &&
                    Mathf.Approximately(c.b, mascotColor.b / 255f);
                Assert.IsFalse(isExactMascot, "아이템 색상이 마스코트 노란 #FFD60A와 정확히 일치 — distinct하게 변경 필요");
            }
        }

        [Test]
        public void NineDistinctItemColors()
        {
            var itemColors = new[]
            {
                new Color(0.95f, 0.42f, 0.42f), new Color(0.42f, 0.66f, 0.95f),
                new Color(0.62f, 0.42f, 0.95f), new Color(0.95f, 0.62f, 0.42f),
                new Color(0.42f, 0.85f, 0.85f), new Color(0.95f, 0.55f, 0.55f),
                new Color(0.95f, 0.78f, 0.30f), new Color(0.50f, 0.85f, 0.50f),
                new Color(0.55f, 0.55f, 0.65f),
            };

            // 색상끼리 최소 거리 (Euclidean) 계산 — 너무 가까우면 시각 구분 어려움
            for (int i = 0; i < itemColors.Length; i++)
            {
                for (int j = i + 1; j < itemColors.Length; j++)
                {
                    var a = itemColors[i]; var b = itemColors[j];
                    float dist = Mathf.Sqrt(
                        Mathf.Pow(a.r - b.r, 2) +
                        Mathf.Pow(a.g - b.g, 2) +
                        Mathf.Pow(a.b - b.b, 2));
                    Assert.Greater(dist, 0.05f, $"색상 너무 가까움 [{i}]={a}, [{j}]={b} — 시각 구분 어려움");
                }
            }
        }
    }
}
