// Elemental Trials: 어둠 탄이 맞았을 때의 작은 터짐. 보라 섬광 1프레임, 어두운 고리, 조각 몇 개. 빛 번짐 없음.
import { ramp, fit, burst, fly, seg, ease, TAU } from "../../core/index.js";

export const meta = {
  name: "ET dark pop",
  desc: "A small flat dark hit: a violet flash frame, a dark ring opening and thinning, a few violet shards flicked out.",
  tags: ["impacts", "shadow", "elemental-trials"],
  size: [24, 24],
  origin: [12, 12],
  duration: 0.34,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#0f0a18", "#2c1a4a", "#5e3290", "#9a48ec", "#d8b0ff"], seed: 8 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5);
  // 첫 프레임: 작은 보라 섬광
  if (t < 0.06) {
    p.circle(0, 0, 3.5, C[3]);
    p.circle(0, 0, 1.8, C[4]);
    for (let i = 0; i < 4; i++) {
      const a = (i / 4) * TAU + 0.8;
      p.line(Math.cos(a) * 3, Math.sin(a) * 3, Math.cos(a) * 6, Math.sin(a) * 6, C[3]);
    }
  }
  // 어두운 고리: 벌어지며 가늘어진다
  const k = seg(t, 0.04, 0.3);
  if (k > 0 && k < 1) {
    const r = 2 + ease.outQuad(k) * 6;
    p.ring(0, 0, r, k < 0.5 ? 2 : 1, k < 0.6 ? C[2] : C[1]);
    if (k < 0.4) p.circle(0, 0, r - 2, C[0]);
  }
  // 튀는 조각
  burst(5, o.seed, (r) => {
    const a = r.range(0, TAU), sp = r.range(40, 70), life = r.range(0.18, 0.28);
    if (t < 0.04 || t > life) return;
    const [x, y] = fly(0, 0, Math.cos(a) * sp, Math.sin(a) * sp, t, 0, 6);
    p.px(x, y, t / life < 0.5 ? C[4] : C[2]);
  });
}
