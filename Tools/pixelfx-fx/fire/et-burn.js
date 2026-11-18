// Elemental Trials: 화상 상태 (반복). 맞은 대상의 몸 둘레에 작은 불꽃이 번갈아 붙어 타오르고 연기와 불씨가 올라간다.
// 플레이어 몸(32px) 기준 크기. 큰 대상은 프리팹을 정수 배로 키워서 붙인다. 원점이 몸 중심.
import { ramp, fit, cycle, TAU } from "../../core/index.js";

export const meta = {
  name: "ET burn",
  desc: "Burning status: small flames cling to the body at a few spots and flare in turn, grey smoke curls and embers drift up.",
  tags: ["fire", "status", "burn", "elemental-trials"],
  size: [32, 40],
  origin: [16, 24],
  duration: 0.8,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#701a14", "#cc3a1a", "#f2701f", "#ffd23c", "#fff3b0"], smoke: ["#3a3644", "#6b6577"], seed: 31 },
};

function flame(p, x, y, h, C) {
  p.tri(x - 2, y, x + 2, y, x, y - h, C[2]);
  p.tri(x - 1, y, x + 1, y, x, y - h * 0.6, C[3]);
  p.dot(x, y - 1, C[4], 1);
}

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), S = fit(ramp(o.smoke), 2), ph = t / meta.duration;
  // 몸 둘레 불꽃 다섯 자리 (어깨·허리·다리 쪽에 고르게)
  const SPOTS = [[-7, -2], [6, -4], [-4, 7], [3, 4], [8, 10]];
  for (let i = 0; i < 5; i++) {
    const [x, y] = SPOTS[i];
    const k = (Math.sin((ph * 2 + i / 5) * TAU) + 1) / 2;
    if (k > 0.25) flame(p, x, y, 3 + k * 5, C);
  }
  // 연기
  cycle(t, meta.duration, 4, o.seed + 1, 0.6, (age, life, q) => {
    const k = age / life, x = q.range(-6, 6) + Math.sin(k * 5) * 1.5, y = -6 - k * 14;
    p.circle(x, y, 1.5 + k * 1.5, k < 0.5 ? S[1] : S[0], 0.6 * (1 - k));
  });
  // 불씨
  cycle(t, meta.duration, 5, o.seed + 2, 0.45, (age, life, q) => {
    const k = age / life;
    p.dot(q.range(-8, 8), 6 - k * 22, k < 0.5 ? C[4] : C[2], 1);
  });
}
