// Elemental Trials: 물 스킬을 골랐을 때 몸 위쪽에 떠 있는 작은 물 원소 (반복). 출렁이는 물방울과 반짝임, 도는 작은 물알.
import { ramp, fit, TAU } from "../../core/index.js";

export const meta = {
  name: "ET orb water",
  desc: "A tiny floating water element: a wobbling droplet with a glint and a small bead circling it.",
  tags: ["water", "orb", "element", "elemental-trials"],
  size: [18, 18],
  origin: [9, 10],
  duration: 0.8,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#123a7a", "#1f6ac8", "#3ea4ea", "#8ad8f8", "#e8fbff"], seed: 3 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), ph = (t / meta.duration) * TAU;
  const bob = Math.round(Math.sin(ph));
  const wx = 3.2 + Math.sin(ph * 2) * 0.5, wy = 3.2 - Math.sin(ph * 2) * 0.5;
  // 위쪽이 뾰족한 물방울
  p.tri(-2.2, bob - 1, 2.2, bob - 1, 0, bob - 6, C[2]);
  p.ellipse(0, bob, wx, wy, C[2]);
  p.ellipse(0, bob + 0.5, wx - 1, wy - 1, C[1]);
  p.px(-1, bob - 1, C[4]);
  p.px(-1, bob, C[3]);
  // 둘레를 도는 작은 물알
  const bx = Math.cos(ph) * 6, by = Math.sin(ph) * 2.5 + bob;
  p.px(bx, by, Math.sin(ph) > 0 ? C[3] : C[2]);
}
