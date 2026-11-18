// Elemental Trials: 불 스킬 시전 마법진. 발밑에 두 겹 원과 육망성이 펼쳐져 돌고, 테두리에서 불꽃이 솟은 뒤 사라진다. 빛 번짐 없음.
// 위에서 내려다본 바닥이라 세로를 FLAT 비율로 눌러 그린다. 지름 약 44px(1.4칸).
import { ramp, fit, burst, seg, ease, TAU } from "../../core/index.js";

const R = 22;
const FLAT = 0.55;

export const meta = {
  name: "ET cast fire",
  desc: "A fire casting circle opens under the caster: double red-orange rings and a turning hexagram, flame tongues licking up from the rim, then it fades.",
  tags: ["fire", "magic-circle", "cast", "elemental-trials"],
  size: [68, 52],
  origin: [34, 32],
  duration: 0.5,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#5a1410", "#b02a18", "#e8561c", "#ffa030", "#ffe68a"], seed: 6 },
};

const pt = (a, r) => [Math.cos(a) * r, Math.sin(a) * r * FLAT];

function ring(p, r, c, a) {
  let prev = pt(0, r);
  for (let i = 1; i <= 48; i++) {
    const cur = pt((i / 48) * TAU, r);
    p.line(prev[0], prev[1], cur[0], cur[1], c, a);
    prev = cur;
  }
}

function polygon(p, n, r, rot, c, a) {
  for (let i = 0; i < n; i++) {
    const [x0, y0] = pt(rot + (i / n) * TAU, r), [x1, y1] = pt(rot + ((i + 1) / n) * TAU, r);
    p.line(x0, y0, x1, y1, c, a);
  }
}

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5);
  const open = ease.outBack(seg(t, 0, 0.15));
  const fade = 1 - seg(t, 0.32, 0.45);
  if (fade <= 0) return;
  const r = R * open, rot = t * 2.4;
  const bright = t > 0.12 && t < 0.24;

  ring(p, r, bright ? C[4] : C[3], fade);
  ring(p, r - 1, C[2], fade);
  ring(p, r * 0.8, C[2], fade);
  // 육망성: 삼각형 두 개를 엇갈려 겹친다
  polygon(p, 3, r * 0.78, rot, bright ? C[4] : C[3], fade);
  polygon(p, 3, r * 0.78, rot + TAU / 6, C[3], fade);
  // 꼭짓점의 작은 점
  for (let i = 0; i < 6; i++) {
    const [x, y] = pt(rot + (i / 6) * TAU, r * 0.9);
    p.px(x, y, C[4], fade);
  }
  p.ellipse(0, 0, 3, 3 * FLAT + 0.5, C[3], fade);

  // 테두리에서 솟는 불꽃
  burst(10, o.seed, (q, i) => {
    const a = (i / 10) * TAU + q.range(-0.2, 0.2), born = q.range(0.08, 0.2), life = q.range(0.18, 0.28);
    const k = seg(t, born, born + life);
    if (t < born || k >= 1) return;
    const [x, y] = pt(a, r);
    const h = Math.sin(k * Math.PI) * q.range(6, 11), w = 2 * (1 - k * 0.5);
    p.tri(x - w, y, x + w, y, x + q.range(-1, 1), y - h, k < 0.5 ? C[3] : C[2]);
    p.tri(x - w * 0.5, y, x + w * 0.5, y, x, y - h * 0.55, C[4]);
  });
}
