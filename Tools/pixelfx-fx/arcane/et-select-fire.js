// Elemental Trials: 불 스킬 선택. 발밑에서 원소색 얇은 띠가 빠르게 한 번 퍼지고 옅어진다. 원점이 발밑.
// 같은 종류(차징·풀차지·선택 띠·포인트 마법진)끼리는 색만 다르고 그리는 코드가 같다. 모양을 바꾸면 같은 종류 파일도 같이 고친다.
import { ramp, fit, seg, ease, TAU } from "../../core/index.js";

const FLAT = 0.55;

export const meta = {
  name: "ET select fire",
  desc: "Selecting the fire skill: a thin fire-coloured ring races out once from under the feet and fades.",
  tags: ["fire", "select", "ring", "elemental-trials"],
  size: [100, 60],
  origin: [50, 30],
  duration: 0.4,
  loop: false,
  fps: 20,
  bg: "dark",
  options: { palette: ["#701a14","#cc3a1a","#f2701f","#ffd23c","#fff3b0"], seed: 13 },
};

function ring(p, r, c, a) {
  let prev = null;
  for (let i = 0; i <= 64; i++) {
    const ang = (i / 64) * TAU, pt = [Math.cos(ang) * r, Math.sin(ang) * r * FLAT];
    if (prev) p.line(prev[0], prev[1], pt[0], pt[1], c, a);
    prev = pt;
  }
}

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5);
  const k = seg(t, 0, 0.36);
  if (k >= 1) return;
  const r = 6 + ease.outCubic(k) * 40, fade = 1 - ease.inQuad(k);
  ring(p, r, k < 0.25 ? C[4] : k < 0.6 ? C[3] : C[2], fade);
  if (r > 10) ring(p, r - 3, C[1], fade * 0.5);
}
