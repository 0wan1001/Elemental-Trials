// Elemental Trials: 날아가는 화염구 (반복). 노랑·주황·검붉은 단색 3단계 혜성, 오른쪽을 본다.
import { ramp, fit, cycle, TAU } from "../../core/index.js";

// 그림 크기 배율 (차징 강화로 2배)
const S = 2;

export const meta = {
  name: "ET fireball",
  desc: "A flat three-tone fireball flying right: a round yellow head, flame tongues licking back into a dark red tail, sparks dropping off.",
  tags: ["fire", "projectile", "elemental-trials"],
  size: [64, 32],
  origin: [44, 16],
  duration: 0.5,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#701a14", "#cc3a1a", "#f2701f", "#ffd23c", "#fff3b0"], seed: 2 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), ph = t / meta.duration;
  // 꼬리 불꽃: 뒤로 갈수록 가늘고 어둡다
  for (let i = 0; i < 3; i++) {
    const w = Math.sin((ph + i / 3) * TAU), dy = (i - 1) * 2.2 + w * 1.2, len = 12 + i % 2 * 3 + w * 2;
    p.tri(-1 * S, (dy - 2.5) * S, -1 * S, (dy + 2.5) * S, -len * S, (dy + w * 1.5) * S, C[i === 1 ? 2 : 1]);
  }
  p.tri(0, -3 * S, 0, 3 * S, (-18 - Math.sin(ph * TAU) * 2) * S, Math.sin(ph * TAU * 2) * S, C[0]);
  p.tri(0, -2 * S, 0, 2 * S, (-9 - Math.cos(ph * TAU) * 1.5) * S, 0, C[2]);
  // 머리: 둥근 노란 덩어리, 앞쪽이 가장 밝다
  p.circle(0, 0, 4 * S, C[2]);
  p.circle(0.5 * S, -0.5 * S, 3 * S, C[3]);
  p.circle(1.5 * S, -1 * S, 1.5 * S, C[4]);
  // 뒤로 떨어지는 불티
  cycle(t, meta.duration, 6, o.seed, 0.35, (age, life, r) => {
    const k = age / life;
    p.px((-4 - k * 14 - r.range(0, 3)) * S, (r.range(-3, 3) + k * 2) * S, k < 0.5 ? C[3] : C[1]);
  });
}
