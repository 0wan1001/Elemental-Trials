// Elemental Trials: 과부하 반응 폭발 (1회). 흰 섬광 뒤 불덩이와 금빛 번개가 함께 사방으로 터지고, 충격 고리가 퍼지며 파편이 흩어진다.
// 원점이 폭발 중심. 반지름 약 40px.
import { ramp, fit, rng, bolt, burst, fly, seg, ease, TAU } from "../../core/index.js";

export const meta = {
  name: "ET overload",
  desc: "Overload reaction blast: a white flash, then fire blobs and golden lightning bursting out together, a shock ring racing out and debris flung away.",
  tags: ["explosion", "overload", "reaction", "elemental-trials"],
  size: [96, 96],
  origin: [48, 48],
  duration: 0.6,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#3d1012", "#cc3a1a", "#f2701f", "#ffd23c", "#ffffff"], bolt: ["#3a52c8", "#7cc8ff", "#ffe45a"], seed: 33 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), B = fit(ramp(o.bolt), 3);
  // 섬광
  if (t < 0.1) { p.circle(0, 0, 14 - t * 40, C[4]); p.circle(0, 0, 18 - t * 40, C[3], 0.6); }
  // 충격 고리
  const rk = seg(t, 0.04, 0.4);
  if (rk > 0 && rk < 1) p.ring(0, 0, 6 + ease.outCubic(rk) * 36, rk < 0.5 ? 3 : 1, rk < 0.4 ? C[3] : C[1], 1 - rk * 0.6);
  // 불덩이
  burst(10, o.seed, (r, i) => {
    const a = (i / 10) * TAU + r.range(-0.3, 0.3), born = r.range(0.03, 0.08), k = seg(t, born, born + r.range(0.3, 0.45));
    if (t < born || k >= 1) return;
    const d = ease.outQuart(k) * r.range(18, 30), rad = r.range(4, 7) * (1 - ease.inQuad(k));
    if (rad < 0.8) return;
    p.circle(Math.cos(a) * d, Math.sin(a) * d - k * 4, rad, C[Math.max(1, 3 - Math.floor(k * 3))]);
  });
  // 금빛 번개: 중심에서 바깥으로 (프레임마다 모양이 바뀐다)
  const bk = seg(t, 0.02, 0.3);
  if (bk > 0 && bk < 1) {
    const r = rng(o.seed, 50 + Math.floor(t * 12));
    for (let i = 0; i < 6; i++) {
      const a = (i / 6) * TAU + r.range(-0.3, 0.3), len = (20 + r.range(0, 16)) * (0.5 + ease.outCubic(bk) * 0.5);
      const pts = bolt(r, 0, 0, Math.cos(a) * len, Math.sin(a) * len, { jag: 0.4, depth: 3 });
      for (let j = 1; j < pts.length; j++) { p.line(pts[j - 1][0] + 1, pts[j - 1][1], pts[j][0] + 1, pts[j][1], B[0]); p.line(pts[j - 1][0], pts[j - 1][1], pts[j][0], pts[j][1], bk < 0.5 ? B[2] : B[1]); }
    }
  }
  // 파편
  burst(16, o.seed + 9, (r) => {
    const a = r.range(0, TAU), sp = r.range(80, 150), life = r.range(0.3, 0.5);
    if (t > life || t < 0.03) return;
    const [x, y] = fly(0, 0, Math.cos(a) * sp, Math.sin(a) * sp, t, 200, 4);
    p.dot(x, y, t / life < 0.5 ? C[3] : C[1], 1);
  });
}
