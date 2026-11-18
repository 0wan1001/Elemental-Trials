// Elemental Trials: 증기 바닥 한 칸(16×16) 반복 타일. 옅은 김이 깔리고 둥근 김 덩어리가 천천히 피어오른다.
// 이웃 칸과 이어지도록 감싸서 그린다. 원점이 칸 가운데.
import { ramp, fit, rng, TAU } from "../../core/index.js";

export const meta = {
  name: "ET floor steam",
  desc: "One seamless 16px tile of steam: a pale haze over the floor with soft puffs swelling and drifting up through it.",
  tags: ["steam", "floor", "tile", "elemental-trials"],
  size: [16, 16],
  origin: [8, 8],
  duration: 2,
  loop: true,
  fps: 8,
  bg: "dark",
  options: { palette: ["#6b7685", "#a8b2c0", "#dfe6ee", "#ffffff"], seed: 23 },
};

const wrap = (v) => ((((v + 8) % 16) + 16) % 16) - 8;

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 4), ph = t / meta.duration;
  for (let y = -8; y < 8; y++) for (let x = -8; x < 8; x++) p.dot(x, y, C[1], 0.3);
  // 김 덩어리 셋: 위로 한 바퀴(16px) 흘러 이음새가 없다
  const r = rng(o.seed, 0);
  for (let i = 0; i < 3; i++) {
    const cx = r.range(-8, 8), cy = r.range(-8, 8) - ph * 16, rad = 2.5 + Math.sin((ph + i / 3) * TAU) * 1;
    for (let y = -4; y <= 4; y++) for (let x = -4; x <= 4; x++) {
      const d = Math.hypot(x, y);
      if (d > rad) continue;
      p.dot(wrap(Math.round(cx + x + Math.sin(ph * TAU + i) )), wrap(Math.round(cy + y)), d < rad * 0.5 ? C[3] : C[2], 0.45);
    }
  }
}
