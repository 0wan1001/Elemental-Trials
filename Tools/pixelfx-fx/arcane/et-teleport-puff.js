// Elemental Trials: 대시(텔레포트) 시작 지점에 남는 짧은 연기. 단색 3단계, 빛 번짐 없음.
import { ramp, fit, burst, fly, seg, ease, TAU } from "../../core/index.js";

export const meta = {
  name: "ET teleport puff",
  desc: "A short flat puff where the caster vanishes: grey-white blobs pop out, swell and shrink away, a few blue motes rise.",
  tags: ["arcane", "teleport", "elemental-trials"],
  size: [32, 32],
  origin: [16, 24],
  duration: 0.5,
  loop: false,
  fps: 12,
  bg: "dark",
  options: {
    palette: ["#3a3644", "#6b6577", "#a8a2b4", "#eeeaf4"],
    motes: ["#2550d0", "#7cc8ff"],
    seed: 3,
  },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 4), M = ramp(o.motes);
  // 1프레임: 몸이 있던 자리의 흰 섬광
  if (t < 0.08) {
    p.ellipse(0, -8, 4, 7, C[2]);
    p.ellipse(0, -8, 2, 5, C[3]);
  }
  // 연기 덩어리: 빠르게 부풀고 천천히 줄어든다
  burst(6, o.seed, (r, i) => {
    const a = (i / 6) * TAU + r.range(-0.4, 0.4), d = r.range(3, 6);
    const born = r.range(0, 0.06), life = r.range(0.3, 0.42), k = seg(t, born, born + life);
    if (t < born || k >= 1) return;
    const grow = ease.outQuad(Math.min(1, k * 2.5)), shrink = 1 - ease.inQuad(k);
    const rad = (2 + r.range(1.5, 3) * grow) * shrink;
    const x = Math.cos(a) * d * (0.5 + grow), y = -8 + Math.sin(a) * d * 0.8 * (0.5 + grow) - k * 4;
    if (rad < 0.6) return;
    p.circle(x, y + 1, rad, C[0]);
    p.circle(x, y, rad, C[1]);
    p.circle(x - rad * 0.3, y - rad * 0.3, rad * 0.6, k < 0.5 ? C[3] : C[2]);
  });
  // 위로 흩어지는 푸른 점
  burst(5, o.seed + 9, (r) => {
    const born = r.range(0.04, 0.12), life = r.range(0.25, 0.35);
    if (t < born || t > born + life) return;
    const age = t - born, [x, y] = fly(r.range(-6, 6), -6, r.range(-15, 15), r.range(-45, -25), age, 0, 2);
    p.px(x, y, age / life < 0.5 ? M[1] : M[0]);
  });
}
