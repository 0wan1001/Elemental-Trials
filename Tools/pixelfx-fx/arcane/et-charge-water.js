// Elemental Trials: 물 차징 (반복). 지팡이 끝 앞 한 점으로 기가 빨려 들어 작은 구슬이 맥동한다. 원점이 모이는 점.
// 같은 종류(차징·풀차지·선택 띠·포인트 마법진)끼리는 색만 다르고 그리는 코드가 같다. 모양을 바꾸면 같은 종류 파일도 같이 고친다.
import { ramp, fit, cycle, rng, ease, TAU } from "../../core/index.js";

export const meta = {
  name: "ET charge water",
  desc: "water energy gathering at the staff tip: motes streak inward from all sides into a small pulsing core.",
  tags: ["water", "charge", "elemental-trials"],
  size: [44, 44],
  origin: [22, 22],
  duration: 0.5,
  loop: true,
  fps: 12,
  bg: "dark",
  options: { palette: ["#123a7a","#1f6ac8","#3ea4ea","#8ad8f8","#e8fbff"], kind: "water", seed: 11 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5), ph = (t / meta.duration) * TAU;
  // 빨려 드는 기: 바깥에서 중심으로, 꼬리를 남긴다
  cycle(t, meta.duration, 16, o.seed, 0.38, (age, life, r) => {
    const k = age / life, spin = o.kind === "water" ? k * 2.4 : o.kind === "fire" ? k * 0.8 : 0;
    const a = r.range(0, TAU) + spin, d = 3 + (1 - ease.inQuad(k)) * r.range(11, 18);
    const x = Math.cos(a) * d, y = Math.sin(a) * d - (o.kind === "fire" ? k * 2 : 0);
    const tail = 2 + (1 - k) * 3, tx = x + Math.cos(a) * tail, ty = y + Math.sin(a) * tail;
    if (o.kind === "lightning") {
      const q = rng(o.seed, Math.floor(t * 24) + r.next() * 99 | 0);
      const mx = (x + tx) / 2 + q.range(-1.5, 1.5), my = (y + ty) / 2 + q.range(-1.5, 1.5);
      p.line(tx, ty, mx, my, C[1]);
      p.line(mx, my, x, y, k < 0.5 ? C[2] : C[3]);
    } else {
      p.line(tx, ty, x, y, C[1]);
      p.px(x, y, k < 0.5 ? C[3] : C[4]);
    }
  });
  // 가운데 구슬: 모일수록 맥동
  const pr = 2.2 + Math.sin(ph * 2) * 0.7;
  p.circle(0, 0, pr + 1.5, C[1]);
  p.circle(0, 0, pr, C[3]);
  p.circle(-0.5, -0.5, Math.max(0.8, pr * 0.45), C[4]);
}
