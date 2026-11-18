// Elemental Trials: 텔레포트(대시) 잔상 (1회). 출발점에서 대시 방향(오른쪽)으로 보라색 띠가 길게 뻗고, 띠 안에 가로 속도선이 흘러 지나간 뒤
// 뒤쪽부터 걷히듯 사라진다. 앞쪽(도착 쪽)이 밝고 굵다. 원점이 출발점(몸 중심 높이). 길이는 대시 거리 약 67px.
import { ramp, fit, burst, seg, ease } from "../../core/index.js";

const LEN = 67;

export const meta = {
  name: "ET teleport streak",
  desc: "A dash afterimage: a violet band shoots right from the start point, thicker and brighter at the arrival end, speed lines rushing through it, then it peels away from the back.",
  tags: ["arcane", "teleport", "dash", "elemental-trials"],
  size: [80, 28],
  origin: [4, 14],
  duration: 0.35,
  loop: false,
  fps: 20,
  bg: "dark",
  options: { palette: ["#2a1450", "#5a2a9a", "#8a5ae0", "#c0a0ff", "#f4ecff"], seed: 39 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5);
  const grow = ease.outCubic(seg(t, 0, 0.06));
  const tail = ease.inQuad(seg(t, 0.06, 0.33));
  const x0 = tail * LEN, x1 = grow * LEN;
  if (x1 - x0 < 1) return;
  // 띠: 뒤로 갈수록 가늘고 어둡다
  for (let x = Math.floor(x0); x <= x1; x++) {
    const k = x / LEN, h = 1 + k * 7;
    for (let y = -Math.round(h); y <= Math.round(h); y++) {
      const edge = Math.abs(y) >= h - 1;
      p.dot(x, y, edge ? C[1] : k > 0.8 ? C[3] : C[2], edge ? 0.6 : 0.75);
    }
  }
  // 속도선
  burst(9, o.seed, (r) => {
    const y = Math.round(r.range(-6, 6)), len = r.range(10, 24), x = ((t * r.range(250, 400)) % (LEN + len)) + x0 - len;
    p.line(Math.max(x0, x), y, Math.min(x1, x + len), y, Math.abs(y) < 3 ? C[4] : C[3], 0.9);
  });
  // 앞쪽 밝은 끝
  if (t < 0.2) { p.ellipse(x1, 0, 3, 7, C[3], 0.9); p.ellipse(x1, 0, 1.5, 4, C[4]); }
  // 흩어지는 반짝임
  burst(6, o.seed + 1, (r) => {
    const born = r.range(0.05, 0.2), k = seg(t, born, born + 0.12);
    if (t < born || k >= 1) return;
    p.dot(r.range(0, LEN), r.range(-9, 9) - k * 3, k < 0.5 ? C[4] : C[2], 1);
  });
}
