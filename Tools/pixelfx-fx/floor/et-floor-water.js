// Elemental Trials: 젖은 바닥 한 칸(16×16) 반복 타일. 반투명 물 위로 잔물결 줄이 흐르고 반짝임이 지나간다.
// 이웃 칸과 이어지도록 모든 그림이 칸 가장자리에서 반대편으로 넘어간다(감싸기). 원점이 칸 가운데.
import { ramp, fit, rng, TAU } from "../../core/index.js";

export const meta = {
  name: "ET floor water",
  desc: "One seamless 16px tile of shallow water: a translucent blue film with soft ripple lines drifting across and glints passing by.",
  tags: ["water", "floor", "tile", "elemental-trials"],
  size: [16, 16],
  origin: [8, 8],
  duration: 1.5,
  loop: true,
  fps: 8,
  bg: "dark",
  options: { palette: ["#123a7a", "#1f6ac8", "#3ea4ea", "#8ad8f8", "#e8fbff"], seed: 21 },
};

const wrap = (v) => ((((v + 8) % 16) + 16) % 16) - 8;

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), ph = t / meta.duration;
  for (let y = -8; y < 8; y++) for (let x = -8; x < 8; x++) p.dot(x, y, C[1], 0.42);
  // 잔물결 두 줄: 가로로 흐르며 위아래로 출렁인다 (한 바퀴에 정확히 16px 이동해 이음새가 없다)
  for (let k = 0; k < 2; k++) {
    const by = -5 + k * 8;
    for (let x = -8; x < 8; x++) {
      const y = by + Math.round(Math.sin(((x + 8) / 16) * TAU * 2 + ph * TAU + k) * 1);
      p.dot(wrap(x + Math.round(ph * 16)), y, C[2], 0.7);
    }
  }
  // 반짝임
  const r = rng(o.seed, 0);
  for (let i = 0; i < 2; i++) {
    const gx = r.range(-8, 8), gy = r.range(-8, 8), on = (ph * 2 + i * 0.5) % 1 < 0.35;
    if (on) p.dot(wrap(Math.round(gx + ph * 16)), Math.round(gy), C[4], 0.9);
  }
}
