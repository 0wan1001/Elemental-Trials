// Elemental Trials: 플레이어 기본 공격 탄 (반복). 흰 중심을 연보라 빛이 감싸고, 뒤로 짧은 꼬리와 반짝이가 흩날린다.
// 어두운 바닥에서도 잘 보이게 밝게 그린다. 원점이 탄 중심, 오른쪽으로 날아간다.
import { ramp, fit, cycle, TAU } from "../../core/index.js";

export const meta = {
  name: "ET arcane bolt",
  desc: "Player basic attack: a bright white core wrapped in pale violet glow, trailing a short tail and sparkles.",
  tags: ["projectiles", "arcane", "elemental-trials"],
  size: [32, 16],
  origin: [22, 8],
  duration: 0.4,
  loop: true,
  fps: 12,
  bg: "dark",
  options: {
    glow: ["#3b2470", "#6e4fd0", "#a98cff", "#dccfff", "#ffffff"],
    seed: 51,
  },
};

export function draw(p, t, o) {
  const G = fit(ramp(o.glow), 5), ph = (t / meta.duration) * TAU;
  // 반짝이 꼬리
  cycle(t, meta.duration, 6, o.seed, 0.3, (age, life, r) => {
    const k = age / life;
    p.dot(-4 - k * 16, r.range(-3, 3) * (1 - k * 0.5), k < 0.4 ? G[4] : G[2], 1 - k * 0.7);
  });
  // 꼬리
  p.tri(-2, -3, -2, 3, -16, 0, G[1], 0.55);
  p.tri(-2, -2, -2, 2, -11, 0, G[2], 0.8);
  // 빛무리
  p.circle(0, 0, 5.5 + Math.sin(ph) * 0.5, G[1], 0.6);
  p.circle(0, 0, 4.2, G[2]);
  p.circle(0, 0, 3, G[3]);
  // 흰 중심
  p.circle(0.5, 0, 1.8, G[4]);
  // 도는 반짝임 두 점
  for (let i = 0; i < 2; i++) {
    const a = ph * 2 + i * Math.PI;
    p.dot(Math.cos(a) * 5, Math.sin(a) * 5, G[4], 1);
  }
}
