// Elemental Trials: 불 스킬을 골랐을 때 몸 왼쪽에 떠 있는 작은 불 원소 (반복). 일렁이는 불꽃 한 점과 위로 튀는 불티.
import { ramp, fit, cycle, TAU } from "../../core/index.js";

export const meta = {
  name: "ET orb fire",
  desc: "A tiny floating fire element: a flickering flame drop with a hot core and embers lifting off it.",
  tags: ["fire", "orb", "element", "elemental-trials"],
  size: [16, 20],
  origin: [8, 12],
  duration: 0.6,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#701a14", "#cc3a1a", "#f2701f", "#ffd23c", "#fff3b0"], seed: 2 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), ph = (t / meta.duration) * TAU;
  const bob = Math.round(Math.sin(ph));
  const tip = 5 + Math.sin(ph * 2) * 1.2, sway = Math.sin(ph * 3) * 0.8;
  p.circle(0, bob, 3.5, C[1]);
  p.tri(-3, bob - 1, 3, bob - 1, sway, bob - tip - 2, C[1]);
  p.circle(0, bob + 0.5, 2.5, C[2]);
  p.tri(-2, bob - 0.5, 2, bob - 0.5, sway * 0.6, bob - tip, C[3]);
  p.circle(0, bob + 1, 1.2, C[4]);
  cycle(t, meta.duration, 3, o.seed, 0.35, (age, life, r) => {
    const k = age / life;
    p.px(r.range(-2, 2) + Math.sin(k * 6) * 0.8, bob - 4 - k * 6, k < 0.5 ? C[3] : C[1]);
  });
}
