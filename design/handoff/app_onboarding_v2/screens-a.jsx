// Boxy Pack — screen components. Each screen receives `nav` ({go, back}) and `tw` (tweaks state).

const { T, Icon, Card, PrimaryBtn, SecondaryBtn, GhostBtn, IconBtn, Pill, BoxyScreen, Topbar, MascotFrame } = window;

// ─────────────────────────────────────────────────────
// S0 — Splash
// ─────────────────────────────────────────────────────
function SplashScreen({ nav }) {
  const [progress, setProgress] = React.useState(0);
  React.useEffect(() => {
    const id = setInterval(() => setProgress(p => Math.min(100, p + 4)), 80);
    return () => clearInterval(id);
  }, []);
  return (
    <BoxyScreen>
      <div style={{ height: '100%', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'space-between', padding: '60px 32px 80px' }}>
        <div style={{ flex: 1 }} />
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 16 }}>
          <MascotFrame pose="default" size={150} />
          <div style={{ fontSize: 44, fontWeight: 800, letterSpacing: '-1.5px', color: T.textPrimary, marginTop: 8 }}>Boxy</div>
          <div style={{ fontSize: 14, color: T.textSecondary }}>Mound Studio</div>
        </div>
        <div style={{ flex: 1 }} />
        <div style={{ width: '60%', height: 6, background: T.borderSoft, borderRadius: 999, overflow: 'hidden' }}>
          <div style={{ width: `${progress}%`, height: '100%', background: T.accent, transition: 'width 80ms linear' }}/>
        </div>
        <button onClick={() => nav.go('onboarding')} style={{
          marginTop: 18, background: 'none', border: 'none', color: T.textSecondary, fontSize: 12, cursor: 'pointer',
        }}>tap to continue →</button>
      </div>
    </BoxyScreen>
  );
}

// ─────────────────────────────────────────────────────
// S1 — Onboarding
// ─────────────────────────────────────────────────────
const ONBOARD_STEPS = [
  { title: '한 칸씩, 꼭 맞게', body: '다양한 모양의 짐을\n빈틈없이 정리해보세요', illust: 'bag' },
  { title: '회전하고 맞추고', body: '아이템을 돌려가며\n완벽한 자리를 찾아요', illust: 'box' },
  { title: '50개의 코지한 퍼즐', body: '가방, 박스, 트렁크\n세 가지 테마를 즐겨요', illust: 'trunk' },
];

function OnboardIllust({ kind }) {
  // Stylized container + items as placeholder hero
  const w = 220, h = 160;
  if (kind === 'bag') {
    return (
      <svg width={w} height={h} viewBox="0 0 220 160">
        <ellipse cx="110" cy="148" rx="80" ry="6" fill={T.shadowWarm}/>
        <path d="M40 60 Q40 20 80 20 L140 20 Q180 20 180 60 L180 140 Q180 148 172 148 L48 148 Q40 148 40 140 Z" fill="#C28E5A" stroke="#8B5E2C" strokeWidth="2"/>
        <path d="M70 30 Q70 10 90 10 L130 10 Q150 10 150 30" stroke="#8B5E2C" strokeWidth="4" fill="none"/>
        <rect x="55" y="75" width="50" height="40" rx="6" fill={T.accent}/>
        <rect x="115" y="80" width="40" height="40" rx="6" fill="#7CB342"/>
        <circle cx="135" cy="55" r="18" fill="#E87764"/>
      </svg>
    );
  }
  if (kind === 'box') {
    return (
      <svg width={w} height={h} viewBox="0 0 220 160">
        <ellipse cx="110" cy="148" rx="80" ry="6" fill={T.shadowWarm}/>
        <path d="M40 50 L180 50 L180 140 L40 140 Z" fill="#D4A373" stroke="#8B5E2C" strokeWidth="2"/>
        <path d="M40 50 L60 30 L200 30 L180 50 Z" fill="#E8C8A0" stroke="#8B5E2C" strokeWidth="2"/>
        <path d="M180 50 L200 30 L200 120 L180 140 Z" fill="#B08555" stroke="#8B5E2C" strokeWidth="2"/>
        <rect x="55" y="70" width="42" height="42" rx="5" fill={T.accent}/>
        <rect x="105" y="80" width="58" height="35" rx="5" fill="#7CB342"/>
      </svg>
    );
  }
  return (
    <svg width={w} height={h} viewBox="0 0 220 160">
      <ellipse cx="110" cy="148" rx="80" ry="6" fill={T.shadowWarm}/>
      <rect x="35" y="40" width="150" height="100" rx="8" fill="#5B8A8A" stroke="#3D5C5C" strokeWidth="2"/>
      <rect x="35" y="68" width="150" height="6" fill="#3D5C5C"/>
      <rect x="100" y="20" width="20" height="20" rx="4" fill="#3D5C5C"/>
      <circle cx="60" cy="100" r="8" fill={T.accent}/>
      <circle cx="160" cy="100" r="8" fill={T.accent}/>
    </svg>
  );
}

function OnboardingScreen({ nav }) {
  const [step, setStep] = React.useState(0);
  const s = ONBOARD_STEPS[step];
  return (
    <BoxyScreen>
      <div style={{ display: 'flex', flexDirection: 'column', height: '100%', padding: '16px 24px 32px' }}>
        <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
          <button onClick={() => nav.go('main')} style={{
            background: 'none', border: 'none', color: T.textSecondary, fontSize: 14, fontWeight: 500, cursor: 'pointer', padding: 8,
          }}>건너뛰기</button>
        </div>

        <div style={{ display: 'flex', justifyContent: 'center', gap: 8, marginTop: 8 }}>
          {ONBOARD_STEPS.map((_, i) => (
            <div key={i} style={{
              width: i === step ? 24 : 8, height: 8, borderRadius: 999,
              background: i === step ? T.accent : T.borderSoft,
              transition: 'all 250ms',
            }}/>
          ))}
        </div>

        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 24 }}>
          <OnboardIllust kind={s.illust} />
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: 26, fontWeight: 800, letterSpacing: '-0.8px', marginBottom: 12 }}>{s.title}</div>
            <div style={{ fontSize: 15, color: T.textSecondary, lineHeight: 1.55, whiteSpace: 'pre-line' }}>{s.body}</div>
          </div>
        </div>

        <PrimaryBtn onClick={() => {
          if (step < ONBOARD_STEPS.length - 1) setStep(step + 1);
          else nav.go('main');
        }}>
          {step === ONBOARD_STEPS.length - 1 ? '시작하기' : '다음'}
        </PrimaryBtn>
      </div>
    </BoxyScreen>
  );
}

// ─────────────────────────────────────────────────────
// S2 — Main Menu
// ─────────────────────────────────────────────────────
function MainMenuScreen({ nav, tw }) {
  return (
    <BoxyScreen>
      <div style={{ display: 'flex', flexDirection: 'column', height: '100%', padding: '14px 20px 28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Pill><Icon.coin/> <span style={{ marginLeft: 2 }}>1,240</span></Pill>
          <Pill soft={false}>{tw.lang === 'ko' ? 'KO' : 'EN'} ▾</Pill>
        </div>

        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 20 }}>
          <MascotFrame pose="default" size={160} />
          <div style={{ textAlign: 'center', marginTop: 4 }}>
            <div style={{ fontSize: 48, fontWeight: 800, letterSpacing: '-1.8px', lineHeight: 1, color: T.textPrimary }}>Boxy</div>
            <div style={{ fontSize: 15, color: T.textSecondary, marginTop: 8 }}>짐 정리의 즐거움</div>
          </div>
        </div>

        <PrimaryBtn onClick={() => nav.go('levels')}>시작하기</PrimaryBtn>
        <div style={{ height: 8 }}/>
        <GhostBtn onClick={() => nav.go('gameplay')}>이어하기 · 1-7</GhostBtn>

        <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 16 }}>
          <IconBtn onClick={() => nav.go('settings')}>{Icon.settings()}</IconBtn>
          <IconBtn onClick={() => nav.go('ad')}>{Icon.shop()}</IconBtn>
        </div>
      </div>
    </BoxyScreen>
  );
}

// ─────────────────────────────────────────────────────
// S3 — Level Select
// ─────────────────────────────────────────────────────
function LevelSelectScreen({ nav, tw }) {
  const [theme, setTheme] = React.useState('bag');
  const total = 18;
  const completed = 6; // first 6 done (3,2,3,3,2,1 stars)
  const stars = [3,2,3,3,2,1];
  const current = 7;

  const themes = [
    { id: 'bag', label: '가방' },
    { id: 'box', label: '박스' },
    { id: 'trunk', label: '트렁크' },
  ];

  return (
    <BoxyScreen>
      <Topbar
        left={<IconBtn size={40} onClick={() => nav.go('main')}>{Icon.back()}</IconBtn>}
        center="레벨 선택"
        right={<Pill><Icon.coin/> <span style={{ marginLeft: 2 }}>1,240</span></Pill>}
        sticky
      />

      {/* Theme tabs */}
      <div style={{ display: 'flex', padding: '4px 16px 0', borderBottom: `1px solid ${T.borderSoft}` }}>
        {themes.map(t => {
          const active = theme === t.id;
          return (
            <button key={t.id} onClick={() => setTheme(t.id)} style={{
              flex: 1, padding: '14px 0', background: 'none', border: 'none',
              fontSize: 15, fontWeight: 700, cursor: 'pointer',
              color: active ? T.accent : T.textSecondary,
              borderBottom: active ? `3px solid ${T.accent}` : '3px solid transparent',
              marginBottom: -1, fontFamily: 'inherit',
            }}>{t.label}</button>
          );
        })}
      </div>

      <div style={{ padding: '20px 16px 32px', overflowY: 'auto', height: 'calc(100% - 110px)' }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 12 }}>
          {Array.from({ length: total }, (_, i) => {
            const n = i + 1;
            const isDone = n <= completed;
            const isCurrent = n === current;
            const isLocked = n > current;
            const star = isDone ? stars[i] : 0;
            return (
              <button key={n} onClick={() => !isLocked && nav.go('gameplay')} disabled={isLocked} style={{
                position: 'relative',
                aspectRatio: '1 / 1.15',
                background: T.bgElevated,
                border: isCurrent ? `3px solid ${T.accent}` : `1px solid ${T.borderSoft}`,
                borderRadius: 14,
                display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
                gap: 6, cursor: isLocked ? 'not-allowed' : 'pointer',
                opacity: isLocked ? 0.55 : 1,
                fontFamily: 'inherit',
                boxShadow: '0 2px 0 rgba(160,90,40,0.08)',
              }}>
                {isLocked ? (
                  Icon.lock()
                ) : (
                  <>
                    <div style={{ fontSize: 22, fontWeight: 800, color: T.textPrimary, letterSpacing: '-0.5px' }}>1-{n}</div>
                    {isDone ? (
                      <div style={{ display: 'flex', gap: 1 }}>
                        {[1,2,3].map(k => Icon.star(k <= star, 14))}
                      </div>
                    ) : isCurrent ? (
                      <div style={{ fontSize: 11, color: T.accent, fontWeight: 700 }}>도전</div>
                    ) : null}
                  </>
                )}
              </button>
            );
          })}
        </div>
      </div>
    </BoxyScreen>
  );
}

window.SplashScreen = SplashScreen;
window.OnboardingScreen = OnboardingScreen;
window.MainMenuScreen = MainMenuScreen;
window.LevelSelectScreen = LevelSelectScreen;
