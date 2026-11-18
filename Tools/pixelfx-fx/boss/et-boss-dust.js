// Elemental Trials: 보스 돌진 중 발밑에서 피어오르는 흙먼지 한 덩이 (1회). 돌진하는 동안 짧은 간격으로 여러 번 뿌린다.
// 뒤쪽(왼쪽)으로 흘러가며 커지고 옅어진다. 진행 방향은 게임에서 좌우 반전으로 맞춘다. 원점이 발밑.
import { ramp, fit, burst, seg, ease } from "../../core/index.js";

export const meta = {
  name: "ET boss dust",
  desc: "A puff of floor dust kicked up behind a charging boss: billows backward, grows and fades, with a few pebbles.",
  tags: ["boss", "dust", "elemental-trials"],
  size: [40, 24],
  origin: [28, 18],
  duration: 0.5,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { dust: ["#2e2836", "#5a5060", "#8a7f86", "#b3a89f"], seed: 47 },
};

export function draw(p, t, o) {
  const D = fit(ramp(o.dust), 4);
  burst(5, o.seed, (r) => {
    const born = r.range(0, 0.08), k = seg(t, born, born + r.range(0.35, 0.42));
    if (t < born || k >= 1) return;
    const x = -ease.outCubic(k) * r.range(10, 22) + r.range(-2, 2), y = -ease.outCubic(k) * r.range(3, 8);
    const rad = 2 + k * r.range(3, 5);
    p.circle(x, y, rad, k < 0.3 ? D[3] : k < 0.6 ? D[2] : D[1], 0.9 * (1 - k));
  });
  burst(3, o.seed + 1, (r) => {
    const k = seg(t, 0, r.range(0.25, 0.35));
    if (k >= 1) return;
    p.dot(-k * r.range(8, 16), -Math.sin(k * Math.PI) * r.range(3, 6), D[3], 1);
  });
}
