// Boxy Pack — main App. Wires nav + tweaks + iOS frame + responsive scaling.

const SCREENS = ['splash','onboarding','main','levels','gameplay','result','ad','settings'];
const SCREEN_LABELS = {
  splash: 'S0 Splash', onboarding: 'S1 Onboarding', main: 'S2 Main Menu',
  levels: 'S3 Level Select', gameplay: 'S4 Gameplay', result: 'S5 Result',
  ad: 'S6 Ad Reward', settings: 'S7 Settings',
};

const TWEAK_DEFAULTS = /*EDITMODE-BEGIN*/{
  "screen": "main",
  "showFrame": true,
  "lang": "ko",
  "theme": "light",
  "resultState": "success"
}/*EDITMODE-END*/;

function App() {
  const [tw, setTweak] = useTweaks(TWEAK_DEFAULTS);
  const [history, setHistory] = React.useState(['main']);
  const screen = tw.screen;

  // sync screen changes to tweak
  const nav = React.useMemo(() => ({
    go: (s) => {
      setHistory(h => [...h, s]);
      setTweak('screen', s);
    },
    back: () => {
      setHistory(h => {
        if (h.length <= 1) return h;
        const next = h.slice(0, -1);
        setTweak('screen', next[next.length - 1]);
        return next;
      });
    },
  }), [setTweak]);

  // on first mount, sync history with current screen
  React.useEffect(() => {
    if (history[history.length - 1] !== screen) {
      setHistory(h => [...h, screen]);
    }
  }, [screen]);

  // Pick screen
  const ScreenComp = {
    splash: SplashScreen,
    onboarding: OnboardingScreen,
    main: MainMenuScreen,
    levels: LevelSelectScreen,
    gameplay: GameplayScreen,
    result: ResultScreen,
    ad: AdScreen,
    settings: SettingsScreen,
  }[screen] || MainMenuScreen;

  // Phone scale: design at 390x800, scale to fit viewport with padding
  const [scale, setScale] = React.useState(1);
  React.useEffect(() => {
    const calc = () => {
      const PAD_X = 40, PAD_Y = 80;
      const designW = tw.showFrame ? 402 : 390;
      const designH = tw.showFrame ? 874 : 800;
      const sx = (window.innerWidth - PAD_X) / designW;
      const sy = (window.innerHeight - PAD_Y) / designH;
      setScale(Math.min(1.05, Math.min(sx, sy)));
    };
    calc();
    window.addEventListener('resize', calc);
    return () => window.removeEventListener('resize', calc);
  }, [tw.showFrame]);

  const screenContent = (
    <div data-screen-label={SCREEN_LABELS[screen]} style={{ width: '100%', height: '100%' }}>
      <ScreenComp nav={nav} tw={tw} setTweak={setTweak}/>
    </div>
  );

  return (
    <div style={{
      width: '100vw', height: '100vh',
      background: '#E8DFD2',
      backgroundImage: 'radial-gradient(circle at 20% 20%, #F3E8D7 0%, #E8DFD2 60%)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      overflow: 'hidden',
      fontFamily: '"Pretendard", -apple-system, system-ui, sans-serif',
    }}>
      <div style={{ transform: `scale(${scale})`, transformOrigin: 'center' }}>
        {tw.showFrame ? (
          <IOSDevice width={402} height={874}>
            {screenContent}
          </IOSDevice>
        ) : (
          <div style={{
            width: 390, height: 800, borderRadius: 32, overflow: 'hidden',
            background: T.bgCozy,
            boxShadow: '0 30px 80px rgba(0,0,0,0.18), 0 0 0 1px rgba(0,0,0,0.08)',
          }}>{screenContent}</div>
        )}
      </div>

      <BoxyTweaks tw={tw} setTweak={setTweak}/>
    </div>
  );
}

function BoxyTweaks({ tw, setTweak }) {
  return (
    <TweaksPanel title="Tweaks">
      <TweakSection title="Screen">
        <TweakSelect
          label="Active screen"
          value={tw.screen}
          options={SCREENS.map(s => ({ value: s, label: SCREEN_LABELS[s] }))}
          onChange={v => setTweak('screen', v)}
        />
      </TweakSection>
      <TweakSection title="Frame">
        <TweakToggle label="iPhone bezel" value={tw.showFrame} onChange={v => setTweak('showFrame', v)}/>
      </TweakSection>
      <TweakSection title="Locale">
        <TweakRadio
          label="Language"
          value={tw.lang}
          options={[{value:'ko',label:'한국어'},{value:'en',label:'English'}]}
          onChange={v => setTweak('lang', v)}
        />
      </TweakSection>
      <TweakSection title="Result modal">
        <TweakRadio
          label="State"
          value={tw.resultState}
          options={[{value:'success',label:'성공'},{value:'fail',label:'실패'}]}
          onChange={v => setTweak('resultState', v)}
        />
      </TweakSection>
    </TweaksPanel>
  );
}

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(<App/>);
