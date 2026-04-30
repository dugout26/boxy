// Boxy Pack — Gameplay + Result + Ad screens
const { T, Icon, Card, PrimaryBtn, SecondaryBtn, GhostBtn, IconBtn, Pill, BoxyScreen, Topbar, MascotFrame } = window;

// 6x6 grid. 1 = filled, 0 = empty. ~70% filled state.
const GAMEPLAY_GRID = [
  [1,1,1,1,0,0],
  [1,1,1,1,1,0],
  [2,2,1,1,1,0],
  [2,2,3,3,3,0],
  [4,4,3,5,5,0],
  [4,0,0,5,5,0],
];

const PIECE_COLORS = {
  1: '#F59E4B', // active orange
  2: '#7CB342', // green
  3: '#E87764', // terracotta
  4: '#7CC5E8', // soft blue
  5: '#C28E5A', // brown
};

function GameplayScreen({ nav, tw }) {
  const completion = 22; // out of 36 = 61%

  return (
    <BoxyScreen>
      {/* Top bar */}
      <div style={{ display: 'flex', alignItems: 'center', padding: '14px 16px', gap: 10, borderBottom: `1px solid ${T.borderSoft}` }}>
        <IconBtn size={40} onClick={() => nav.go('levels')}>{Icon.pause()}</IconBtn>
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4 }}>
          <div style={{ fontSize: 15, fontWeight: 800, letterSpacing: '-0.3px' }}>Level 1-7</div>
          <div style={{ width: '70%', height: 6, background: T.borderSoft, borderRadius: 999, overflow: 'hidden' }}>
            <div style={{ width: `${(completion/36)*100}%`, height: '100%', background: T.accent }}/>
          </div>
        </div>
        <IconBtn size={40}>{Icon.bulb()}</IconBtn>
        <IconBtn size={40}>{Icon.undo()}</IconBtn>
      </div>

      {/* Canvas */}
      <div style={{ padding: '20px 20px 12px', display: 'flex', justifyContent: 'center' }}>
        <div style={{
          position: 'relative',
          width: '100%', aspectRatio: '1 / 1',
          background: 'linear-gradient(135deg, #B8865A 0%, #8B5E2C 100%)',
          borderRadius: 16, padding: 8,
          boxShadow: 'inset 0 4px 12px rgba(0,0,0,0.25), 0 4px 12px rgba(160,90,40,0.2)',
        }}>
          <div style={{
            display: 'grid', gridTemplateColumns: 'repeat(6, 1fr)', gridTemplateRows: 'repeat(6, 1fr)',
            gap: 3, width: '100%', height: '100%',
          }}>
            {GAMEPLAY_GRID.flat().map((cell, i) => (
              <div key={i} style={{
                background: cell ? PIECE_COLORS[cell] : 'rgba(255,255,255,0.08)',
                borderRadius: 4,
                border: cell ? '1px solid rgba(0,0,0,0.12)' : '1px solid rgba(255,255,255,0.1)',
                boxShadow: cell ? 'inset 0 2px 0 rgba(255,255,255,0.25), inset 0 -2px 0 rgba(0,0,0,0.12)' : 'none',
              }}/>
            ))}
          </div>
          {/* mascot in corner */}
          <div style={{ position: 'absolute', top: -16, right: -10, transform: 'rotate(8deg)' }}>
            <BoxyMascot pose="thinking" size={68}/>
          </div>
        </div>
      </div>

      {/* Item tray */}
      <div style={{ padding: '8px 16px 20px' }}>
        <Card padding={12} radius={16}>
          <div style={{ display: 'flex', gap: 10, overflowX: 'auto', paddingBottom: 4 }}>
            {[
              { c: T.accent, shape: 'L' },
              { c: '#7CB342', shape: 'I' },
              { c: '#E87764', shape: 'T' },
            ].map((it, i) => (
              <div key={i} style={{
                position: 'relative',
                minWidth: 96, height: 96,
                background: T.bgCozy, borderRadius: 12,
                border: i === 0 ? `3px solid ${T.accent}` : `1px solid ${T.borderSoft}`,
                transform: i === 0 ? 'scale(1.04)' : 'scale(1)',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                boxShadow: i === 0 ? '0 2px 8px rgba(245,158,75,0.25)' : 'none',
                flexShrink: 0,
              }}>
                {/* shape preview */}
                <PiecePreview shape={it.shape} color={it.c}/>
                {/* rotate */}
                <div style={{
                  position: 'absolute', top: 4, right: 4,
                  width: 24, height: 24, borderRadius: '50%',
                  background: T.bgElevated, border: `1px solid ${T.borderSoft}`,
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                }}>{Icon.rotate(T.accent)}</div>
              </div>
            ))}
          </div>
          <div style={{ display: 'flex', justifyContent: 'center', gap: 4, marginTop: 8 }}>
            {[0,1,2].map(i => (
              <div key={i} style={{ width: i === 0 ? 16 : 5, height: 5, borderRadius: 999, background: i === 0 ? T.accent : T.borderSoft }}/>
            ))}
          </div>
        </Card>
        <button onClick={() => nav.go('result')} style={{
          marginTop: 10, background: 'none', border: 'none', color: T.textSecondary,
          fontSize: 11, cursor: 'pointer', width: '100%',
        }}>(데모: 결과 화면 보기 →)</button>
      </div>
    </BoxyScreen>
  );
}

function PiecePreview({ shape, color }) {
  // very small block-piece SVG
  const cells = {
    L: [[0,0],[0,1],[0,2],[1,2]],
    I: [[0,0],[1,0],[2,0],[3,0]],
    T: [[0,0],[1,0],[2,0],[1,1]],
  }[shape] || [[0,0]];
  const sz = 14;
  const maxX = Math.max(...cells.map(c=>c[0])) + 1;
  const maxY = Math.max(...cells.map(c=>c[1])) + 1;
  return (
    <svg width={maxX*sz} height={maxY*sz}>
      {cells.map(([x,y],i) => (
        <rect key={i} x={x*sz} y={y*sz} width={sz-1} height={sz-1} rx={2}
              fill={color} stroke="rgba(0,0,0,0.12)"/>
      ))}
    </svg>
  );
}

// ─── Result modal ───
function ResultScreen({ nav, tw }) {
  const success = tw.resultState !== 'fail';
  return (
    <BoxyScreen bg="rgba(61,43,31,0.65)">
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%', padding: 20 }}>
        <div style={{
          width: '100%', background: T.bgElevated, borderRadius: 22,
          padding: '24px 22px 22px',
          boxShadow: '0 16px 40px rgba(0,0,0,0.3)',
        }}>
          <div style={{ display: 'flex', justifyContent: 'center', marginTop: -8, marginBottom: 8 }}>
            <BoxyMascot pose={success ? 'happy' : 'sad'} size={130}/>
          </div>
          <div style={{ textAlign: 'center', fontSize: 26, fontWeight: 800, letterSpacing: '-0.8px', color: T.textPrimary }}>
            {success ? '축하합니다!' : '아쉬워요'}
          </div>
          <div style={{ textAlign: 'center', fontSize: 14, color: T.textSecondary, marginTop: 6 }}>
            {success ? '1-7단계 클리어' : '다시 한 번 도전해볼까요?'}
          </div>
          {success && (
            <div style={{ display: 'flex', justifyContent: 'center', gap: 8, margin: '18px 0 4px' }}>
              {Icon.star(true, 44)}{Icon.star(true, 44)}{Icon.star(false, 44)}
            </div>
          )}
          {/* Stats */}
          <div style={{ marginTop: 16, padding: '10px 4px', borderTop: `1px solid ${T.borderSoft}`, borderBottom: `1px solid ${T.borderSoft}` }}>
            {[
              { label: '소요 시간', val: '1:24' },
              { label: '사용한 힌트', val: '0' },
              { label: '되돌리기', val: '3' },
            ].map((r, i) => (
              <div key={i} style={{ display: 'flex', justifyContent: 'space-between', padding: '8px 6px', fontSize: 14 }}>
                <span style={{ color: T.textSecondary }}>{r.label}</span>
                <span style={{ fontWeight: 700, color: T.textPrimary }}>{r.val}</span>
              </div>
            ))}
          </div>
          <div style={{ marginTop: 16 }}>
            <PrimaryBtn onClick={() => nav.go('gameplay')}>
              {success ? '다음 레벨' : '다시 시도'}
            </PrimaryBtn>
            <div style={{ height: 8 }}/>
            <GhostBtn onClick={() => nav.go('main')}>메뉴로</GhostBtn>
          </div>
        </div>
      </div>
    </BoxyScreen>
  );
}

// ─── Ad reward modal ───
function AdScreen({ nav }) {
  return (
    <BoxyScreen bg="rgba(61,43,31,0.65)">
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%', padding: 24 }}>
        <div style={{
          width: '100%', background: T.bgElevated, borderRadius: 22,
          padding: '24px 22px',
        }}>
          <div style={{ display: 'flex', justifyContent: 'center' }}>
            <BoxyMascot pose="thinking" size={130}/>
          </div>
          <div style={{ textAlign: 'center', fontSize: 22, fontWeight: 800, letterSpacing: '-0.5px', marginTop: 8 }}>
            힌트가 필요한가요?
          </div>
          <div style={{ textAlign: 'center', fontSize: 14, color: T.textSecondary, marginTop: 6 }}>
            광고를 시청하고 힌트를 받아요
          </div>
          <div style={{
            margin: '18px auto', padding: '10px 16px', display: 'inline-flex', alignItems: 'center', gap: 8,
            background: T.accentSoft, borderRadius: 999, fontSize: 14, fontWeight: 700, color: T.accent,
            display: 'flex', justifyContent: 'center', width: 'fit-content', marginLeft: 'auto', marginRight: 'auto',
          }}>
            <Icon.bulb/> +1 힌트
          </div>
          <div style={{ marginTop: 8 }}>
            <PrimaryBtn onClick={() => nav.back()}>광고 보기 (15초)</PrimaryBtn>
            <div style={{ height: 6 }}/>
            <GhostBtn onClick={() => nav.back()}>닫기</GhostBtn>
          </div>
        </div>
      </div>
    </BoxyScreen>
  );
}

window.GameplayScreen = GameplayScreen;
window.ResultScreen = ResultScreen;
window.AdScreen = AdScreen;
