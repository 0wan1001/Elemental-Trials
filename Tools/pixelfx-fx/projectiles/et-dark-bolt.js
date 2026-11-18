// Elemental Trials: 기본 공격 어둠 탄 (반복). 화염구보다 작고 약한 보라·검정 구슬, 오른쪽을 본다. 빛 번짐 없음.
import { ramp, fit, cycle, TAU } from "../../core/index.js";

// 그림 크기 배율 (기본 탄 30% 키움)
const S = 1.3;

export const meta = {
  name: "ET dark bolt",
  desc: "A small flat dark orb flying right: a black-violet core with a pale violet rim, a short smoky tail and dark motes falling behind.",
  tags: ["projectiles", "shadow", "elemental-trials"],
  size: [32, 16],
  origin: [21, 8],
  duration: 0.5,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#0f0a18", "#2c1a4a", "#5e3290", "#9a48ec", "#d8b0ff"], seed: 6 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), ph = t / meta.duration, w = Math.sin(ph * TAU);
  // 꼬리: 뒤로 갈수록 작아지는 연기 덩어리 셋이 흔들린다
  for (let i = 0; i < 3; i++) {
    const y = Math.sin((ph + i * 0.3) * TAU) * (0.5 + i * 0.4);
    p.circle((-3.5 - i * 3) * S, y * S, (2 - i * 0.55) * S, i === 0 ? C[2] : C[1]);
  }
  // 구슬: 검은 테두리, 보라 속, 앞쪽 위가 밝다
  p.circle(0, 0, 3 * S, C[0]);
  p.circle(0, 0, 2.2 * S, C[2]);
  p.circle(0.6 * S, -0.6 * S, 1.1 * S, C[3]);
  p.px((1 + (w > 0 ? 1 : 0)) * S, -1 * S, C[4]);
  // 뒤로 떨어지는 어두운 점
  cycle(t, meta.duration, 3, o.seed, 0.3, (age, life, r) => {
    const k = age / life;
    p.px((-3 - k * 9 - r.range(0, 2)) * S, (r.range(-2, 2) + k) * S, k < 0.5 ? C[2] : C[1]);
  });
}
