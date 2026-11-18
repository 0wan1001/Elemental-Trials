// Elemental Trials: 풀차지가 된 순간의 섬광 (공용, 흰색). 흰 원이 번쩍하고 긴 네 갈래 별빛과 고리가 퍼진다. 원점이 지팡이 끝.
import { ramp, fit, seg, ease, TAU } from "../../core/index.js";

export const meta = {
  name: "ET charge ready",
  desc: "The instant a charge is full: a white pop, long four-point star rays and a ring flying out.",
  tags: ["charge", "ready", "flash", "elemental-trials"],
  size: [48, 48],
  origin: [24, 24],
  duration: 0.3,
  loop: false,
  fps: 20,
  bg: "dark",
  options: { palette: ["#9aa6c8", "#dfe6ff", "#ffffff"], seed: 15 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 3);
  const k = seg(t, 0, 0.28);
  if (k >= 1) return;
  if (k < 0.3) p.circle(0, 0, 6 - k * 8, C[2]);
  const len = 6 + ease.outCubic(k) * 16, w = k < 0.5 ? 1 : 0;
  for (let i = 0; i < 4; i++) {
    const a = (i / 4) * TAU, ex = Math.cos(a) * len * (1 - k * 0.4), ey = Math.sin(a) * len * (1 - k * 0.4);
    p.line(0, 0, ex, ey, k < 0.5 ? C[2] : C[1]);
    if (w) p.line(-Math.sin(a), Math.cos(a), ex * 0.6, ey * 0.6, C[1]);
  }
  p.ring(0, 0, 3 + ease.outCubic(k) * 14, 1, k < 0.5 ? C[2] : C[0], 1 - k);
}
