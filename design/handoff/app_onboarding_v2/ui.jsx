// Boxy Pack — shared UI primitives, screen components, icons.
// Loaded after mascot.jsx and before app.jsx.

const T = {
  bgCozy: '#FFF6EC',
  bgElevated: '#FFFFFF',
  accent: '#F59E4B',
  accentOn: '#FFFFFF',
  accentSoft: '#FFE4C8',
  accentDeep: '#D97A2B',
  mascotYellow: '#FFD60A',
  textPrimary: '#3D2B1F',
  textSecondary: '#8B6F5C',
  borderSoft: '#F0E0CC',
  success: '#7CB342',
  warning: '#F4A93D',
  error: '#E87764',
  shadowWarm: 'rgba(160,90,40,0.12)',
  shadowWarmStrong: 'rgba(160,90,40,0.22)',
};

// ─── icons (stroke-based, mobile-friendly weight) ───
const Icon = {
  back: (c = T.textPrimary) => (
    <svg width="28" height="28" viewBox="0 0 24 24" fill="none">
      <path d="M15 18l-6-6 6-6" stroke={c} strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  close: (c = T.textPrimary) => (
    <svg width="26" height="26" viewBox="0 0 24 24" fill="none">
      <path d="M6 6l12 12M18 6L6 18" stroke={c} strokeWidth="2.5" strokeLinecap="round"/>
    </svg>
  ),
  pause: (c = T.textPrimary) => (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
      <rect x="6" y="5" width="4" height="14" rx="1.4" fill={c}/>
      <rect x="14" y="5" width="4" height="14" rx="1.4" fill={c}/>
    </svg>
  ),
  bulb: (c = T.textPrimary) => (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
      <path d="M9 18h6M10 21h4M12 3a6 6 0 00-4 10.5c.6.6 1 1.4 1 2.2V17h6v-1.3c0-.8.4-1.6 1-2.2A6 6 0 0012 3z"
            stroke={c} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  undo: (c = T.textPrimary) => (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
      <path d="M9 14L4 9l5-5M4 9h10a6 6 0 016 6v0a6 6 0 01-6 6H8"
            stroke={c} strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  settings: (c = T.textPrimary) => (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
      <circle cx="12" cy="12" r="3" stroke={c} strokeWidth="2"/>
      <path d="M19 12a7 7 0 00-.1-1.2l2-1.5-2-3.4-2.3.9a7 7 0 00-2-1.2L14.2 3h-4l-.4 2.6a7 7 0 00-2 1.2l-2.3-.9-2 3.4 2 1.5A7 7 0 005 12c0 .4 0 .8.1 1.2l-2 1.5 2 3.4 2.3-.9a7 7 0 002 1.2l.4 2.6h4l.4-2.6a7 7 0 002-1.2l2.3.9 2-3.4-2-1.5c.1-.4.1-.8.1-1.2z"
            stroke={c} strokeWidth="2" strokeLinejoin="round"/>
    </svg>
  ),
  shop: (c = T.textPrimary) => (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
      <path d="M4 8h16l-1.4 11.2a2 2 0 01-2 1.8H7.4a2 2 0 01-2-1.8L4 8z" stroke={c} strokeWidth="2" strokeLinejoin="round"/>
      <path d="M9 8V6a3 3 0 016 0v2" stroke={c} strokeWidth="2" strokeLinecap="round"/>
    </svg>
  ),
  coin: () => (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
      <circle cx="12" cy="12" r="9" fill="#F4C842" stroke="#C99820" strokeWidth="1.8"/>
      <text x="12" y="16" textAnchor="middle" fontFamily="ui-rounded, system-ui" fontSize="12" fontWeight="800" fill="#7A5A10">¢</text>
    </svg>
  ),
  star: (filled = true, size = 32) => (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <path d="M12 3l2.7 5.6 6.1.9-4.4 4.3 1 6-5.4-2.8L6.6 20l1-6L3.2 9.5l6.1-.9L12 3z"
            fill={filled ? T.accent : 'transparent'} stroke={filled ? T.accentDeep : T.borderSoft} strokeWidth="1.8" strokeLinejoin="round"/>
    </svg>
  ),
  lock: (c = T.textSecondary) => (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none">
      <rect x="5" y="11" width="14" height="9" rx="2" stroke={c} strokeWidth="2"/>
      <path d="M8 11V8a4 4 0 018 0v3" stroke={c} strokeWidth="2" strokeLinecap="round"/>
    </svg>
  ),
  rotate: (c = T.accent) => (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
      <path d="M3 12a9 9 0 0115-6.7L21 8M21 3v5h-5" stroke={c} strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
  chev: (c = T.textSecondary) => (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none">
      <path d="M9 6l6 6-6 6" stroke={c} strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
};

// ─── Scaled card with warm 2-layer shadow ───
function Card({ children, style, padding = 16, radius = 16, shadowAlpha = 0.10 }) {
  return (
    <div style={{ position: 'relative', ...style }}>
      <div style={{
        position: 'absolute', inset: 0, borderRadius: radius,
        background: `rgba(160,90,40,${shadowAlpha})`,
        transform: 'translateY(3px)',
      }}/>
      <div style={{
        position: 'relative', background: T.bgElevated,
        borderRadius: radius, padding,
        border: `1px solid ${T.borderSoft}`,
      }}>
        {children}
      </div>
    </div>
  );
}

// ─── Buttons ───
function PrimaryBtn({ children, onClick, style, disabled }) {
  const [pressed, setPressed] = React.useState(false);
  return (
    <div style={{ position: 'relative', ...style }}>
      <div style={{
        position: 'absolute', inset: 0, borderRadius: 10,
        background: `rgba(217,122,43,${pressed ? 0.30 : 0.28})`,
        transform: `translateY(${pressed ? 1 : 4}px)`,
        transition: 'transform 100ms, background 100ms',
      }}/>
      <button
        onClick={onClick}
        disabled={disabled}
        onMouseDown={() => setPressed(true)}
        onMouseUp={() => setPressed(false)}
        onMouseLeave={() => setPressed(false)}
        onTouchStart={() => setPressed(true)}
        onTouchEnd={() => setPressed(false)}
        style={{
          position: 'relative',
          width: '100%', height: 56, border: 'none',
          background: T.accent, color: T.accentOn,
          fontSize: 17, fontWeight: 700,
          fontFamily: 'inherit',
          borderRadius: 10, cursor: disabled ? 'not-allowed' : 'pointer',
          opacity: disabled ? 0.5 : 1,
          transform: pressed ? 'scale(0.98)' : 'scale(1)',
          transition: 'transform 100ms',
          letterSpacing: '-0.2px',
        }}>
        {children}
      </button>
    </div>
  );
}

function SecondaryBtn({ children, onClick, style }) {
  return (
    <button onClick={onClick} style={{
      width: '100%', height: 48, border: `2px solid ${T.accent}`,
      background: 'transparent', color: T.accent,
      fontSize: 16, fontWeight: 700, fontFamily: 'inherit',
      borderRadius: 10, cursor: 'pointer', ...style,
    }}>{children}</button>
  );
}

function GhostBtn({ children, onClick, style }) {
  return (
    <button onClick={onClick} style={{
      width: '100%', height: 44, border: 'none',
      background: 'transparent', color: T.textSecondary,
      fontSize: 15, fontWeight: 500, fontFamily: 'inherit',
      borderRadius: 10, cursor: 'pointer', ...style,
    }}>{children}</button>
  );
}

function IconBtn({ children, onClick, size = 44, style }) {
  return (
    <div style={{ position: 'relative', width: size, height: size, ...style }}>
      <div style={{
        position: 'absolute', inset: 0, borderRadius: '50%',
        background: T.shadowWarm, transform: 'translateY(2px)',
      }}/>
      <button onClick={onClick} style={{
        position: 'relative',
        width: size, height: size, borderRadius: '50%',
        border: `1px solid ${T.borderSoft}`,
        background: T.bgElevated, cursor: 'pointer',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        padding: 0,
      }}>{children}</button>
    </div>
  );
}

function Pill({ children, style, soft = true, onClick }) {
  return (
    <div onClick={onClick} style={{
      display: 'inline-flex', alignItems: 'center', gap: 6,
      height: 30, padding: '0 12px', borderRadius: 999,
      background: soft ? T.accentSoft : T.bgElevated,
      color: T.accent, fontSize: 13, fontWeight: 700,
      border: soft ? 'none' : `1px solid ${T.borderSoft}`,
      cursor: onClick ? 'pointer' : 'default',
      ...style,
    }}>{children}</div>
  );
}

// ─── Boxy Frame: a simulated phone screen (390×800) inside the iOS device.
// The design spec uses 1080×1920 but we render proportionally at our viewport.
// All paddings scaled accordingly: spec/3 ≈ device px.
// ───
function BoxyScreen({ children, bg = T.bgCozy }) {
  return (
    <div style={{
      width: '100%', height: '100%',
      background: bg,
      position: 'relative', overflow: 'hidden',
      fontFamily: '"Pretendard", -apple-system, system-ui, sans-serif',
      color: T.textPrimary,
    }}>
      {children}
    </div>
  );
}

// ─── Topbar with safe area awareness (top inset already in iOS frame nav) ───
function Topbar({ left, center, right, sticky }) {
  return (
    <div style={{
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      padding: '14px 16px', minHeight: 56,
      borderBottom: sticky ? `1px solid ${T.borderSoft}` : 'none',
      background: sticky ? T.bgCozy : 'transparent',
      position: sticky ? 'sticky' : 'relative', top: 0, zIndex: 5,
    }}>
      <div style={{ minWidth: 44, display: 'flex' }}>{left}</div>
      <div style={{ flex: 1, textAlign: 'center', fontWeight: 700, fontSize: 17, letterSpacing: '-0.3px' }}>{center}</div>
      <div style={{ minWidth: 44, display: 'flex', justifyContent: 'flex-end' }}>{right}</div>
    </div>
  );
}

// ─── Mascot frame: rounded container with soft peach radial ───
function MascotFrame({ pose, size = 140 }) {
  return (
    <div style={{ position: 'relative', display: 'inline-block' }}>
      <div style={{
        position: 'absolute', inset: 0, borderRadius: 28,
        background: T.shadowWarm, transform: 'translateY(4px)',
      }}/>
      <div style={{
        position: 'relative',
        width: size + 28, height: size + 28,
        borderRadius: 28,
        background: `radial-gradient(circle at 50% 55%, ${T.accentSoft} 0%, ${T.bgElevated} 70%)`,
        border: `1px solid ${T.borderSoft}`,
        display: 'flex', alignItems: 'flex-end', justifyContent: 'center',
        padding: '0 14px 4px',
      }}>
        <BoxyMascot pose={pose} size={size} />
      </div>
    </div>
  );
}

// expose
Object.assign(window, {
  T, Icon, Card, PrimaryBtn, SecondaryBtn, GhostBtn, IconBtn, Pill,
  BoxyScreen, Topbar, MascotFrame,
});
