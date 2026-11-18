// Elemental Trials: 물 스킬 시전 마법진. 발밑에 원과 물결 고리, 물방울 문양 셋이 펼쳐져 돌고, 거품이 떠오른 뒤 사라진다. 빛 번짐 없음.
// 위에서 내려다본 바닥이라 세로를 FLAT 비율로 눌러 그린다. 지름 약 44px(1.4칸).
import { ramp, fit, burst, seg, ease, TAU } from "../../core/index.js";

const R = 22;
const FLAT = 0.55;

export const meta = {
  name: "ET cast water",
  desc: "A water casting circle opens under the caster: blue rings with a rippling wave band, three turning droplet glyphs, bubbles rising off it, then it fades.",
  tags: ["water", "magic-circle", "cast", "elemental-trials"],
  size: [68, 52],
  origin: [34, 32],
  duration: 0.5,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#123a7a", "#1f6ac8", "#3ea4ea", "#8ad8f8", "#e8fbff"], seed: 8 },
};

const pt = (a, r) => [Math.cos(a) * r, Math.sin(a) * r * FLAT];

function loop(p, rf, c, a) {
  let prev = pt(0, rf(0));
  for (let i = 1; i <= 64; i++) {
    const ang = (i / 64) * TAU, cur = pt(ang, rf(ang));
    p.line(prev[0], prev[1], cur[0], cur[1], c, a);
    prev = cur;
  }
}

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5);
  const open = ease.outBack(seg(t, 0, 0.15));
  const fade = 1 - seg(t, 0.32, 0.45);
  if (fade <= 0) return;
  const r = R * open, rot = -t * 2;
  const bright = t > 0.12 && t < 0.24;

  loop(p, () => r, bright ? C[4] : C[3], fade);
  loop(p, () => r * 0.7, C[2], fade);
  // 물결 고리: 두 원 사이에서 출렁인다
  loop(p, (a) => r * 0.85 + Math.sin(a * 8 + t * 18) * 1.6 * open, C[3], fade);

  // 물방울 문양 셋: 둥근 몸과 뾰족한 끝, 바깥을 향한다
  for (let i = 0; i < 3; i++) {
    const a = rot + (i / 3) * TAU, [x, y] = pt(a, r * 0.42);
    const [tx, ty] = pt(a, r * 0.42 + 9);
    const nx = -Math.sin(a) * 3.2, ny = Math.cos(a) * 3.2 * FLAT;
    p.tri(x + nx, y + ny, x - nx, y - ny, tx, ty, C[2], fade);
    p.ellipse(x, y, 3.4, 3.4 * FLAT + 0.8, C[2], fade);
    p.px(x - 1, y - 1, C[4], fade);
    p.px(x - 1, y, C[3], fade);
  }
  p.ellipse(0, 0, 2.5, 2.5 * FLAT + 0.5, C[3], fade);

  // 떠오르는 거품
  burst(9, o.seed, (q) => {
    const a = q.range(0, TAU), born = q.range(0.1, 0.24), life = q.range(0.2, 0.28);
    const k = seg(t, born, born + life);
    if (t < born || k >= 1) return;
    const [x, y] = pt(a, r * q.range(0.5, 1));
    const by = y - ease.outQuad(k) * q.range(8, 14), bx = x + Math.sin(k * 9 + a) * 1.2;
    if (k < 0.75) p.ring(bx, by, q.range(1, 2), 1, C[3]);
    else p.px(bx, by, C[4]);
  });
}
