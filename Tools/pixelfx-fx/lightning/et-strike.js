// Elemental Trials: 하늘에서 비스듬히 꽂히는 금빛 낙뢰. 흰 심에 금빛, 바깥에 살짝 하늘색 빛이 도는 굵은 번개(번개 차징과 같은 색 조합)가 잔가지를 뻗으며 두 번 번쩍이고,
// 꽂힌 자리에서 지그재그 번개가 부채꼴로 위·옆으로 튀어 오른 뒤 사라진다. 빛 번짐 없음.
// 원점이 꽂히는 지점. 바닥 번개가 뻗는 폭은 타격 판정 반지름 1.25칸(PPU 32 → 40px)에 맞춘다.
import { ramp, fit, rng, bolt, forks, burst, fly, seg, ease } from "../../core/index.js";

export const meta = {
  name: "ET lightning strike",
  desc: "A thick golden bolt with a white core and a faint sky-blue rim slams down at a slant from the sky, shedding many fine forks; it flickers once with a new shape, then jagged golden arcs fan up and out from the impact point and snap away.",
  tags: ["lightning", "strike", "elemental-trials"],
  size: [124, 196],
  origin: [64, 176],
  duration: 0.5,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#3a52c8", "#7cc8ff", "#ffd23c", "#fff08a", "#ffffff"], seed: 4 },
};

function path(p, pts, c, dx = 0, dy = 0) {
  for (let i = 1; i < pts.length; i++) p.line(pts[i - 1][0] + dx, pts[i - 1][1] + dy, pts[i][0] + dx, pts[i][1] + dy, c);
}

// 굵은 번개: 바깥 하늘색 → 금 → 흰 심
function thickBolt(p, pts, C, w) {
  if (w >= 3) { path(p, pts, C[1], -2); path(p, pts, C[1], 2); }
  if (w >= 2) { path(p, pts, C[2], -1); path(p, pts, C[2], 1); }
  path(p, pts, w >= 2 ? C[4] : C[3]);
}

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5);

  // 떨어지는 번개: 0프레임 굵게, 1프레임 모양을 바꿔 다시 번쩍, 2프레임 가는 잔상
  const flash = t < 0.05 ? 0 : t < 0.13 ? 1 : t < 0.21 ? 2 : -1;
  if (flash >= 0) {
    const r = rng(o.seed, flash === 2 ? 1 : flash + 10);
    const main = bolt(r, -34 + r.range(-6, 6), -170, 0, 0, { jag: 0.2, depth: 5 });
    thickBolt(p, main, C, flash < 2 ? 3 : 1);
    if (flash < 2) {
      for (const f of forks(r, main, { count: 7, len: 30, jag: 0.4, spread: 1.1 })) {
        path(p, f, C[2]);
        path(p, f, C[1], 0, -1);
        for (const g of forks(r, f, { count: 1, len: 12, jag: 0.5 })) path(p, g, C[2]);
      }
    }
  }

  // 타격 끝: 꽂힌 자리에서 지그재그 번개가 부채꼴로 튀어 오른다 (프레임마다 모양이 바뀌며 짧아진다)
  const ik = seg(t, 0.03, 0.36);
  if (ik > 0 && ik < 1) {
    const grow = ik < 0.25 ? ease.outCubic(ik / 0.25) : 1, shrink = 1 - ease.inQuad(seg(ik, 0.45, 1));
    const r = rng(o.seed, 30 + Math.floor(t * 12));
    for (let i = 0; i < 8; i++) {
      const side = i % 2 ? 1 : -1;
      // 안쪽(거의 위)일수록 길고, 바깥(옆)일수록 짧다 → V자 부채꼴
      const a = -Math.PI / 2 + side * (0.3 + (i >> 1) * 0.32) + r.range(-0.08, 0.08);
      const len = (46 - (i >> 1) * 7 + r.range(-4, 4)) * grow * shrink;
      if (len < 3) continue;
      const pts = bolt(r, side * 2, 0, Math.cos(a) * len, Math.sin(a) * len, { jag: 0.3, depth: 3 });
      thickBolt(p, pts, C, ik < 0.4 ? 2 : 1);
      if (ik < 0.5) for (const f of forks(r, pts, { count: 1, len: 9, jag: 0.5 })) path(p, f, C[2]);
    }
    if (ik < 0.35) {
      p.ellipse(0, 0, 9 - ik * 10, 3, C[3]);
      p.ellipse(0, 0, 4, 1.5, C[4]);
    }
  }

  // 튀는 금빛 불꽃
  burst(14, o.seed + 3, (r) => {
    const a = r.range(-Math.PI, 0), sp = r.range(90, 160), life = r.range(0.2, 0.32);
    if (t > life + 0.05 || t < 0.05) return;
    const [x, y] = fly(0, -1, Math.cos(a) * sp, Math.sin(a) * sp, t - 0.05, 400, 3);
    p.px(x, y, (t - 0.05) / life < 0.5 ? C[4] : r.next() < 0.5 ? C[1] : C[2]);
  });
}
