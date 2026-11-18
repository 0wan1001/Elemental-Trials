// Elemental Trials: 불장판 한 칸(16×16) 반복 타일. 달아오른 바닥 위로 작은 불꽃 혀가 번갈아 솟고 불씨가 올라간다.
// 이웃 칸과 이어지도록 감싸서 그린다. 원점이 칸 가운데.
import { ramp, fit, rng, TAU } from "../../core/index.js";

export const meta = {
  name: "ET floor fire",
  desc: "One seamless 16px tile of burning ground: a glowing dark-red bed with small flame tongues flaring in turn and embers rising.",
  tags: ["fire", "floor", "tile", "elemental-trials"],
  size: [16, 16],
  origin: [8, 8],
  duration: 1,
  loop: true,
  fps: 8,
  bg: "dark",
  options: { palette: ["#5a1410", "#b02a18", "#e8561c", "#ffa030", "#ffe68a"], seed: 22 },
};

const wrap = (v) => ((((v + 8) % 16) + 16) % 16) - 8;

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), ph = t / meta.duration;
  for (let y = -8; y < 8; y++) for (let x = -8; x < 8; x++) p.dot(x, y, C[0], 0.5);
  for (let y = -8; y < 8; y++) for (let x = -8; x < 8; x++) {
    if ((x * 7 + y * 3 + 64) % 5 === 0) p.dot(x, y, C[1], 0.55);
  }
  // 불꽃 혀 셋: 자리마다 위상이 달라 번갈아 솟는다
  const r = rng(o.seed, 0);
  for (let i = 0; i < 3; i++) {
    const fx = Math.round(r.range(-8, 8)), fy = Math.round(r.range(-3, 7));
    const k = (Math.sin((ph + i / 3) * TAU) + 1) / 2, h = 2 + k * 5;
    for (let j = 0; j <= h; j++) {
      const w = Math.max(0, Math.round((1 - j / (h + 1)) * 1.5));
      for (let dx = -w; dx <= w; dx++) {
        const c = j > h * 0.6 ? C[2] : dx === 0 && j < h * 0.5 ? C[4] : C[3];
        p.dot(wrap(fx + dx), wrap(fy - j), c, 0.9);
      }
    }
  }
  // 불씨
  for (let i = 0; i < 2; i++) {
    const q = (ph + i * 0.5) % 1, ex = Math.round(r.range(-8, 8));
    p.dot(wrap(ex + Math.round(Math.sin(q * TAU))), wrap(Math.round(6 - q * 16)), q < 0.6 ? C[4] : C[2], 1);
  }
}
