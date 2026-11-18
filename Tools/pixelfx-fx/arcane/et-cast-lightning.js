// Elemental Trials: 번개 스킬 시전 마법진. 발밑에 원과 지그재그 고리, 팔망성이 펼쳐져 돌고, 테두리에서 스파크가 튄 뒤 사라진다. 빛 번짐 없음.
// 위에서 내려다본 바닥이라 세로를 FLAT 비율로 눌러 그린다. 지름 약 44px(1.4칸).
import { ramp, fit, rng, bolt, burst, seg, ease, TAU } from "../../core/index.js";

const R = 22;
const FLAT = 0.55;

export const meta = {
  name: "ET cast lightning",
  desc: "A lightning casting circle opens under the caster: a yellow ring with a zigzag band, a turning eight-point star of two squares, sparks arcing up off the rim, then it fades.",
  tags: ["lightning", "magic-circle", "cast", "elemental-trials"],
  size: [68, 56],
  origin: [34, 36],
  duration: 0.5,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#3a2a08", "#8a6a10", "#e0b020", "#ffe45a", "#fffbe0"], seed: 4 },
};

const ARC_PAL = ["#2550d0", "#7cc8ff", "#e8f8ff"];

const pt = (a, r) => [Math.cos(a) * r, Math.sin(a) * r * FLAT];

function loop(p, n, rf, c, a) {
  let prev = pt(0, rf(0, 0));
  for (let i = 1; i <= n; i++) {
    const cur = pt((i / n) * TAU, rf((i / n) * TAU, i));
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
  const C = fit(ramp(o.palette), 5), ARC = fit(ramp(ARC_PAL), 3);
  const open = ease.outBack(seg(t, 0, 0.15));
  const fade = 1 - seg(t, 0.32, 0.45);
  if (fade <= 0) return;
  const r = R * open, rot = t * 3;
  const bright = t > 0.12 && t < 0.24;

  loop(p, 48, () => r, bright ? C[4] : C[3], fade);
  // 지그재그 고리: 안팎으로 꺾인다 (프레임마다 엇갈려 지직거린다)
  const flip = Math.floor(t * 12) % 2;
  loop(p, 24, (a, i) => r * 0.86 + ((i + flip) % 2 ? 1.8 : -1.8) * open, C[2], fade);
  loop(p, 32, () => r * 0.3, C[2], fade);
  // 팔망성: 사각형 두 개를 45° 엇갈려 겹친다
  polygon(p, 4, r * 0.8, rot, bright ? C[4] : C[3], fade);
  polygon(p, 4, r * 0.8, rot + TAU / 8, C[3], fade);
  p.ellipse(0, 0, 2.5, 2.5 * FLAT + 0.5, C[4], fade);

  // 테두리에서 위로 튀는 스파크
  burst(7, o.seed, (q, i) => {
    const a = (i / 7) * TAU + q.range(-0.3, 0.3), born = q.range(0.06, 0.26);
    if (t < born || t > born + 0.09) return;
    const [x, y] = pt(a, r);
    const path = bolt(rng(o.seed, i + Math.floor(t * 12) * 13), x, y, x + q.range(-4, 4), y - q.range(8, 14), { jag: 0.35, depth: 2 });
    for (let j = 1; j < path.length; j++) {
      p.line(path[j - 1][0], path[j - 1][1], path[j][0], path[j][1], j === 1 ? ARC[2] : ARC[1]);
    }
  });
}
