// Elemental Trials: 번개 스킬을 골랐을 때 몸 오른쪽에 떠 있는 작은 번개 원소 (반복). 노란 번개 모양 아이콘이 떠서 깜빡이고 작은 불꽃이 튄다.
import { ramp, fit, rng, TAU } from "../../core/index.js";

export const meta = {
  name: "ET orb lightning",
  desc: "A tiny floating lightning element: a yellow zigzag bolt icon with a dark outline that bobs, flickers bright and sheds a spark now and then.",
  tags: ["lightning", "orb", "element", "elemental-trials"],
  size: [18, 22],
  origin: [9, 11],
  duration: 0.5,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#6a3a08", "#e0a020", "#ffd23c", "#fff3b0"], seed: 4 },
};

// 번개 아이콘 (위 오른쪽에서 아래 왼쪽으로 꺾이는 모양)
const BOLT = [[2, -7], [-3, 1], [0, 1], [-2, 7], [4, -1], [1, -1], [3, -7]];

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 4), ph = (t / meta.duration) * TAU;
  const bob = Math.round(Math.sin(ph));
  const f = Math.floor(t * 12);
  const at = (dx, dy) => BOLT.map(([x, y]) => [x + dx, y + bob + dy]);
  // 어두운 테두리 → 몸 → 밝은 심
  for (const [dx, dy] of [[-1, 0], [1, 0], [0, -1], [0, 1]]) p.poly(at(dx, dy), C[0]);
  p.poly(at(0, 0), f % 3 === 0 ? C[3] : C[2]);
  p.line(1, bob - 5, -1, bob, C[3]);
  p.line(0, bob + 2, -1, bob + 5, C[3]);
  // 가끔 튀는 작은 불꽃
  const r = rng(o.seed, f);
  if (r.next() < 0.6) p.px(r.range(-6, 6), bob + r.range(-8, 8), C[3]);
}
