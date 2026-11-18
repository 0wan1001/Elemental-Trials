// Elemental Trials: 오른쪽 부채꼴로 물을 촤악 뿌린다. 손끝에서 물이 터지고, 물덩이들이 늘어나며 크기가 제각각인 물방울로 끊어져
// 흩어지고, 떨어진 자리마다 작게 튀며, 옅은 물안개가 남는다. 앞쪽 끝은 일정하지 않다. 빛 번짐 없음.
// 크기는 물 스킬 판정에 맞춘다: 사거리 4.5칸(PPU 32 → 144px), 반각 35°(0.61rad). 게임에서 마우스 방향으로 회전한다.
import { ramp, fit, burst, seg, ease } from "../../core/index.js";

const RANGE = 144;
const HALF = 0.61;

export const meta = {
  name: "ET water cone",
  desc: "Water flung right in a wide fan like a thrown bucket: a white burst at the hand, stretched clumps that break into uneven droplets, a ragged front, little splashes where they land and a drifting mist.",
  tags: ["water", "spray", "cone", "elemental-trials"],
  size: [156, 172],
  origin: [4, 86],
  duration: 0.55,
  loop: false,
  fps: 12,
  bg: "dark",
  options: { palette: ["#123a7a", "#1f6ac8", "#3ea4ea", "#8ad8f8", "#e8fbff"], seed: 3 },
};

const at = (a, d) => [Math.cos(a) * d, Math.sin(a) * d];

// 가운데로 몰리는 각도 (두 난수 평균)
const aim = (r) => ((r.next() + r.next()) - 1) * HALF;

// 진행 방향으로 늘어진 물덩이: 앞이 밝고 둥글며 뒤로 갈수록 가늘고 어둡다
function clump(p, x, y, a, len, rad, C) {
  const n = Math.max(2, Math.round(len / 2));
  for (let i = n; i >= 0; i--) {
    const q = i / n, cx = x - Math.cos(a) * len * q, cy = y - Math.sin(a) * len * q;
    p.circle(cx, cy, Math.max(0.6, rad * (1 - q * 0.6)), q > 0.6 ? C[1] : C[2]);
  }
  p.circle(x, y, rad, C[2]);
  p.px(x - 0.5, y - rad * 0.5, C[4]);
}

export function draw(p, t, o) {
  const C = fit(ramp(o.palette), 5);

  // 손끝에서 터지는 물
  if (t < 0.12) {
    const k = seg(t, 0, 0.12);
    burst(7, o.seed + 40, (r) => {
      const a = aim(r) * 1.4, d = 3 + k * r.range(6, 12);
      const [x, y] = at(a, d);
      p.circle(x + 4, y, r.range(2, 3.5) * (1 - k * 0.5), k < 0.5 ? C[4] : C[3]);
    });
  }

  // 물덩이: 늘어나며 날아가다 2~3개 물방울로 끊어지고, 끝에서 튄다
  burst(36, o.seed, (r) => {
    const a = aim(r), born = r.range(0, 0.07), life = r.range(0.24, 0.36);
    const reach = r.range(0.55, 1) * RANGE, rad = r.range(2.2, 3.8);
    const k = seg(t, born, born + life);
    if (t < born) return;
    if (k < 1) {
      const d = 6 + ease.outQuad(k) * (reach - 6);
      const [x, y] = at(a, d);
      if (k < 0.45) {
        clump(p, x, y, a, 4 + k * 22, rad, C);
      } else {
        // 끊어진 물방울: 조금씩 다른 각도와 거리
        const pieces = 2 + (r.next() < 0.5 ? 1 : 0);
        for (let j = 0; j < pieces; j++) {
          const da = (j - (pieces - 1) / 2) * 0.03 * (k - 0.3), dd = -j * (3 + k * 6);
          const [px, py] = at(a + da, d + dd);
          const pr = rad * (1 - j * 0.3) * (1 - (k - 0.45) * 0.6);
          p.circle(px, py, Math.max(0.6, pr), j === 0 ? C[3] : C[2]);
          if (j === 0) p.px(px - 0.5, py - pr * 0.5, C[4]);
        }
      }
      return;
    }
    // 떨어진 자리: 작은 물결 고리와 튀는 점
    const sk = seg(t, born + life, born + life + 0.14);
    if (sk >= 1) return;
    const [x, y] = at(a, reach);
    p.ellipse(x, y, 2 + sk * 4, 1 + sk * 2, sk < 0.5 ? C[3] : C[1], 1 - sk);
    for (let j = 0; j < 3; j++) {
      const ja = a + (j - 1) * 1.1, jd = 1 + ease.outQuad(sk) * 5;
      p.px(x + Math.cos(ja) * jd, y + Math.sin(ja) * jd, sk < 0.6 ? C[4] : C[2]);
    }
  });

  // 가는 물보라: 아주 작은 물방울이 제각각 속도로
  burst(70, o.seed + 9, (r) => {
    const a = aim(r) * 1.05, born = r.range(0.02, 0.1), life = r.range(0.2, 0.4);
    const k = seg(t, born, born + life);
    if (t < born || k >= 1) return;
    const d = 8 + ease.outCubic(k) * r.range(0.3, 1.05) * RANGE;
    const [x, y] = at(a, d);
    p.px(x, y, k < 0.4 ? C[4] : k < 0.75 ? C[3] : C[1]);
  });

  // 지나간 자리에 남는 옅은 물안개
  burst(14, o.seed + 21, (r) => {
    const a = aim(r), d = r.range(0.2, 0.8) * RANGE, born = 0.12 + (d / RANGE) * 0.12;
    const k = seg(t, born, 0.55);
    if (t < born || k >= 1) return;
    const [x, y] = at(a, d + k * 6);
    p.circle(x, y, 2 + k * 3, C[1], 0.35 * (1 - k));
  });
}
