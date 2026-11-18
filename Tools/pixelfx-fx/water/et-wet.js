// Elemental Trials: 젖음 상태 (반복). 몸 둘레에 물방울이 맺혀 부풀다가 아래로 떨어지고, 발밑에 물 고리가 퍼진다.
// 몸 전체의 파란 덮개는 게임 코드(SpriteFlash.SetTint)가 따로 그린다. 여기서는 물방울만 그린다.
// 플레이어 몸(32px) 기준 크기. 큰 대상은 프리팹을 정수 배로 키워서 붙인다. 원점이 몸 중심.
import { ramp, fit, cycle } from "../../core/index.js";

export const meta = {
  name: "ET wet",
  desc: "Wet status: big water beads form on the body, swell and drip down, a ripple ring spreads at the feet.",
  tags: ["water", "status", "wet", "elemental-trials"],
  size: [32, 40],
  origin: [16, 24],
  duration: 1.2,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#1d3f6e", "#2f6fb5", "#5fa8e8", "#a8dcff", "#ffffff"], seed: 17 },
};

// 2px 폭 물방울: 위는 밝은 반사, 아래는 짙은 그림자
function bead(p, x, y, len, C) {
  for (let i = 0; i < len; i++) {
    const c = i === len - 1 ? C[1] : C[2];
    p.dot(x, y + i, c, 1);
    p.dot(x + 1, y + i, i === 0 ? C[4] : C[3], 1);
  }
}

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5);
  // 맺혔다가 떨어지는 물방울: 앞 40%는 제자리에서 부풀고, 나머지는 아래로 떨어지며 늘어난다
  cycle(t, meta.duration, 9, o.seed, 0.9, (age, life, q) => {
    const k = age / life, x = Math.round(q.range(-9, 8)), y0 = Math.round(q.range(-11, 6));
    if (k < 0.4) {
      bead(p, x, y0, k < 0.2 ? 1 : 2, C);
    } else {
      const f = (k - 0.4) / 0.6, y = Math.round(y0 + f * f * 18);
      bead(p, x, y, 3, C);
    }
  });
  // 몸 표면 물기 반짝임
  cycle(t, meta.duration, 5, o.seed + 1, 0.35, (age, life, q) => {
    const k = age / life, x = Math.round(q.range(-8, 8)), y = Math.round(q.range(-11, 8));
    p.dot(x, y, k < 0.5 ? C[4] : C[3], 1);
    if (k < 0.5) { p.dot(x - 1, y, C[3], 1); p.dot(x + 1, y, C[3], 1); }
  });
  // 발밑 물 고리: 퍼지면서 옅어진다
  cycle(t, meta.duration, 2, o.seed + 2, 0.8, (age, life, q) => {
    const k = age / life, rx = 3 + k * 8, ry = 1 + k * 2, cx = Math.round(q.range(-2, 2)), cy = 12;
    const c = k < 0.5 ? C[3] : C[2];
    for (let a = 0; a < 24; a++) {
      const th = (a / 24) * Math.PI * 2;
      p.dot(Math.round(cx + Math.cos(th) * rx), Math.round(cy + Math.sin(th) * ry), c, 1);
    }
  });
}
