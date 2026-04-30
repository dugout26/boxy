// Boxy mascot — 5 poses, pure SVG. Stylized cube with face + stubby limbs + ground shadow.
// Sizes are intrinsic 240×260 viewBox; pass `size` to scale.

function BoxyMascot({ pose = 'default', size = 240 }) {
  const yellow = '#FFD60A';
  const yellowDark = '#E5B800';
  const yellowLight = '#FFE85C';
  const stroke = '#3D2B1F';
  const cheek = '#F59E4B';

  // Eye + mouth shape per pose
  const renderFace = () => {
    switch (pose) {
      case 'happy':
        return (
          <g>
            {/* squinty happy eyes */}
            <path d="M82,118 Q92,108 102,118" stroke={stroke} strokeWidth="5" fill="none" strokeLinecap="round" />
            <path d="M138,118 Q148,108 158,118" stroke={stroke} strokeWidth="5" fill="none" strokeLinecap="round" />
            {/* big smile */}
            <path d="M100,140 Q120,160 140,140" stroke={stroke} strokeWidth="5" fill="none" strokeLinecap="round" />
            <ellipse cx="78" cy="135" rx="9" ry="6" fill={cheek} opacity="0.5" />
            <ellipse cx="162" cy="135" rx="9" ry="6" fill={cheek} opacity="0.5" />
          </g>
        );
      case 'sad':
        return (
          <g>
            {/* down-curving eyes */}
            <path d="M82,118 Q92,128 102,118" stroke={stroke} strokeWidth="5" fill="none" strokeLinecap="round" />
            <path d="M138,118 Q148,128 158,118" stroke={stroke} strokeWidth="5" fill="none" strokeLinecap="round" />
            {/* frown */}
            <path d="M100,150 Q120,138 140,150" stroke={stroke} strokeWidth="5" fill="none" strokeLinecap="round" />
            {/* tear */}
            <path d="M88,128 Q88,142 92,148 Q92,140 88,128 Z" fill="#7CC5E8" />
          </g>
        );
      case 'thinking':
        return (
          <g>
            <circle cx="92" cy="118" r="5" fill={stroke} />
            <ellipse cx="148" cy="118" rx="6" ry="4" fill={stroke} />
            {/* small mouth, slightly off */}
            <path d="M108,148 Q120,144 132,148" stroke={stroke} strokeWidth="4.5" fill="none" strokeLinecap="round" />
            {/* raised eyebrow */}
            <path d="M138,100 L160,96" stroke={stroke} strokeWidth="4" strokeLinecap="round" />
          </g>
        );
      case 'sleeping':
        return (
          <g>
            {/* closed eyes — gentle arcs */}
            <path d="M82,120 Q92,114 102,120" stroke={stroke} strokeWidth="4.5" fill="none" strokeLinecap="round" />
            <path d="M138,120 Q148,114 158,120" stroke={stroke} strokeWidth="4.5" fill="none" strokeLinecap="round" />
            {/* tiny mouth */}
            <ellipse cx="120" cy="146" rx="6" ry="4" fill={stroke} />
            {/* zzz */}
            <text x="180" y="80" fontFamily="ui-rounded, system-ui" fontSize="22" fontWeight="700" fill={stroke}>z</text>
            <text x="195" y="60" fontFamily="ui-rounded, system-ui" fontSize="16" fontWeight="700" fill={stroke} opacity="0.7">z</text>
          </g>
        );
      default:
        return (
          <g>
            <circle cx="92" cy="118" r="6" fill={stroke} />
            <circle cx="148" cy="118" r="6" fill={stroke} />
            <path d="M105,142 Q120,154 135,142" stroke={stroke} strokeWidth="5" fill="none" strokeLinecap="round" />
            <ellipse cx="78" cy="135" rx="8" ry="5" fill={cheek} opacity="0.4" />
            <ellipse cx="162" cy="135" rx="8" ry="5" fill={cheek} opacity="0.4" />
          </g>
        );
    }
  };

  // arms differ a bit per pose
  const arms = () => {
    if (pose === 'happy') {
      return (
        <g>
          {/* arms up */}
          <path d="M50,90 Q35,55 30,30" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
          <path d="M190,90 Q205,55 210,30" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
          <circle cx="30" cy="30" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
          <circle cx="210" cy="30" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
        </g>
      );
    }
    if (pose === 'sad') {
      return (
        <g>
          <path d="M55,140 Q40,170 36,180" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
          <path d="M185,140 Q200,170 204,180" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
          <circle cx="36" cy="182" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
          <circle cx="204" cy="182" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
        </g>
      );
    }
    if (pose === 'thinking') {
      return (
        <g>
          {/* left arm down */}
          <path d="M55,140 Q42,160 38,178" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
          <circle cx="38" cy="180" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
          {/* right arm to chin */}
          <path d="M185,140 Q175,120 165,108" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
          <circle cx="162" cy="106" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
        </g>
      );
    }
    if (pose === 'sleeping') {
      return (
        <g>
          <path d="M55,135 Q40,150 30,160" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
          <path d="M185,135 Q200,150 210,160" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
          <circle cx="28" cy="160" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
          <circle cx="212" cy="160" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
        </g>
      );
    }
    // default
    return (
      <g>
        <path d="M55,140 Q42,160 38,178" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
        <path d="M185,140 Q198,160 202,178" stroke={yellowDark} strokeWidth="14" strokeLinecap="round" fill="none" />
        <circle cx="38" cy="180" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
        <circle cx="202" cy="180" r="11" fill={yellow} stroke={yellowDark} strokeWidth="2" />
      </g>
    );
  };

  // Body tilt for sleeping
  const bodyTransform = pose === 'sleeping' ? 'rotate(-8 120 130)' : '';

  return (
    <svg viewBox="0 0 240 260" width={size} height={size * (260 / 240)} style={{ display: 'block' }}>
      {/* ground shadow */}
      <ellipse cx="120" cy="232" rx="68" ry="10" fill="#3D2B1F" opacity="0.14" />

      <g transform={bodyTransform}>
        {/* arms behind body */}
        {arms()}

        {/* body cube */}
        <rect x="42" y="60" width="156" height="160" rx="28" ry="28"
              fill={yellow} stroke={yellowDark} strokeWidth="3" />
        {/* top highlight */}
        <path d="M52,80 Q52,68 64,68 L176,68 Q188,68 188,80"
              stroke={yellowLight} strokeWidth="6" fill="none" strokeLinecap="round" opacity="0.85" />
        {/* side soft shadow */}
        <path d="M186,82 L186,210 Q186,218 178,218" stroke={yellowDark} strokeWidth="4" fill="none" opacity="0.35" strokeLinecap="round" />

        {/* face */}
        {renderFace()}

        {/* feet */}
        <ellipse cx="84" cy="226" rx="20" ry="9" fill={yellowDark} />
        <ellipse cx="156" cy="226" rx="20" ry="9" fill={yellowDark} />
        <ellipse cx="84" cy="223" rx="20" ry="9" fill={yellow} />
        <ellipse cx="156" cy="223" rx="20" ry="9" fill={yellow} />
      </g>
    </svg>
  );
}

window.BoxyMascot = BoxyMascot;
