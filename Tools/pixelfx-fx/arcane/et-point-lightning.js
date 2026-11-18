// Elemental Trials: 번개 스킬 발사 지점의 아주 작은 포인트 마법진. 번쩍 열려 돌다가 사라진다. 원점이 마우스 지점.
// 같은 종류(차징·풀차지·선택 띠·포인트 마법진)끼리는 색만 다르고 그리는 코드가 같다. 모양을 바꾸면 같은 종류 파일도 같이 고친다.
import { ramp, fit, seg, ease, TAU } from "../../core/index.js";

const FLAT = 0.6;

export const meta = {
  name: "ET point lightning",
  desc: "A tiny lightning target sigil at the aimed point: it pops open with a flash, turns once and fades.",
  tags: ["lightning", "magic-circle", "target", "elemental-trials"],
  size: [24, 18],
  origin: [12, 9],
  duration: 0.45,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#3a52c8","#7cc8ff","#ffd23c","#fff08a","#ffffff"], seed: 14 },
};

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5);
  const open = ease.outBack(seg(t, 0, 0.1)), fade = 1 - seg(t, 0.25, 0.45);
  if (fade <= 0) return;
  const R = 8 * open, rot = t * 6;
  if (t < 0.09) p.ellipse(0, 0, 4, 3, C[4]);
  let prev = null;
  for (let i = 0; i <= 32; i++) {
    const a = (i / 32) * TAU, pt = [Math.cos(a) * R, Math.sin(a) * R * FLAT];
    if (prev) p.line(prev[0], prev[1], pt[0], pt[1], C[3], fade);
    prev = pt;
  }
  for (let i = 0; i < 4; i++) {
    const a = rot + (i / 4) * TAU + 0.785;
    p.px(Math.cos(a) * (R + 2), Math.sin(a) * (R + 2) * FLAT, C[4], fade);
  }
  for (let i = 0; i < 4; i++) { const a = rot + i * 1.571; p.line(0, 0, Math.cos(a) * 4, Math.sin(a) * 4 * FLAT, i % 2 ? C[3] : C[4], fade); }
}
