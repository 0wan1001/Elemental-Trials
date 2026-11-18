// Elemental Trials: 보스 탄이 무언가에 맞아 부서질 때 (1회). 청록 보석 조각이 깨져 튀고, 짧은 빛 고리와 보라빛 돌가루가 흩어진다.
// 원점이 맞은 지점.
import { ramp, fit, burst, fly, seg, ease, TAU } from "../../core/index.js";

export const meta = {
  name: "ET boss shatter",
  desc: "Ancient guardian bullet shatters: teal gem shards burst out, a short glow ring flashes and purple stone dust scatters.",
  tags: ["impacts", "boss", "elemental-trials"],
  size: [32, 32],
  origin: [16, 16],
  duration: 0.4,
  loop: false,
  fps: 12,
  bg: "dark",
  options: {
    gem: ["#0e2a2c", "#1f6b66", "#3fb3a2", "#9af0dc", "#ffffff"],
    stone: ["#2a2234", "#4d4160", "#7a6a8e"],
    seed: 43,
  },
};

export function draw(p, t, o) {
  const G = fit(ramp(o.gem), 5), S = fit(ramp(o.stone), 3);
  const rk = seg(t, 0, 0.18);
  if (rk < 1) p.ring(0, 0, 2 + ease.outCubic(rk) * 8, 1, rk < 0.5 ? G[4] : G[3], 1 - rk);
  if (t < 0.06) p.circle(0, 0, 3, G[4]);
  // 보석 조각
  burst(7, o.seed, (r) => {
    const a = r.range(0, TAU), sp = r.range(50, 90), life = r.range(0.22, 0.35);
    if (t > life) return;
    const [x, y] = fly(0, 0, Math.cos(a) * sp, Math.sin(a) * sp, t, 120, 4);
    const k = t / life;
    p.dot(x, y, k < 0.5 ? G[3] : G[2], 1);
    p.dot(x + Math.cos(a), y + Math.sin(a), G[1], 1);
  });
  // 돌가루
  burst(6, o.seed + 1, (r) => {
    const a = r.range(0, TAU), d = r.range(3, 9), k = seg(t, 0, r.range(0.3, 0.4));
    if (k >= 1) return;
    p.circle(Math.cos(a) * d * (0.5 + k), Math.sin(a) * d * (0.5 + k), 1 + k * 1.5, k < 0.5 ? S[2] : S[1], 0.8 * (1 - k));
  });
}
