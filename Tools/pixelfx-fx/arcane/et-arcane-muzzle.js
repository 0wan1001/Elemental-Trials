// Elemental Trials: 기본 공격 발사 순간 지팡이 끝 번쩍임 (1회). 흰 점이 터지며 연보라 고리와 짧은 빛살 4개가 퍼진다. 원점이 지팡이 끝.
import { ramp, fit, seg, ease } from "../../core/index.js";

export const meta = {
  name: "ET arcane muzzle",
  desc: "Player basic attack muzzle flash at the staff tip: a white pop, a pale violet ring and four short rays.",
  tags: ["arcane", "elemental-trials"],
  size: [20, 20],
  origin: [10, 10],
  duration: 0.2,
  loop: false,
  fps: 15,
  bg: "dark",
  options: {
    glow: ["#3b2470", "#6e4fd0", "#a98cff", "#dccfff", "#ffffff"],
  },
};

export function draw(p, t, o) {
  const G = fit(ramp(o.glow), 5), k = seg(t, 0, meta.duration), e = ease.outCubic(k);
  if (k < 0.4) p.circle(0, 0, 3 - k * 4, G[4]);
  p.ring(0, 0, 2 + e * 6, 1, k < 0.5 ? G[3] : G[2], 1 - k);
  const len = 2 + e * 5;
  for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
    for (let i = 0; i < 3; i++) {
      const d = len - i;
      if (d > 1.5) p.dot(dx * d, dy * d, i === 0 ? G[4] : G[3], 1 - k);
    }
  }
}
