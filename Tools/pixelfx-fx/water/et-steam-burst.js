// Elemental Trials: 증기 반응 (1회). 치익 하고 흰 김 덩어리들이 사방으로 부풀어 퍼진 뒤 위로 흩어지며 옅어진다. 원점이 반응 중심.
import { ramp, fit, burst, seg, ease, TAU } from "../../core/index.js";

export const meta = {
  name: "ET steam burst",
  desc: "Steam reaction: a hiss of white puffs swelling out on all sides, rising and thinning away, with a few hot droplets flicking out.",
  tags: ["steam", "reaction", "elemental-trials"],
  size: [72, 64],
  origin: [36, 40],
  duration: 0.8,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#6b7685", "#a8b2c0", "#dfe6ee", "#ffffff"], drop: "#8ad8f8", seed: 34 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 4), D = fit(ramp([o.drop, o.drop]), 1)[0];
  burst(12, o.seed, (r, i) => {
    const a = (i / 12) * TAU + r.range(-0.3, 0.3), born = r.range(0, 0.1), k = seg(t, born, born + r.range(0.5, 0.7));
    if (t < born || k >= 1) return;
    const d = ease.outCubic(k) * r.range(10, 24), rad = 3 + ease.outQuad(k) * r.range(4, 7);
    const x = Math.cos(a) * d, y = Math.sin(a) * d * 0.6 - k * 12;
    p.circle(x, y, rad, k < 0.3 ? C[3] : k < 0.6 ? C[2] : C[1], 0.85 * (1 - ease.inQuad(k)));
  });
  burst(8, o.seed + 5, (r) => {
    const a = r.range(-Math.PI, 0), k = seg(t, 0, 0.3);
    if (k >= 1) return;
    const d = ease.outQuad(k) * r.range(14, 24);
    p.dot(Math.cos(a) * d, Math.sin(a) * d, D, 1);
  });
}
