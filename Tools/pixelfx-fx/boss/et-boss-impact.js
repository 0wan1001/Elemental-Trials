// Elemental Trials: 보스 돌진이 끝나 멈출 때 (1회). 바닥에 납작한 충격파 고리가 퍼지고, 흙먼지가 양옆으로 일며 파편이 튄다. 원점이 보스 발밑.
import { ramp, fit, burst, fly, seg, ease, TAU } from "../../core/index.js";

const FLAT = 0.45;

export const meta = {
  name: "ET boss impact",
  desc: "The boss slams to a stop: a flat shock ring spreads over the floor, dust billows out to both sides and chips fly.",
  tags: ["boss", "impact", "elemental-trials"],
  size: [112, 64],
  origin: [56, 44],
  duration: 0.6,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#0e2a2c", "#1f6b66", "#3fb3a2", "#c8fff0"], dust: ["#2e2836", "#5a5060", "#8a7f86"], seed: 38 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 4), D = fit(ramp(o.dust), 3);
  const rk = seg(t, 0, 0.35);
  if (rk < 1) {
    const R = 8 + ease.outCubic(rk) * 44;
    let prev = null;
    for (let i = 0; i <= 48; i++) {
      const a = (i / 48) * TAU, pt = [Math.cos(a) * R, Math.sin(a) * R * FLAT];
      if (prev) { p.line(prev[0], prev[1], pt[0], pt[1], rk < 0.4 ? C[3] : C[2], 1 - rk); if (rk < 0.5) p.line(prev[0], prev[1] + 1, pt[0], pt[1] + 1, C[1], 1 - rk); }
      prev = pt;
    }
  }
  burst(14, o.seed, (r) => {
    const side = r.next() < 0.5 ? -1 : 1, born = r.range(0, 0.08), k = seg(t, born, born + r.range(0.35, 0.5));
    if (t < born || k >= 1) return;
    const x = side * (6 + ease.outCubic(k) * r.range(16, 40)), y = r.range(-2, 3) - k * r.range(4, 12);
    p.circle(x, y, 2 + k * 4, k < 0.4 ? D[2] : D[1], 0.85 * (1 - k));
  });
  burst(10, o.seed + 3, (r) => {
    const a = r.range(-Math.PI, 0), sp = r.range(60, 120), life = r.range(0.3, 0.45);
    if (t > life) return;
    const [x, y] = fly(0, 0, Math.cos(a) * sp, Math.sin(a) * sp, t, 300, 3);
    p.dot(x, y, D[2], 1);
  });
}
