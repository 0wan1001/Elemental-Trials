// Elemental Trials: 화염구 폭발. 노란 섬광 뒤 불꽃 덩어리가 퍼지며 식고, 불티가 흩어진다. 빛 번짐 없음.
import { ramp, fit, burst, fly, seg, ease, TAU } from "../../core/index.js";

// 그림 크기 배율: 폭발 판정 반지름 1.5칸(PPU 32 → 48px)에 맞춘다
const S = 2.4;

export const meta = {
  name: "ET fireball burst",
  desc: "A flat fireball pop: a yellow star frame, flame blobs bursting out and cooling from yellow to dark red, a red rim ring, sparks flung away.",
  tags: ["fire", "explosion", "impact", "elemental-trials"],
  size: [108, 116],
  origin: [54, 54],
  duration: 0.55,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#3d1012", "#8c1d1c", "#cc3a1a", "#f2701f", "#ffd23c", "#fff3b0"], seed: 5 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 6);
  // 첫 프레임: 노란 별 모양 섬광
  if (t < 0.09) {
    p.circle(0, 0, 6 * S, C[4]);
    p.circle(0, 0, 3.5 * S, C[5]);
    for (let i = 0; i < 4; i++) {
      const a = (i / 4) * TAU + 0.4;
      p.tri(Math.cos(a + 0.5) * 4 * S, Math.sin(a + 0.5) * 4 * S, Math.cos(a - 0.5) * 4 * S, Math.sin(a - 0.5) * 4 * S,
        Math.cos(a) * 11 * S, Math.sin(a) * 11 * S, C[4]);
    }
  }
  // 바깥 테두리 고리
  const rk = seg(t, 0.05, 0.3);
  if (rk > 0 && rk < 1) p.ring(0, 0, (4 + ease.outQuad(rk) * 16) * S, rk < 0.5 ? 3 : 2, rk < 0.5 ? C[2] : C[1]);
  // 불꽃 덩어리: 퍼지면서 식는다
  burst(10, o.seed, (r, i) => {
    const a = (i / 10) * TAU + r.range(-0.3, 0.3), born = r.range(0.03, 0.08), life = r.range(0.32, 0.45);
    const k = seg(t, born, born + life);
    if (t < born || k >= 1) return;
    const d = ease.outQuart(k) * r.range(7, 12) * S, rad = r.range(2.5, 4) * S * (1 - ease.inQuad(k));
    const x = Math.cos(a) * d, y = Math.sin(a) * d - k * 3 * S;
    if (rad < 0.6) return;
    p.circle(x, y, rad, C[Math.max(1, 4 - Math.floor(k * 4))]);
    if (k < 0.45) p.circle(x - 0.5 * S, y - 0.5 * S, rad * 0.5, C[5 - Math.floor(k * 4)]);
  });
  // 불티
  burst(14, o.seed + 7, (r) => {
    const a = r.range(0, TAU), sp = r.range(60, 110) * S, life = r.range(0.25, 0.4);
    if (t > life || t < 0.03) return;
    const [x, y] = fly(0, 0, Math.cos(a) * sp, Math.sin(a) * sp, t, 120, 4);
    p.px(x, y, t / life < 0.5 ? C[4] : C[2]);
  });
}
