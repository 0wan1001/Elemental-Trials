// Elemental Trials: 텔레포트 도착 (1회). 도착한 자리에 보라색 빛이 번쩍하고 네 갈래 반짝임과 작은 고리가 퍼진 뒤 사라진다. 원점이 몸 중심.
import { ramp, fit, burst, seg, ease, TAU } from "../../core/index.js";

export const meta = {
  name: "ET teleport arrive",
  desc: "Arriving from a dash: a violet pop at the body, a four-point sparkle and a small ring spreading out, motes scattering.",
  tags: ["arcane", "teleport", "elemental-trials"],
  size: [36, 36],
  origin: [18, 18],
  duration: 0.3,
  loop: false,
  fps: 20,
  bg: "dark",
  options: { palette: ["#2a1450", "#5a2a9a", "#8a5ae0", "#c0a0ff", "#f4ecff"], seed: 40 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), k = seg(t, 0, 0.28);
  if (k >= 1) return;
  if (k < 0.3) p.circle(0, 0, 5 - k * 10, C[4]);
  const len = 4 + ease.outCubic(k) * 10;
  for (let i = 0; i < 4; i++) {
    const a = (i / 4) * TAU;
    p.line(0, 0, Math.cos(a) * len * (1 - k * 0.5), Math.sin(a) * len * (1 - k * 0.5), k < 0.5 ? C[4] : C[3], 1 - k);
  }
  p.ring(0, 0, 3 + ease.outCubic(k) * 12, 1, k < 0.5 ? C[3] : C[1], 1 - k);
  burst(8, o.seed, (r) => {
    const a = r.range(0, TAU), d = 4 + ease.outCubic(k) * r.range(8, 14);
    p.dot(Math.cos(a) * d, Math.sin(a) * d, k < 0.5 ? C[4] : C[2], 1 - k);
  });
}
