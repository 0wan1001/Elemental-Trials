// Elemental Trials: 보스 탄환 (반복). "고대의 수호자" 컨셉: 청록 보석 조각이 소용돌이 문양 빛을 두르고 돌며 오른쪽으로 날아가고,
// 뒤로 보라빛 돌가루가 흩날린다. 구슬 지름 약 12px(기존 판정 크기와 비슷). 원점이 탄 중심.
import { ramp, fit, cycle, TAU } from "../../core/index.js";

export const meta = {
  name: "ET boss bullet",
  desc: "Ancient guardian bullet: a spinning teal gem shard wrapped in a glowing rune spiral, trailing purple stone dust.",
  tags: ["projectiles", "boss", "elemental-trials"],
  size: [30, 18],
  origin: [20, 9],
  duration: 0.5,
  loop: true,
  fps: 12,
  bg: "dark",
  options: {
    gem: ["#0e2a2c", "#1f6b66", "#3fb3a2", "#9af0dc", "#ffffff"],
    stone: ["#2a2234", "#4d4160", "#7a6a8e"],
    seed: 41,
  },
};

export function draw(p, t, o) {
  const G = fit(ramp(o.gem), 5), S = fit(ramp(o.stone), 3), ph = (t / meta.duration) * TAU;
  // 돌가루 꼬리
  cycle(t, meta.duration, 5, o.seed, 0.35, (age, life, r) => {
    const k = age / life;
    p.dot(-5 - k * 13, r.range(-3, 3) + Math.sin(k * 6) * 0.5, k < 0.4 ? S[2] : S[1], 1 - k * 0.6);
  });
  p.tri(-2, -3, -2, 3, -10, 0, G[1], 0.6);
  // 빛 테두리
  p.circle(0, 0, 6.5, G[0]);
  p.circle(0, 0, 5.5, G[1]);
  // 소용돌이 문양: 바깥에서 안으로 감기는 점들이 돈다
  for (let i = 0; i < 10; i++) {
    const u = i / 10, a = ph + u * TAU * 1.5, r = 5 - u * 3.5;
    p.dot(Math.cos(a) * r, Math.sin(a) * r, u < 0.5 ? G[3] : G[2], 1);
  }
  // 육각 보석 핵
  const hex = [];
  for (let i = 0; i < 6; i++) {
    const a = ph * 0.5 + (i / 6) * TAU;
    hex.push([Math.cos(a) * 2.6, Math.sin(a) * 2.6]);
  }
  p.poly(hex, G[2]);
  p.dot(0, -1, G[4], 1);
  p.dot(1, -1, G[3], 1);
}
