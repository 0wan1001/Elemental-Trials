// Elemental Trials: 기본 공격 탄이 맞을 때 (1회). 흰 섬광 뒤로 연보라 고리가 퍼지고 반짝이 조각이 사방으로 튄다. 원점이 맞은 지점.
import { ramp, fit, burst, fly, seg, ease, TAU } from "../../core/index.js";

export const meta = {
  name: "ET arcane pop",
  desc: "Player basic attack hit: a white flash, a widening pale violet ring and sparkle shards bursting out.",
  tags: ["impacts", "arcane", "elemental-trials"],
  size: [28, 28],
  origin: [14, 14],
  duration: 0.3,
  loop: false,
  fps: 15,
  bg: "dark",
  options: {
    glow: ["#3b2470", "#6e4fd0", "#a98cff", "#dccfff", "#ffffff"],
    seed: 53,
  },
};

export function draw(p, t, o) {
  const G = fit(ramp(o.glow), 5), rk = seg(t, 0, 0.2);
  if (t < 0.07) p.circle(0, 0, 4, G[4]);
  else if (t < 0.12) p.circle(0, 0, 3, G[3]);
  if (rk < 1) p.ring(0, 0, 3 + ease.outCubic(rk) * 9, 1, rk < 0.5 ? G[4] : G[2], 1 - rk);
  burst(8, o.seed, (r) => {
    const a = r.range(0, TAU), sp = r.range(55, 95), life = r.range(0.18, 0.28);
    if (t > life) return;
    const [x, y] = fly(0, 0, Math.cos(a) * sp, Math.sin(a) * sp, t, 140, 4);
    p.dot(x, y, t / life < 0.5 ? G[4] : G[3], 1);
  });
}
