// Elemental Trials: 감전 상태 (반복). 몸을 감싸는 짧은 지그재그 전기가 프레임마다 모양을 바꿔 튀고, 노란 스파크가 번쩍인다.
// 플레이어 몸(32px) 기준 크기. 큰 대상은 프리팹을 정수 배로 키워서 붙인다. 원점이 몸 중심.
import { ramp, fit, rng, bolt, TAU } from "../../core/index.js";

export const meta = {
  name: "ET shock",
  desc: "Electrocuted status: jagged arcs snap around the body in new shapes every frame, with yellow sparks popping and a faint flicker.",
  tags: ["lightning", "status", "shock", "elemental-trials"],
  size: [40, 44],
  origin: [20, 22],
  duration: 0.5,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#2550d0", "#7cc8ff", "#ffe45a", "#ffffff"], seed: 32 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 4), f = Math.floor(t * meta.fps);
  const r = rng(o.seed, f);
  // 몸 둘레 전기 세 줄: 타원 위의 두 점을 잇는다
  for (let i = 0; i < 3; i++) {
    const a0 = r.range(0, TAU), a1 = a0 + r.range(0.8, 1.8);
    const pts = bolt(r, Math.cos(a0) * 11, Math.sin(a0) * 15, Math.cos(a1) * 11, Math.sin(a1) * 15, { jag: 0.45, depth: 3 });
    for (let j = 1; j < pts.length; j++) p.line(pts[j - 1][0], pts[j - 1][1], pts[j][0], pts[j][1], i === 0 ? C[3] : C[1]);
  }
  // 스파크
  for (let i = 0; i < 4; i++) {
    const x = Math.round(r.range(-13, 13)), y = Math.round(r.range(-17, 17));
    p.dot(x, y, C[2], 1);
    if (r.next() < 0.5) { p.dot(x - 1, y, C[2], 1); p.dot(x + 1, y, C[2], 1); p.dot(x, y - 1, C[2], 1); p.dot(x, y + 1, C[2], 1); }
  }
}
