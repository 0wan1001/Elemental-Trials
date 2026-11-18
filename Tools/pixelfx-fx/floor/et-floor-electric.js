// Elemental Trials: 전도된 젖은 바닥 한 칸(16×16) 반복 타일. 물 위로 가로·세로 지그재그 전기가 지직거리며 이웃 칸과 이어진다.
// 전기 줄은 칸 가운데 줄(가로 y=0, 세로 x=0)에서 시작·끝나서 옆 칸 줄과 맞물린다. 원점이 칸 가운데.
import { ramp, fit, rng } from "../../core/index.js";

export const meta = {
  name: "ET floor electric",
  desc: "One seamless 16px tile of electrified water: a cyan film with jagged arcs running across and down the tile, meeting the neighbours at the edges, re-forming every frame.",
  tags: ["lightning", "conduct", "floor", "tile", "elemental-trials"],
  size: [16, 16],
  origin: [8, 8],
  duration: 0.5,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#1f6ac8", "#7cc8ff", "#ffe45a", "#ffffff"], seed: 24 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 4), f = Math.floor(t * meta.fps);
  for (let y = -8; y < 8; y++) for (let x = -8; x < 8; x++) p.dot(x, y, C[0], 0.4);
  const r = rng(o.seed, f);
  // 가로 줄과 세로 줄: 가장자리에서는 가운데(0)로 돌아와 옆 칸과 이어진다
  const zig = (i) => (i === 0 || i === 4 ? 0 : Math.round(r.range(-3, 3)));
  const hs = [0, 1, 2, 3, 4].map((i) => [-8 + i * 4, zig(i)]);
  const vs = [0, 1, 2, 3, 4].map((i) => [zig(i), -8 + i * 4]);
  for (const pts of [hs, vs]) {
    for (let i = 1; i < pts.length; i++) p.line(pts[i - 1][0], pts[i - 1][1], Math.min(7, pts[i][0]), Math.min(7, pts[i][1]), f % 2 ? C[1] : C[3]);
  }
  if (r.next() < 0.6) p.dot(Math.round(r.range(-7, 7)), Math.round(r.range(-7, 7)), C[2], 1);
}
