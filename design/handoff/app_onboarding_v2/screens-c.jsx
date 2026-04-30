// Boxy Pack — Settings screen
const { T, Icon, IconBtn, Pill, BoxyScreen, Topbar } = window;

function Toggle({ on, onChange }) {
  return (
    <div onClick={() => onChange(!on)} style={{
      width: 50, height: 30, borderRadius: 999,
      background: on ? T.accent : T.borderSoft,
      position: 'relative', cursor: 'pointer',
      transition: 'background 200ms',
    }}>
      <div style={{
        position: 'absolute', top: 3, left: on ? 23 : 3,
        width: 24, height: 24, borderRadius: '50%',
        background: '#fff', boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
        transition: 'left 200ms',
      }}/>
    </div>
  );
}

function Row({ label, right, danger, onClick }) {
  return (
    <div onClick={onClick} style={{
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      padding: '14px 16px', minHeight: 52,
      borderBottom: `1px solid ${T.borderSoft}`,
      cursor: onClick ? 'pointer' : 'default',
      fontSize: 15,
      color: danger ? T.error : T.textPrimary,
      fontWeight: danger ? 600 : 400,
    }}>
      <span>{label}</span>
      <span style={{ display: 'flex', alignItems: 'center', gap: 6 }}>{right}</span>
    </div>
  );
}

function SectionHeader({ children }) {
  return (
    <div style={{
      padding: '20px 16px 8px', fontSize: 12, fontWeight: 700,
      color: T.textSecondary, textTransform: 'uppercase', letterSpacing: '0.5px',
    }}>{children}</div>
  );
}

function SettingsScreen({ nav, tw, setTweak }) {
  const [sfx, setSfx] = React.useState(true);
  const [bgm, setBgm] = React.useState(true);
  const [haptic, setHaptic] = React.useState(true);
  const [voice, setVoice] = React.useState(false);

  return (
    <BoxyScreen>
      <Topbar
        left={<IconBtn size={40} onClick={() => nav.go('main')}>{Icon.back()}</IconBtn>}
        center="설정"
        right={<div style={{ width: 40 }}/>}
        sticky
      />
      <div style={{ overflowY: 'auto', height: 'calc(100% - 70px)', paddingBottom: 32 }}>
        <SectionHeader>사운드</SectionHeader>
        <div style={{ background: T.bgElevated, marginInline: 12, borderRadius: 14, overflow: 'hidden', border: `1px solid ${T.borderSoft}` }}>
          <Row label="효과음" right={<Toggle on={sfx} onChange={setSfx}/>}/>
          <Row label="배경음악" right={<Toggle on={bgm} onChange={setBgm}/>}/>
          <Row label="햅틱(진동)" right={<Toggle on={haptic} onChange={setHaptic}/>}/>
          <Row label="마스코트 보이스" right={<Toggle on={voice} onChange={setVoice}/>}/>
        </div>

        <SectionHeader>게임</SectionHeader>
        <div style={{ background: T.bgElevated, marginInline: 12, borderRadius: 14, overflow: 'hidden', border: `1px solid ${T.borderSoft}` }}>
          <Row label="언어"
               onClick={() => setTweak('lang', tw.lang === 'ko' ? 'en' : 'ko')}
               right={<><Pill>{tw.lang === 'ko' ? '한국어' : 'English'}</Pill>{Icon.chev()}</>}/>
          <Row label="테마"
               onClick={() => setTweak('theme', tw.theme === 'light' ? 'dark' : 'light')}
               right={<><Pill>{tw.theme === 'dark' ? '다크' : '라이트'}</Pill>{Icon.chev()}</>}/>
          <Row label="튜토리얼 다시 보기" onClick={() => nav.go('onboarding')} right={Icon.chev()}/>
        </div>

        <SectionHeader>정보</SectionHeader>
        <div style={{ background: T.bgElevated, marginInline: 12, borderRadius: 14, overflow: 'hidden', border: `1px solid ${T.borderSoft}` }}>
          <Row label="광고 제거" right={<><Pill>₩3,900</Pill>{Icon.chev()}</>} onClick={() => {}}/>
          <Row label="구매 복원" right={Icon.chev()} onClick={() => {}}/>
          <Row label="이용약관" right={Icon.chev()} onClick={() => {}}/>
          <Row label="개인정보처리방침" right={Icon.chev()} onClick={() => {}}/>
          <Row label="버전" right={<span style={{ color: T.textSecondary, fontSize: 13 }}>1.0.0 (8)</span>}/>
        </div>

        <SectionHeader>위험 액션</SectionHeader>
        <div style={{ background: T.bgElevated, marginInline: 12, borderRadius: 14, overflow: 'hidden', border: `1px solid ${T.borderSoft}` }}>
          <Row label="진행도 초기화" danger onClick={() => {}} right={Icon.chev(T.error)}/>
        </div>
      </div>
    </BoxyScreen>
  );
}

window.SettingsScreen = SettingsScreen;
