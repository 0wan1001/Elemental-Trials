// Elemental Trials: 물 풀차지 (반복). 지팡이 끝 구슬이 커져서 크게 맥동하고, 네 갈래 별빛이 돌며 반짝인다. 원점이 구슬 중심.
// 같은 종류(차징·풀차지·선택 띠·포인트 마법진)끼리는 색만 다르고 그리는 코드가 같다. 모양을 바꾸면 같은 종류 파일도 같이 고친다.
import { ramp, fit, TAU } from "../../core/index.js";

export const meta = {
  name: "ET charged water",
  desc: "water full charge held at the staff tip: a big bright core pulsing hard, a turning four-point sparkle and orbiting motes.",
  tags: ["water", "charge", "full", "elemental-trials"],
  size: [36, 36],
  origin: [18, 18],
  duration: 0.4,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#123a7a","#1f6ac8","#3ea4ea","#8ad8f8","#e8fbff"], seed: 12 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), ph = (t / meta.duration) * TAU;
  const pr = 4.5 + Math.sin(ph) * 1.2;
  p.circle(0, 0, pr + 2, C[1]);
  p.circle(0, 0, pr + 0.8, C[2]);
  p.circle(0, 0, pr, C[3]);
  p.circle(-1, -1, pr * 0.55, C[4]);
  // 네 갈래 별빛
  const rot = ph * 0.25, len = 9 + Math.sin(ph) * 3;
  for (let i = 0; i < 4; i++) {
    const a = rot + (i / 4) * TAU, ex = Math.cos(a) * len, ey = Math.sin(a) * len;
    p.line(Math.cos(a) * pr, Math.sin(a) * pr, ex, ey, i % 2 ? C[3] : C[4]);
  }
  // 도는 작은 점
  for (let i = 0; i < 3; i++) {
    const a = -ph + (i / 3) * TAU;
    p.px(Math.cos(a) * (pr + 5), Math.sin(a) * (pr + 5), C[4]);
  }
}
