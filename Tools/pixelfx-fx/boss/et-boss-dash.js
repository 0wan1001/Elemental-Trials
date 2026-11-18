// Elemental Trials: 보스 돌진 중 (반복). 오른쪽으로 달려가는 보스 뒤로 붉은 속도선이 흐르고 흙먼지가 튀어 뒤로 흩어진다.
// 원점이 보스 발밑 가운데. 보스(56×80px)의 진행 방향으로 돌려서 붙인다.
import { ramp, fit, cycle } from "../../core/index.js";

export const meta = {
  name: "ET boss dash",
  desc: "Boss charging right: red speed lines streaming behind it and dust kicked up at its feet, tumbling back and fading.",
  tags: ["boss", "dash", "elemental-trials"],
  size: [96, 72],
  origin: [64, 40],
  duration: 0.4,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#5a0a0a", "#c81e1e", "#ff8070"], dust: ["#3a3230", "#6a5a50", "#9a8a78"], seed: 37 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 3), D = fit(ramp(o.dust), 3);
  // 속도선: 몸 높이 곳곳에서 뒤로 흐른다
  cycle(t, meta.duration, 9, o.seed, 0.2, (age, life, r) => {
    const k = age / life, y = r.range(-34, 8), len = r.range(12, 26), x = -10 - k * 40;
    p.line(x, y, x - len, y, k < 0.5 ? C[2] : C[1], 1 - k);
  });
  // 흙먼지
  cycle(t, meta.duration, 8, o.seed + 1, 0.35, (age, life, r) => {
    const k = age / life, x = -8 - k * r.range(20, 40), y = r.range(-2, 4) - k * r.range(4, 10);
    p.circle(x, y, 1.5 + k * 3, k < 0.4 ? D[2] : k < 0.7 ? D[1] : D[0], 0.8 * (1 - k));
  });
}
